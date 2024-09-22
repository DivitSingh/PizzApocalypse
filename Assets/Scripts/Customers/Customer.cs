using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip rageSound;
    public AudioClip hurtSound;
    public AudioClip dieSound;
    public AudioClip happySound;
    
    [Header("Stats")]
    [SerializeField] private int health;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float waitingTime = 3f;
    
    private NavMeshAgent _agent;
    private CustomerAnimator _animator;
    
    private Transform _player;
    [SerializeField] private LayerMask playerLayer;

    private enum State
    {
        Chasing,
        Waiting
    }

    private State _state = State.Waiting;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<CustomerAnimator>();
        _player = GameObject.Find("Player").transform;
        StartCoroutine(WaitForFood());
    }

    private void Update()
    {
        if (_state != State.Chasing) return;
        
        if (Physics.CheckSphere(transform.position, attackRange, playerLayer))
        {
            Attack();
        }
        else if (!_animator.IsAttacking())
        {
            // Don't move character while attacking
            _agent.destination = _player.position;
        }
    }

    private IEnumerator WaitForFood()
    {
        yield return new WaitForSeconds(waitingTime);
        StartChasing();
    }

    private void StartChasing()
    {
        _state = State.Chasing;
        GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(rageSound);
        _agent.SetDestination(_player.position);
        _animator.SetChasing();
    }

    private void Attack()
    {
        if (!_animator.IsAttacking())
        {
            _animator.Attack();
            _agent.SetDestination(transform.position);
        }
    }
    
    public void ReceivePizza(int damage)
    {
        if (_state == State.Chasing)
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
        else if (_state == State.Waiting)
        {
            GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(happySound);
            StopCoroutine(WaitForFood());
            Destroy(gameObject);
            //TODO Trigger customer eating a pizza slice animation.
        }
    }
}
