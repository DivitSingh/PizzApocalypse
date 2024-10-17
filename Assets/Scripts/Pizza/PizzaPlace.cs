using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaPlace : MonoBehaviour
{
    [SerializeField] private AudioClip restockSound;
    [SerializeField] private float restockTime = 2.5f; // Time in seconds for restocking

    private AudioSource audioSource;

    private void Start()
    {
        // Get or add an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
        audioSource.clip = restockSound;
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(RestockWithLoopingSound(other.gameObject));
        }
    }

    private IEnumerator RestockWithLoopingSound(GameObject player)
    {
        // Start playing the looping sound
        audioSource.Play();

        // Wait for the specified restock time
        yield return new WaitForSeconds(restockTime);

        // Stop the looping sound
        audioSource.Stop();

        // Perform the restocking
        Player thePlayer = player.GetComponent<Player>();
        thePlayer.Restocking();
    }
}