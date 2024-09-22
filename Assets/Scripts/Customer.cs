using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int health;
    [SerializeField] private float waitingTime = 3f;

    [Header("Audio")]
    [SerializeField] private AudioClip rageSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip happySound;

    private NavMeshAgent agent;
    private Animator animator;
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
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            animator.SetTrigger("isAttacking");
            // Vector3 currentPosition = transform.position;
            // transform.LookAt(player);
            // transform.position.Set(currentPosition.x, transform.position.y, currentPosition.z);
            agent.isStopped = true;
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
            Destroy(gameObject);
        }
    }
}
