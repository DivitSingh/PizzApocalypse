using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaSlice : MonoBehaviour
{

    // How much damage the pizzaSlice does
    public int damage;
    private Rigidbody rb_pizzaSlice;

    // Start is called before the first frame update
    private void Start()
    {
        rb_pizzaSlice = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Customer>() != null)
        {
            Customer currentCustomer = other.gameObject.GetComponent<Customer>();
            currentCustomer.ReceivePizza(damage);
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

}
