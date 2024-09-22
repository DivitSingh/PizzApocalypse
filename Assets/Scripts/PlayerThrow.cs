using System.Collections.Generic;
using UnityEngine;

public class PlayerThrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private List<GameObject> pizzas;

    [Header("Settings")]
    [SerializeField] private int maxThrows;
    [SerializeField] private float throwCooldown;

    [Header("Throwing")]
    [SerializeField] private KeyCode throwKey = KeyCode.Mouse0;
    [SerializeField] private float throwForce;
    [SerializeField] private float throwUpwardForce;

    private bool readyToThrow;
    private int currentThrows;
    private float timer = 0.00f;

    private void Start()
    {
        readyToThrow = true;
        currentThrows = maxThrows;
    }

    private void Update()
    {
        if (Input.GetKeyDown(throwKey) && readyToThrow && maxThrows > 0)
        {
            timer = Time.time;
        }

        if (Input.GetKeyUp(throwKey))
        {
            if (Time.time - timer < 1.00f)
                Throw(pizzas[0]);
            else
                Throw(pizzas[1]);

        }
    }

    private void Throw(GameObject objectToThrow)
    {
        readyToThrow = false;

        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        Vector3 forceDirection = cam.transform.forward;
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        Vector3 forceToAdd = forceDirection * throwForce + transform.up * throwUpwardForce;
        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);
        currentThrows--;
        Invoke(nameof(ResetThrow), throwCooldown);
    }

    private void ResetThrow()
    {
        currentThrows = maxThrows;
        readyToThrow = true;
    }
}