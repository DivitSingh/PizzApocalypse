using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private LayerMask customerLayerMask;
    [SerializeField] private Canvas healthBarCanvas; // TODO: Move this to Customer class?

    private Transform[] spawnPoints;
    private readonly float checkRadius = 2f;
    private readonly Random random = new Random();
    private IEnumerator spawnCoroutine;
    private int nextId = 1;

    public event Action<Customer> OnSpawned; 

    private void Awake()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
            .Select(s => s.transform)
            .ToArray();

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found! Make sure your spawn points are tagged with 'SpawnPoint'");
        }

        if (customerPrefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned in the CustomerSpawner!");
        }

        if (healthBarCanvas == null)
        {
            Debug.LogError("HealthBar Canvas cannot be null.");
        }
    }

    /// <summary>
    /// This method is called to begin the process of spawning customers.
    /// </summary>
    /// <param name="spawnInterval">Rate at which customers spawn.</param>
    /// <param name="totalSpawns">Total amount of customers to be spawned.</param>
    /// <param name="health">The starting health for a spawned customer.</param>
    /// <param name="patience">The patience value for a spawned customer.</param>
    /// <param name="attackDamage">The amount of damage a customer attack deals to the player.</param>
    public void StartSpawning(float spawnInterval, int totalSpawns, float health, float patience, float attackDamage)
    {
        nextId = 1;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            Debug.LogError("Previous spawning did not finish before new one started.");
        }

        spawnCoroutine = SpawnCustomers(spawnInterval, totalSpawns, health, patience, attackDamage);
        StartCoroutine(spawnCoroutine);
    }

    private IEnumerator SpawnCustomers(float spawnInterval, int totalSpawns, float health, float patience, float attackDamage)
    {
        for (int i = 0; i < totalSpawns; i++)
        {
            SpawnCustomer(health, patience, attackDamage);
            if (i != totalSpawns - 1)
            {
                yield return new WaitForSeconds(spawnInterval);    
            }
        }

        spawnCoroutine = null;
    }

    private void SpawnCustomer(float health, float patience, float attackDamage)
    {
        // TODO: What to do if availableSpawnPoints is 0, still need to spawn?
        var availableSpawnPoints = spawnPoints
            .Where(s => !Physics.CheckSphere(s.position, checkRadius, customerLayerMask))
            .ToList();
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("All spawn points are occupied.");
            return;
        }
        var index = random.Next(0, availableSpawnPoints.Count);
        var spawnPoint = availableSpawnPoints[index];

        var customerObject = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        var customer = customerObject.GetComponent<Customer>();
        if (customer != null)
        {
            var order = OrderFactory.CreateOrder(4);
            // customer.SetHealthBarCanvas(healthBarCanvas); // TODO: Move Canvas code to Customer class
            customer.Initialize(health, patience, attackDamage, order, nextId);
            nextId++;
            OnSpawned?.Invoke(customer);
        }
    }
}