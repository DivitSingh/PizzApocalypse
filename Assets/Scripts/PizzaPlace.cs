using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PizzaPlace : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        if (other.gameObject.GetComponent<PlayerMovement>() != null)
        {
            PlayerMovement thePlayer = other.gameObject.GetComponent<PlayerMovement>();
            thePlayer.Restocking();
        }
    }
}
