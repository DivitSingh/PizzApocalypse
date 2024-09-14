using UnityEngine;
using System.Collections;

public class EnemyBehavior : MonoBehaviour
{
    public float timeToAngry = 2f; // Set this to 2 seconds as per your requirement
    private bool isAngry = false;
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;
        Debug.Log($"Enemy spawned at {spawnTime}");
        StartCoroutine(BecomeAngryAfterDelay());
    }

    public void SetAngryImmediately()
    {
        StopAllCoroutines();
        BecomeAngry();
    }

    private IEnumerator BecomeAngryAfterDelay()
    {
        yield return new WaitForSeconds(timeToAngry);
        BecomeAngry();
    }

    private void BecomeAngry()
    {
        if (!isAngry)
        {
            isAngry = true;
            GetComponent<Renderer>().material.color = Color.red;
            float angryTime = Time.time;
            Debug.Log($"Enemy became angry at {angryTime}. Time since spawn: {angryTime - spawnTime}");
        }
    }
}