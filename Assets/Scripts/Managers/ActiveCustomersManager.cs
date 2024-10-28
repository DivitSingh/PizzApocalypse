using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles keeping track of the customers currently waiting for orders and associating them with order numbers.
/// </summary>
public class ActiveCustomersManager: MonoBehaviour
{
    // TODO: May not even need this?
    private List<Customer> customers = new List<Customer>();
    
    // TODO: Keep track of ids?
    
    // Events
    // TODO: On Customer added -> Notifies UI to add new grid element
    // TODO: On Customer Expired -> Notifies UI to remove new grid element
    // TODO: On Customer Fed -> Notifies UI to remove with positive animation

    public event Action<Customer> OnCustomerAdded;
    public event Action<Customer, bool> OnOrderStatusChanged; // Indicates that an order has expired or been completed

    public void Add(Customer customer)
    {
        customers.Add(customer);
        OnCustomerAdded?.Invoke(customer);
        
        // Listen for state changes
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
        OnOrderStatusChanged?.Invoke(customer, false);
        Remove(customer);
        // TODO: Remove from UI with success animation?
    }

    private void HandleFed(Customer customer)
    {
        OnOrderStatusChanged?.Invoke(customer, true);
        Remove(customer);
        // TODO: Remove from UI with failed animation?
    }
}
