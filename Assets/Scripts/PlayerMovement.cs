using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private List<GameObject> pizzas;

    // TODO: Should perhaps be replaced if we have different textures to map PizzaType to Prefab
    [SerializeField] private GameObject pizzaSlicePrefab;
    [SerializeField] private GameObject pizzaBoxPrefab;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4500;
    [SerializeField] private float maxSpeed = 20;
    [SerializeField] private float counterMovement = 0.175f;
    [SerializeField] private float maxSlopeAngle = 35f;
    [SerializeField] private bool grounded;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Throwing")]
    [SerializeField] private float throwForce;
    [SerializeField] private float throwUpwardForce;
    [SerializeField] private int maxThrows;
    [SerializeField] private float throwCooldown;

    [Header("PizzaAmo")]
    [SerializeField] private int maxPizzaAmo = 40;

    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;
    private float threshold = 0.01f;
    private float timer = 0.00f;
    private float x, y;
    private int currentThrows;
    private int currentPizzaAmo;
    private Vector3 normalVector = Vector3.up;
    private bool readyToThrow;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        readyToThrow = true;
        currentThrows = maxThrows;
        currentPizzaAmo = maxPizzaAmo;
        GameObject.Find("Canvas").GetComponentInChildren<TextMeshProUGUI>().text = currentPizzaAmo.ToString(); // Setting initial PizzaAmmo
    }


    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        playerCam.position = transform.position + new Vector3(0, 0.5f, 0);

        if (Time.timeScale == 0) return;

        MyInput();
        Look();

        if (Input.GetButtonDown("Fire1") && readyToThrow && maxThrows > 0)
        {
            timer = Time.time;
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (Time.time - timer < 1.00f)
            {
                // Throw(pizzas[0]);// Slice Pizza
                Throw(PizzaType.Cheese, false);
            }
            else
            {
                if (currentPizzaAmo >= 8)
                    // Throw(pizzas[1]);// Full Pizza, only thrown, when there are 8 or more pieces left.
                    Throw(PizzaType.Cheese, true);
                // TODO: Should probably refactor to only pass in whether thrown pizza is box or not
                // TODO: Let Throw method handle which type of Pizza is thrown
                else
                    // Throw(pizzas[0]);// Slice Pizza thrown instead when 7 or less slices are left.
                    Throw(PizzaType.Cheese, false);
            }
        }
    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
    }

    private void Movement()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        //Apply forces to move player
        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (Gamepad.current == null)
            playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        else
            playerCam.transform.Rotate(Input.GetAxis("Joystick X") * Vector3.up * Time.fixedDeltaTime * sensitivity);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded) return;

        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        if (Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2)) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    private void Throw(GameObject objectToThrow)
    {
        // Check if pizzas stocked
        if (currentPizzaAmo > 0)
        {
            readyToThrow = false;

            GameObject projectile = Instantiate(objectToThrow, attackPoint.position, playerCam.GetChild(0).rotation);
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            Vector3 forceDirection = playerCam.GetChild(0).transform.forward;
            RaycastHit hit;

            if (Physics.Raycast(playerCam.GetChild(0).position, playerCam.GetChild(0).forward, out hit, 500f))
                forceDirection = (hit.point - attackPoint.position).normalized;

            Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
            projectileRb.AddForce(forceToAdd, ForceMode.Impulse);
            currentThrows--;
            Invoke(nameof(ResetThrow), throwCooldown);

            // TODO: Call pizza deduct method
            LosingPizzas(objectToThrow);
            // GameObject.Find("Canvas").GetComponentInChildren<TextMeshProUGUI>().text = "Hi";

            // If pizza count < 0 you can't throw
        }
        // TODO? - Else, if Amo is empty, display text?

    }

    private void Throw(PizzaType pizzaType, bool isBox)
    {
        // TODO: Needs to check ammo for specific type of pizza
        // TODO: Pizza creation logic could be moved to factory
        if (pizzaType == PizzaType.Cheese)
        {
            readyToThrow = false;
            
            var prefab = isBox ? pizzaBoxPrefab : pizzaSlicePrefab;
            var quantity = isBox ? 8 : 1;
            var cheesePizza = new CheesePizza(quantity);

            var projectile = Instantiate(prefab, attackPoint.position, playerCam.GetChild(0).rotation);
            projectile.GetComponent<ThrowablePizza>().Initialize(cheesePizza);
            var projectileRb = projectile.GetComponent<Rigidbody>();
            var forceDirection = playerCam.GetChild(0).transform.forward;
            RaycastHit hit;
            
            if (Physics.Raycast(playerCam.GetChild(0).position, playerCam.GetChild(0).forward, out hit, 500f))
                forceDirection = (hit.point - attackPoint.position).normalized;

            Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
            projectileRb.AddForce(forceToAdd, ForceMode.Impulse);
            currentThrows--;
            Invoke(nameof(ResetThrow), throwCooldown);

            // TODO: Remove from appropriate PizzaType ammi
        }
    }

    public void LosingPizzas(GameObject objectThrown)
    {
        bool fullPizzaShot = false;
        if (objectThrown.name == "Pizza (Whole)") // Check if full pizza was thrown
            fullPizzaShot = true;

        // Deduct pizzas while shooting
        if (fullPizzaShot)
            SetCurrentPizzaAmo(-8); // Full Pizza thrown: 8 slices lost

        else
            SetCurrentPizzaAmo(-1); // Only a slice thrown: 1 slice lost
    }

    public int GetCurrentPizzaAmo()
    { return currentPizzaAmo; }
    public void SetCurrentPizzaAmo(int change)
    {
        if (currentPizzaAmo + change > 40)
            currentPizzaAmo = 40;
        else if (currentPizzaAmo + change < 0)
            currentPizzaAmo = 0;
        else
            currentPizzaAmo = currentPizzaAmo + change;
        GameObject.Find("Canvas").GetComponentInChildren<TextMeshProUGUI>().text = currentPizzaAmo.ToString();
    }

    public void Restocking()
    {
        // Restocking pizzas to max order amount
        SetCurrentPizzaAmo(maxPizzaAmo);
    }

    private void ResetThrow()
    {
        currentThrows = maxThrows;
        readyToThrow = true;
    }

}
