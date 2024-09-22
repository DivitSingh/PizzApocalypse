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

    private Transform[] spawnPoints;
    private readonly Random _random = new Random();

    private void Start()
    {
        spawnPoints = GameObject.FindObjectsOfType<Transform>()
            .Where(t => t.CompareTag("SpawnPoint"))
            .ToArray();
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
        var availableSpawnPoints = spawnPoints
            .Where(s => Physics.CheckSphere(s.position, checkRadius))
            .ToList();

        if (availableSpawnPoints.Count == 0) return;

        var index = _random.Next(0, availableSpawnPoints.Count);
        var spawnPoint = availableSpawnPoints[index];
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
    }
}
