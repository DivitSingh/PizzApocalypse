using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles keeping track of the customers currently waiting for orders and associating them with order numbers.
/// </summary>
public class ActiveCustomersManager: MonoBehaviour
{
    private List<Customer> customers = new List<Customer>();

    public void Add(Customer customer)
    {
        customers.Add(customer);
        
        // TODO: Set up delegates
        customer.OnBecameAngry += HandleBecameAngry;
        customer.OnFed += HandleFed;
    }

    private void Remove(Customer customer)
    {
        Debug.Log("Removing customer");
        customers.Remove(customer);
        
        customer.OnBecameAngry -= HandleBecameAngry;
        customer.OnFed -= HandleFed;
    }

    private void HandleBecameAngry(Customer customer)
    {
        Remove(customer);
        // TODO: Remove from UI with success animation?
    }

    private void HandleFed(Customer customer)
    {
        Remove(customer);
        // TODO: Remove from UI with failed animation?
    }
}