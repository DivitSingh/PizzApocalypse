using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    private int health;
    private EnemyPatrol enemyPatrol;
    private EnemyFollow enemyFollow;
    private CustomerAnimator _customerAnimator;

    private Transform _player;
    private NavMeshAgent _agent;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackRange = 2f;

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
        _agent = GetComponent<NavMeshAgent>();
        _customerAnimator = GetComponent<CustomerAnimator>();
        enemyPatrol = GetComponent<EnemyPatrol>();
        enemyPatrol.enabled = false;
        enemyPatrol.onPlayerDetected += HandlePlayerDetected;

        enemyFollow = GetComponent<EnemyFollow>();
        enemyFollow.enabled = true;

        _player = GameObject.Find("Player").transform;
        ChangeState(CustomerState.Waiting);
    }

    private void Update()
    {
        if (state == CustomerState.Chasing && Physics.CheckSphere(transform.position, attackRange, playerLayer))
        {
            Debug.Log("Player detected");
            Attack();
        }
    }

    private void HandlePlayerDetected()
    {
        if (state != CustomerState.Patrolling) return;

        state = CustomerState.Chasing;
        enemyPatrol.enabled = false;
        enemyFollow.enabled = false;
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
                
                // TODO: Deal with changing color
                // GetComponent<Renderer>().material.color = Color.red;
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(rageSound);
                _agent.SetDestination(_player.position);
                _customerAnimator.SetChasing();
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

    private void Attack()
    {
        if (!_customerAnimator.IsAttacking())
        {
            _customerAnimator.Attack();
            _agent.SetDestination(transform.position);
        }
    }
}
