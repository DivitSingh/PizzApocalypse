using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaSlice : MonoBehaviour
{

    // How much damage the pizzaSlice does
    private int damage = 1;
    private Rigidbody rb_pizzaSlice;

    private bool targetHit;


    // Start is called before the first frame update
    private void Start()
    {
        rb_pizzaSlice = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // check if target was already hit by the slice
        if(targetHit)
            return;
        else
            targetHit = true;

        // check if you hit an customer
        if (collision.gameObject.GetComponent<Customer>() != null)
        {
            Customer currentCustomer = collision.gameObject.GetComponent<Customer>();

            currentCustomer.ReceivePizza(damage);
        }

        // Pizza slices destroys itself 
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

}
