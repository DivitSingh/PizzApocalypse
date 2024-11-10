using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Random = System.Random;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private LayerMask customerLayerMask;
    [SerializeField] private Canvas healthBarCanvas; // TODO: Move this to Customer class?

    private Transform[] spawnPoints;
    private readonly float checkRadius = 1.5f;
    private readonly Random random = new Random();
    
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

    public void StartSpawning(SpawnConfiguration spawnConfig, GenerateCustomerConfiguration customerConfig)
    {
        StartCoroutine(SpawnCustomers(spawnConfig, customerConfig));
    }

    private IEnumerator SpawnCustomers(SpawnConfiguration spawnConfig, GenerateCustomerConfiguration customerConfig)
    {
        // Wait a bit before beginning spawning
        yield return new WaitForSeconds(0.5f);
        
        var id = 1;
        for (int i = 0; i < spawnConfig.Count; i++)
        {
            SpawnCustomer(customerConfig, id);
            if (i != spawnConfig.Count - 1)
            {
                yield return new WaitForSeconds(spawnConfig.Interval);    
            }

            id++;
        }
    }

    private void SpawnCustomer(GenerateCustomerConfiguration config, int id)
    {
        // TODO: Choose spawn point
        var spawnPoint = PickSpawnPoint();
        if (spawnPoint == null) return;
        
        var customerObject = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        var customer = customerObject.GetComponent<Customer>();
        if (customer != null)
        {
            var order = OrderFactory.CreateOrder(config.MaxOrderSize);
            customer.Initialize(config.Satisfaction, config.Patience, config.Attack, order, id);
            OnSpawned?.Invoke(customer);
        }
    }

    private Transform PickSpawnPoint()
    {
        var availableSpawnPoints = spawnPoints
            .Where(s => !Physics.CheckSphere(s.position, checkRadius, customerLayerMask))
            .ToList();

        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points available.");
            return null;
        }
        
        var index = random.Next(0, availableSpawnPoints.Count);
        return availableSpawnPoints[index];
    }
}