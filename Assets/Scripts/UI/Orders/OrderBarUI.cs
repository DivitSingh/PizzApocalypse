using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages displaying current orders with their statuses.
/// </summary>
public class OrderBarUI : MonoBehaviour
{
    [SerializeField] private ActiveCustomersManager customersManager;
    [SerializeField] private GameObject orderPrefab;

    private readonly Dictionary<Customer, GameObject> customerObjectMap = new Dictionary<Customer, GameObject>();

    private void Awake()
    {
        customersManager.OnCustomerAdded += AddOrder;
        customersManager.OnOrderStatusChanged += RemoveOrder;
    }

    private void AddOrder(Customer customer)
    {
        var container = Instantiate(orderPrefab, gameObject.transform);
        customerObjectMap[customer] = container;
        container.GetComponent<OrderUI>().Configure(customer);
    }

    private void RemoveOrder(Customer customer, bool success)
    {
        // TODO: Differentiate between success and fail
        var container = customerObjectMap[customer];
        Destroy(container);
    }
}
