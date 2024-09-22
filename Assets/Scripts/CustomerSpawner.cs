using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 1f;
    private Transform[] _spawnPoints;

    private void Start()
    {
        _spawnPoints = FindObjectsOfType<Transform>()
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
        List<Transform> availableSpawnPoints = new List<Transform>(_spawnPoints);
        while (availableSpawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomIndex];
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            availableSpawnPoints.RemoveAt(randomIndex);
        }
    }
}
