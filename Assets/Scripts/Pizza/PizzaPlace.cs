using System.Collections;
using System.Collections.Generic;
using TMPro; 
using UnityEngine;

public class PizzaPlace : MonoBehaviour
{
    [SerializeField] private AudioClip restockSound;
    [SerializeField] private float restockTime = 2.5f; // Time in seconds for restocking
    [SerializeField] private TMP_Text restockingText; // Text displaying while restocking

    private AudioSource audioSource;
    private Coroutine restockingCoroutine;
    private bool isRestocking = false;

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

        //hider restocking text       
        if (restockingText != null)
        {
            restockingText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isRestocking = true;
            if (restockingCoroutine != null)
            {
                StopCoroutine(restockingCoroutine);
            }
            restockingCoroutine = StartCoroutine(RestockWithLoopingSound(other.gameObject));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InterruptRestocking();
        }
    }

    private void InterruptRestocking()
    {
        isRestocking = false;
        if (restockingCoroutine != null)
        {
            StopCoroutine(restockingCoroutine);
            restockingCoroutine = null;
        }

        if (restockingText != null)
        {
            restockingText.gameObject.SetActive(false);
            audioSource.Stop();
        }
    }

    private IEnumerator RestockWithLoopingSound(GameObject player)
    {
        // Start playing the looping sound
        audioSource.Play();
        // Start to display the restocking text
        restockingText.gameObject.SetActive(true);

        // Wait for the specified restock time
        float elapsedTime = 0f;
        while (isRestocking && elapsedTime < restockTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (isRestocking)
        {
            // Reset restocking var
            isRestocking = false;
            // Stop the looping sound
            audioSource.Stop();
            // Stop to display restocking text
            restockingText.gameObject.SetActive(false);
            // Perform the restocking
            Player thePlayer = player.GetComponent<Player>();
            thePlayer.Restocking();
        }
    }
}