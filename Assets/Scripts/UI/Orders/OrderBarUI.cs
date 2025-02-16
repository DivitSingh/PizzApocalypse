using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages displaying current orders with their statuses.
/// </summary>
public class OrderBarUI : MonoBehaviour
{
    [SerializeField] private CustomersManager customersManager;
    [SerializeField] private GameObject orderPrefab;

    private readonly Dictionary<Customer, OrderUI> customerObjectMap = new Dictionary<Customer, OrderUI>();

    private void Awake()
    {
        customersManager.OnCustomerAdded += AddOrder;
        customersManager.OnOrderStatusChanged += RemoveOrder;
        customersManager.OnReset += Reset;
    }

    private void Start()
    {
        GameManager.Instance.OnGameOver += Reset;
    }

    private void AddOrder(Customer customer)
    {
        var container = Instantiate(orderPrefab, gameObject.transform);
        var orderScript = container.GetComponent<OrderUI>();
        customerObjectMap[customer] = orderScript;
        orderScript.Configure(customer);
    }

    private void RemoveOrder(Customer customer, bool success)
    {
        if (customerObjectMap.ContainsKey(customer))
        {
            var container = customerObjectMap[customer];
            customerObjectMap.Remove(customer);
            container.Remove(success);
        }
    }

    private void Reset()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        customerObjectMap.Clear();
    }
}
