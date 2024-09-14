using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 1f;
    public float checkRadius = 0.5f;

    private Transform[] _spawnPoints;
    // TODO: Set up later?
    // private bool isFirstEnemy = true;

    private void Start()
    {
        _spawnPoints = GameObject.FindObjectsOfType<Transform>()
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

            if (!Physics.CheckSphere(spawnPoint.position, checkRadius))
            {
                Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                break;
            }
        }
    }
}
