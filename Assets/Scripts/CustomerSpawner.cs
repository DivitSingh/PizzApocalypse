using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 1f;
    public float checkRadius = 0.5f;

    private Transform[] spawnPoints;
    private bool isFirstEnemy = true;

    private void Start()
    {
        spawnPoints = GameObject.FindObjectsOfType<Transform>()
            .Where(t => t.name == "SpawnPoint")
            .ToArray();

        Debug.Log($"Found {spawnPoints.Length} spawn points");
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
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        while (availableSpawnPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomIndex];

            if (!Physics.CheckSphere(spawnPoint.position, checkRadius))
            {
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                EnemyBehavior enemyBehavior = newEnemy.GetComponent<EnemyBehavior>();
                
                if (enemyBehavior != null)
                {
                    if (isFirstEnemy)
                    {
                        enemyBehavior.SetAngryImmediately();
                        isFirstEnemy = false;
                        Debug.Log("First enemy set to angry immediately");
                    }
                }
                
                break;
            }
            else
            {
                availableSpawnPoints.RemoveAt(randomIndex);
            }
        }
    }
}