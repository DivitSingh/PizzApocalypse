using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int health;
    [SerializeField] private float waitingTime = 3f;
    [SerializeField] private float attackDamage = 20f;

    [Header("Audio")]
    [SerializeField] private AudioClip rageSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip happySound;

    private NavMeshAgent agent;
    private Animator animator;
    private BoxCollider boxCollider;
    private Transform player;
    private State state = State.Hungry;

    private enum State
    {
        Angry,
        Hungry
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponentInChildren<BoxCollider>();
        player = GameObject.Find("Player").transform;
        StartCoroutine(Waiting());
    }

    private void Update()
    {
        if (state != State.Angry) return;

        if (Vector3.Distance(transform.position, player.position) <= agent.stoppingDistance * 1.5)
            Attack();
        else
            Chase();
    }

    private IEnumerator Waiting()
    {
        state = State.Hungry;
        yield return new WaitForSeconds(waitingTime);
        state = State.Angry;
        GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(rageSound);
        Chase();
    }

    private void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        animator.SetBool("isChasing", true);
    }

    private void Attack()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !animator.IsInTransition(0))
        {
            animator.SetTrigger("isAttacking");
            agent.isStopped = true;
        }
    }

    public void EnableAttack()
    {
        boxCollider.enabled = true;
    }

    public void DisableAttack()
    {
        boxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    public void ReceivePizza(int damage)
    {
        if (state == State.Angry)
        {
            health -= damage;

            if (health <= 0)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(dieSound);
                Destroy(gameObject);
            }
            else
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(hurtSound);
        }
        else if (state == State.Hungry)
        {
            GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(happySound);
            StopCoroutine(Waiting());
            GameManager.Instance.HandleFedCustomerScoring(this);
            Destroy(gameObject);
        }
    }
}
