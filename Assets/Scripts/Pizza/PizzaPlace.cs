using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaPlace : MonoBehaviour
{
    [SerializeField] private AudioClip restockSound;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        if (other.gameObject.tag == "Player")
        {
            PlayerMovement thePlayer = other.gameObject.GetComponent<PlayerMovement>();
            thePlayer.Restocking();
            GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(restockSound);
        }
    }
}
