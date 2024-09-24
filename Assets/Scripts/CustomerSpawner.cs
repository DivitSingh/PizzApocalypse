using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private Canvas healthBarCanvas;

    private Transform[] spawnPoints;
    private readonly Random _random = new Random();

    private void Start()
    {
        spawnPoints = GameObject.FindObjectsOfType<Transform>()
            .Where(t => t.CompareTag("SpawnPoint"))
            .ToArray();

        Debug.Log($"Found {spawnPoints.Length} spawn points");

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points found! Make sure your spawn points are tagged with 'SpawnPoint'");
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned in the CustomerSpawner!");
        }

        if (healthBarCanvas == null)
        {
            Debug.LogError("Health Bar Canvas is not assigned in the CustomerSpawner!");
        }

        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null)
        {
            Debug.LogError("Cannot spawn enemy: missing spawn points or enemy prefab");
            return;
        }

        var availableSpawnPoints = spawnPoints
            .Where(s => Physics.CheckSphere(s.position, checkRadius))
            .ToList();

        Debug.Log($"Available spawn points: {availableSpawnPoints.Count}");

        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No available spawn points found. Skipping this spawn.");
            return;
        }

        var index = _random.Next(0, availableSpawnPoints.Count);
        var spawnPoint = availableSpawnPoints[index];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        Customer customer = enemy.GetComponent<Customer>();
        if (customer != null && healthBarCanvas != null)
        {
            customer.SetHealthBarCanvas(healthBarCanvas);
            Debug.Log($"Spawned customer at {spawnPoint.position}");
        }
        else
        {
            Debug.LogError("Failed to set health bar canvas for spawned customer!");
        }
    }
}