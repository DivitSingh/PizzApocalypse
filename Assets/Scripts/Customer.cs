using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    public int health;
    public float waitingTime;
    public AudioClip rageSound;
    public AudioClip hurtSound;
    public AudioClip dieSound;
    public AudioClip happySound;
    private CustomerState state;

    #region State
    public enum CustomerState
    {
        Chasing,
        Waiting
    }
    #endregion

    private void Start()
    {
        ChangeState(CustomerState.Waiting);
    }

    private void Update()
    {
        if (state == CustomerState.Chasing)
            GetComponent<NavMeshAgent>().destination = GameObject.Find("Player").transform.position;
    }

    private IEnumerator WaitForFood()
    {
        yield return new WaitForSeconds(waitingTime);
        ChangeState(CustomerState.Chasing);
    }

    private void ChangeState(CustomerState newState)
    {
        if (newState == state) return;
        state = newState;
        switch (newState)
        {
            case CustomerState.Chasing:
                GetComponent<Renderer>().material.color = Color.red;
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(rageSound);
                break;
            case CustomerState.Waiting:
                StartCoroutine(WaitForFood());
                GetComponent<NavMeshAgent>().ResetPath();
                break;
            default:
                break;
        }
    }

    public void ReceivePizza(int damage)
    {
        if (state == CustomerState.Chasing)
        {
            health -= damage;

            if (health <= 0)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(dieSound);
                Destroy(gameObject);
            }
            else
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(hurtSound);
            }
        }
        else if (state == CustomerState.Waiting)
        {
            GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(happySound);
            StopCoroutine(WaitForFood());
            Destroy(gameObject);
        }
    }
}
