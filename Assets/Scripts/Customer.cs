using System.Collections;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private int health;
    private EnemyPatrol enemyPatrol;
    private EnemyFollow enemyFollow;

    public AudioClip rageSound;
    public AudioClip hurtSound;
    public AudioClip dieSound;
    public AudioClip happySound;
    public float waitingTime = 3f;

    #region State
    public enum CustomerState
    {
        Chasing, //chasing => angry customer
        Patrolling,
        Waiting  //waiting => hungry customer
    }

    private CustomerState state = CustomerState.Patrolling;
    #endregion

    private void Start()
    {
        enemyPatrol = GetComponent<EnemyPatrol>();
        enemyPatrol.enabled = false;
        enemyPatrol.onPlayerDetected += HandlePlayerDetected;

        enemyFollow = GetComponent<EnemyFollow>();
        enemyFollow.enabled = false;

        ChangeState(CustomerState.Waiting);
    }

    private void HandlePlayerDetected()
    {
        if (state != CustomerState.Patrolling) return;

        state = CustomerState.Chasing;
        enemyPatrol.enabled = false;
        enemyFollow.enabled = true;
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
                enemyPatrol.enabled = false;
                enemyFollow.enabled = true;
                GetComponent<Renderer>().material.color = Color.red;
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(rageSound);
                break;
            case CustomerState.Waiting:
                StartCoroutine(WaitForFood());
                enemyFollow.enabled = false;
                enemyFollow.enabled = false;
                break;
            case CustomerState.Patrolling:
                enemyFollow.enabled = false;
                enemyPatrol.enabled = true;
                break;
            default:
                break;
        }
    }

    // called by a pizzaSlice collision
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
            //TODO Trigger customer eating a pizza slice animation.
        }
    }
}
