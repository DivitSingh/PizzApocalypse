using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;


// TODO: Need to be able to spawn with context
/**
 * - Don't spawn when game is paused, nor remove time from timer
 * - Need to know customer stats (Health, Patience, Order Size?)
 * - Need to know round spawning stats (totalSpawns, spawnInterval)
 * - Need to know spawn points
 */
public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private Canvas healthBarCanvas; // TODO: Move this to Customer class?

    private Transform[] spawnPoints;
    private readonly float checkRadius = 0.5f;
    private readonly Random random = new Random();

    private void Awake()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
            .Select(s => s.transform)
            .ToArray();
        
        if (spawnPoints.Length == 0)
        {
            throw new ArgumentException("No spawn points found! Make sure your spawn points are tagged with 'SpawnPoint'");
        }

        if (customerPrefab == null)
        {
            // Debug.LogError("Enemy prefab is not assigned in the CustomerSpawner!");
            throw new ArgumentException("Customer Prefab cannot be null.");
        }

        if (healthBarCanvas == null)
        {
            throw new ArgumentException("HealthBar Canvas cannot be null.");
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
        StartCoroutine(SpawnCustomers(spawnInterval, totalSpawns, health, patience, attackDamage));
    }

    private IEnumerator SpawnCustomers(float spawnInterval, int totalSpawns, float health, float patience, float attackDamage)
    {
        for (int i = 0; i < totalSpawns; i++)
        {
            SpawnCustomer(health, patience, attackDamage);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCustomer(float health, float patience, float attackDamage)
    {
        // TODO: What to do if availableSpawnPoints is 0, still need to spawn?
        var availableSpawnPoints = spawnPoints
            .Where(s => Physics.CheckSphere(s.position, checkRadius))
            .ToList();

        var index = random.Next(0, availableSpawnPoints.Count);
        var spawnPoint = availableSpawnPoints[index];
        
        var customerObject = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        var customer = customerObject.GetComponent<Customer>();
        if (customer != null)
        {
            var order = GenerateOrder();
            customer.SetHealthBarCanvas(healthBarCanvas); // TODO: Move Canvas code to Customer class
            customer.Initialize(health, patience, attackDamage, order);
        }
    }

    private Order GenerateOrder()
    {
        // TODO: Add quantity to orders?
        var pizzaType = (PizzaType) random.Next(0, Enum.GetValues(typeof(PizzaType)).Length);
        return new Order(pizzaType);
    }
}