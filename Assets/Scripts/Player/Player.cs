using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject pizzaSlicePrefab;
    [SerializeField] private GameObject pizzaBoxPrefab;
    [SerializeField] private PauseMenu pauseMenu;
    private Animator animator;
    public bool IsGamePaused { get; private set; } = false;

    [Header("Input Actions")]
    [SerializeField] private InputAction moveControls;
    [SerializeField] private InputAction lookControls;
    [SerializeField] public InputAction shootControls;
    [SerializeField] public InputAction eatControls;
    [SerializeField] public InputAction swapForwardControls;
    [SerializeField] public InputAction swapBackwardControls;

    [Header("Movement")]
    private float moveSpeed = 3000; //old was 3500
    [SerializeField] private float maxSpeed = 20;
    [SerializeField] private float counterMovement = 0.175f;
    private Rigidbody rb;
    [SerializeField] private float sensitivity = 50f;
    private float horizontalRotation = 0.0f;
    private float threshold = 0.01f;
    private float timer = 0.00f;
    private float x, y;

    [Header("Throwing")]
    [SerializeField] private float throwForce;
    [SerializeField] private float throwUpwardForce;
    [SerializeField] private int maxThrows;
    [SerializeField] private float throwCooldown;
    private int currentThrows;
    private bool readyToThrow;

    [Header("Stats")]
    private PlayerHealth playerHealth;
    public PlayerInventory playerInventory;
    public static int money = 0;
    [SerializeField] private int baseAttack = 15;
    private List<IEffect> activeEffects = new List<IEffect>();

    [Header("Audio")]
    [SerializeField] private AudioClip sliceSound;
    [SerializeField] private AudioClip pizzaSound;
    [SerializeField] private AudioClip swapSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip consumeSound;

    private void OnEnable()
    {
        moveControls.Enable();
        lookControls.Enable();
        shootControls.Enable();
        shootControls.performed += FirePerformed;
        shootControls.canceled += FireCanceled;
        eatControls.Enable();
        eatControls.performed += Consume;
        swapForwardControls.Enable();
        swapForwardControls.performed += SwapForward;
        swapBackwardControls.Enable();
        swapBackwardControls.performed += SwapBackward;
    }
    public void SetPauseState(bool isPaused)
    {
        IsGamePaused = isPaused;
    }
    private void OnDisable()
    {
        moveControls.Disable();
        lookControls.Disable();
        shootControls.Disable();
        eatControls.Disable();
        swapForwardControls.Disable();
        swapBackwardControls.Disable();
    }

    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = newSensitivity;
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        readyToThrow = true;
        currentThrows = maxThrows;
        rb = GetComponent<Rigidbody>();
        playerInventory = GetComponent<PlayerInventory>();
        playerHealth = GetComponent<PlayerHealth>();
        playerInventory.InitializeInventory();

        // Load saved sensitivity on start
        float savedSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", 100f);
        SetSensitivity(savedSensitivity);
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Update()
    {
        if (IsGamePaused) return;
        playerCam.position = transform.position + new Vector3(0, 0.5f, 0);
        if (Time.timeScale == 0) return;
        Look();
    }

    private void FirePerformed(InputAction.CallbackContext context)
    {
        if (readyToThrow && maxThrows > 0 && Time.timeScale != 0)
            timer = Time.time;
    }

    private void FireCanceled(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0 || playerInventory.GetPizzaAmmo(playerInventory.GetEquippedPizza()) < 1) return;
        animator.SetTrigger("IsThrowing");

        if (Time.time - timer < 1.00f)
        {
            GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(sliceSound);
            Throw(playerInventory.GetEquippedPizza(), false);
        }
        else
        {
            if (playerInventory.GetPizzaAmmo(playerInventory.GetEquippedPizza()) >= 8)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(pizzaSound);
                Throw(playerInventory.GetEquippedPizza(), true);
            }
            else
                Throw(playerInventory.GetEquippedPizza(), false);
        }
    }

    private void Consume(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0) return;
        animator.SetTrigger("IsEating");
        GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(consumeSound);
        var pizzaType = playerInventory.GetEquippedPizza();
        if (playerInventory.GetPizzaAmmo(pizzaType) == 0) return;
        IPizza pizza = PizzaFactory.CreatePizza(pizzaType, baseAttack);
        playerHealth.Heal(pizza.Healing);
        playerInventory.LosePizzas(1);
        StartCoroutine(StartEffect(pizza.PlayerEffect));
    }

    private void SwapForward(InputAction.CallbackContext context)
    {
        if (pauseMenu.isPaused || GameManager.Instance.isPaused) return;
        GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(swapSound);
        playerInventory.SwitchPizzaForward();
    }

    private void SwapBackward(InputAction.CallbackContext context)
    {
        if (pauseMenu.isPaused || GameManager.Instance.isPaused) return;
        GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(swapSound);
        playerInventory.SwitchPizzaBackward();
    }

    private void Movement()
    {
        if (IsGamePaused) return;
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

        //Apply forces to move player
        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Look()
    {
        if (IsGamePaused) return;
        Vector2 moveInput = moveControls.ReadValue<Vector2>();
        x = moveInput.x;
        y = moveInput.y;
        float currentSensitivity = lookControls.activeControl?.device is Gamepad ? sensitivity : sensitivity * 0.25f;
        horizontalRotation += lookControls.ReadValue<float>() * currentSensitivity * Time.fixedDeltaTime;
        playerCam.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        orientation.localRotation = playerCam.localRotation;
        transform.Find("Model Container").localRotation = playerCam.localRotation;//new Quaternion(0f, playerCam.localRotation.y, transform.Find("Model").localRotation.z, transform.Find("Model").localRotation.w);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
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

    private void Throw(PizzaType pizzaType, bool isBox)
    {
        // TODO: Pizza creation logic could be moved to factory
        Debug.Log("Pizza type to throw: " + pizzaType + "Current ammo: " + playerInventory.GetPizzaAmmo(playerInventory.GetEquippedPizza()));

        if (playerInventory.GetPizzaAmmo(playerInventory.GetEquippedPizza()) > 0)
        {
            readyToThrow = false;
            var prefab = isBox ? pizzaBoxPrefab : pizzaSlicePrefab;
            var quantity = isBox ? 8 : 1;
            var pizzaToThrow = CreatePizzaToThrow(playerInventory.GetEquippedPizza(), quantity);
            //TODO Create Pizza to throw
            playerInventory.LosePizzas(quantity);

            var projectile = Instantiate(prefab, attackPoint.position, playerCam.GetChild(0).rotation);
            projectile.GetComponent<ThrowablePizza>().Initialize(pizzaToThrow);
            var projectileRb = projectile.GetComponent<Rigidbody>();
            var forceDirection = playerCam.GetChild(0).transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(playerCam.GetChild(0).position, playerCam.GetChild(0).forward, out hit, 500f))
                forceDirection = (hit.point - attackPoint.position).normalized;
            Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
            projectileRb.AddForce(forceToAdd, ForceMode.Impulse);
            currentThrows--;
            Invoke(nameof(ResetThrow), throwCooldown);
        }
    }

    private IPizza CreatePizzaToThrow(PizzaType pizzaTypeToThrow, int quantity)
    {
        Debug.Log("Pizza of type: " + pizzaTypeToThrow + " will be created to throw.");
        return PizzaFactory.CreatePizza(pizzaTypeToThrow, baseAttack, quantity);
    }

    public void Restocking()
    {
        // Restocking pizzas to max order amount
        playerInventory.RestockPizzas();
    }

    private void ResetThrow()
    {
        currentThrows = maxThrows;
        readyToThrow = true;
    }

    private IEnumerator StartEffect(IEffect effect)
    {
        if (effect == null) yield break;

        // Check for existing effect
        var existingEffect =
            activeEffects.FirstOrDefault(e => e.Type == effect.Type);
        if (existingEffect != null)
        {
            existingEffect.Duration = Math.Max(effect.Duration, existingEffect.Duration);
            yield break;
        }

        activeEffects.Add(effect);
        while (effect.Duration > 0)
        {
            ApplyEffect(effect);
            effect.Duration--;
            yield return new WaitForSeconds(1);
        }

        activeEffects.Remove(effect);
    }

    /// <summary>
    /// Handles the application of any given effect.
    /// </summary>
    /// <param name="effect"></param>
    private void ApplyEffect(IEffect effect)
    {
        // NOTE: Currently only handles Regen
        switch (effect.Type)
        {
            case EffectType.Regen:
                playerHealth.Heal(effect.Value);
                break;
        }
    }

    // TODO: Move attacking code to separate class
    public void IncreaseAttack(int amount)
    {
        baseAttack += amount;
    }

    public void IncreaseSpeed(int amount)
    {
        moveSpeed += amount;
    }
}
