using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Random = System.Random;
using UnityEngine.AI;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] customerPrefabs;
    [SerializeField] private LayerMask customerLayerMask;
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

        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogError("No customer prefabs assigned in the CustomerSpawner!");
        }
    }

    public void StartSpawning(SpawnConfiguration spawnConfig, GenerateCustomerConfiguration customerConfig)
    {
        // Stop all coroutines right before - DONE
        StopAllCoroutines();
        StartCoroutine(SpawnCustomers(spawnConfig, customerConfig));
    }

    private IEnumerator SpawnCustomers(SpawnConfiguration spawnConfig, GenerateCustomerConfiguration customerConfig)
    {
        // Wait a bit before beginning spawning
        yield return new WaitForSeconds(1f);
        Debug.Log($"Spawn Interval: {spawnConfig.Interval}");
        var id = 1;

        for (int i = 0; i < spawnConfig.TotalCustomerCount; i++) // Bug here end coroutine, make for loop per wave
        {
            // Spawn amount of customers for wave
            for (int ii = 0; ii < spawnConfig.CustomerPerWave; ii++)
            {
                SpawnCustomer(customerConfig, id);
                id++;
            }
            if (i != spawnConfig.TotalCustomerCount - 1)
            {
                yield return new WaitForSeconds(spawnConfig.Interval);
            }
        }
    }
// old SpawnCustomers below
//     private IEnumerator SpawnCustomers(int round, SpawnConfiguration spawnConfig, GenerateCustomerConfiguration customerConfig)
//     {
//         // Wait a bit before beginning spawning
//         yield return new WaitForSeconds(1f);
        
//         var id = 1;
//         for (int i = 0; i < spawnConfig.Count; i++)
//         {
//             SpawnCustomer(customerConfig, id);
//             // new spawn customers as rounds
//             // id++;
//             // SpawnCustomer(customerConfig, id);
//             if (i != spawnConfig.Count - 1)
//             {
//                 yield return new WaitForSeconds(spawnConfig.Interval);    
//             }

//             id++;
//         }
//     }

    private void SpawnCustomer(GenerateCustomerConfiguration config, int id)
    {
        // TODO: Choose spawn point
        var spawnPoint = PickSpawnPoint();
        if (spawnPoint == null) return;

        // Randomly select a prefab
        var selectedPrefab = customerPrefabs[random.Next(customerPrefabs.Length)];

        var customerObject = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        var customer = customerObject.GetComponent<Customer>();
        if (customer != null)
        {
            var order = OrderFactory.CreateOrder(config.MaxOrderSize);
            customer.Initialize(config.Satisfaction, config.Patience, config.Attack, order, id);
            OnSpawned?.Invoke(customer);
        }
    }

    public void SpawnHungryCustomer()
    {
        var spawnPoint = PickSpawnPoint();
        if (spawnPoint == null) return;

        // Randomly select a prefab
        var selectedPrefab = customerPrefabs[random.Next(customerPrefabs.Length)];
        var customerObject = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        customerObject.name = "Hungry Customer";
        var customer = customerObject.GetComponent<Customer>();
        if (customer != null)
        {
            var order = OrderFactory.CreateOrder(1);
            customer.Initialize(10, 1000, 0, order, 1);
            OnSpawned?.Invoke(customer);
        }
    }


    public void SpawnAngryCustomer()
    {
        var spawnPoint = PickSpawnPoint();
        if (spawnPoint == null) return;

        // Randomly select a prefab
        var selectedPrefab = customerPrefabs[random.Next(customerPrefabs.Length)];
        var customerObject = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        customerObject.name = "Angry Customer";
        var customer = customerObject.GetComponent<Customer>();
        if (customer != null)
        {
            var order = OrderFactory.CreateOrder(1);
            customer.Initialize(10, 0.1f, 0, order, 2);
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