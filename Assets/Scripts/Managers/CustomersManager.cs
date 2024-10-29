using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps track of the currently existing customers in the game, and deals with state changes such as becoming angry
/// or death.
/// </summary>
public class CustomersManager: MonoBehaviour
{
    private readonly List<Customer> customers = new List<Customer>();
    private int expectedCount;

    private int _handledCount;
    private int HandledCount
    {
        get => _handledCount;
        set
        {
            _handledCount = value;
            if (_handledCount == expectedCount)
            {
                OnAllCustomersHandled?.Invoke();
            }
        }
    }
    
    // Events
    public event Action<Customer> OnCustomerAdded;
    public event Action<Customer, bool> OnOrderStatusChanged; // Indicates that an order has expired or been completed
    public event Action OnReset;
    public event Action OnAllCustomersHandled; // Called when all customers have been fed or killed, use for early exit

    /// <summary>
    /// Configure the total amount of customers that will be spawned for the current round.
    /// </summary>
    /// <param name="expectedCustomers">The number of customers</param>
    public void Configure(int expectedCustomers)
    {
        expectedCount = expectedCustomers;
        HandledCount = 0;
    }
    
    public void Add(Customer customer)
    {
        customers.Add(customer);
        OnCustomerAdded?.Invoke(customer);
        
        // Listen for state changes
        customer.OnBecameAngry += HandleBecameAngry;
        customer.OnFed += HandleFed;
        customer.OnDeath += HandleDeath;
    }

    private void Cleanup(Customer customer)
    {
        customer.OnBecameAngry -= HandleBecameAngry;
        customer.OnFed -= HandleFed;
        customer.OnDeath -= HandleDeath;
    }

    private void HandleBecameAngry(Customer customer)
    {
        OnOrderStatusChanged?.Invoke(customer, false);
    }

    private void HandleFed(Customer customer)
    {
        HandledCount++;
        OnOrderStatusChanged?.Invoke(customer, true);
        Cleanup(customer);
    }

    private void HandleDeath(Customer customer)
    {
        HandledCount++;
        Cleanup(customer);
    }

    public void Reset()
    {
        foreach (var customer in customers)
        {
            if (customer != null)
            {
                Cleanup(customer);    
                Destroy(customer.gameObject);
            }
        }
        customers.Clear();
        OnReset?.Invoke();
    }
}
