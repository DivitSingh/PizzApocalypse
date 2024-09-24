using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int health;
    [SerializeField] private float waitingTime = 3f;
    [SerializeField] private float attackDamage = 20f;

    [Header("UI")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2f, 0);
    private Canvas healthBarCanvas;
    private HealthBar healthBar;
    [SerializeField] private GameObject circularTimerPrefab;
    private CircularTimer circularTimer;


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
        health = maxHealth;
        CreateCircularTimer();
        StartCoroutine(Waiting());
    }

    private void Update()
    {
        if (state != State.Angry) return;

        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (Vector3.Distance(transform.position, player.position) <= agent.stoppingDistance * 1.5)
            Attack();
        else
            Chase();
    }

    private void CreateCircularTimer()
    {
        if (circularTimerPrefab == null)
        {
            Debug.LogError("CircularTimer Prefab is not assigned in the Customer script!");
            return;
        }

        if (healthBarCanvas == null)
        {
            Debug.LogError("Health Bar Canvas is not set for this Customer!");
            return;
        }

        GameObject timerObj = Instantiate(circularTimerPrefab, healthBarCanvas.transform);
        circularTimer = timerObj.GetComponent<CircularTimer>();
        if (circularTimer == null)
        {
            Debug.LogError("CircularTimer component not found on the instantiated prefab!");
            return;
        }

        circularTimer.SetTarget(transform);
        circularTimer.maxTime = waitingTime;
        circularTimer.ResetTimer();

        Debug.Log($"Circular timer created for customer at {transform.position}");
    }

    private IEnumerator Waiting()
    {
        state = State.Hungry;
        yield return new WaitForSeconds(waitingTime);
        circularTimer.gameObject.SetActive(false);
        BecomeAngry();
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

    private void BecomeAngry()
    {
        state = State.Angry;
        GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(rageSound);
        CreateHealthBar();
        Chase();
    }


    public void SetHealthBarCanvas(Canvas canvas)
    {
        healthBarCanvas = canvas;
    }

    private void CreateHealthBar()
    {
        if (healthBarPrefab == null)
        {
            Debug.LogError("Health Bar Prefab is not assigned in the Customer script!");
            return;
        }

        if (healthBarCanvas == null)
        {
            Debug.LogError("Health Bar Canvas is not set for this Customer!");
            return;
        }

        GameObject healthBarObject = Instantiate(healthBarPrefab, healthBarCanvas.transform);
        healthBar = healthBarObject.GetComponent<HealthBar>();
        if (healthBar == null)
        {
            Debug.LogError("HealthBar component not found on the instantiated prefab! Attempting to add one.");
            healthBar = healthBarObject.AddComponent<HealthBar>();
        }

        if (healthBar == null)
        {
            Debug.LogError("Failed to create or find HealthBar component!");
            return;
        }

        Slider slider = healthBarObject.GetComponentInChildren<Slider>();
        if (slider == null)
        {
            Debug.LogError("Slider component not found in the health bar prefab!");
            return;
        }

        healthBar.SetSlider(slider);
        healthBar.SetTarget(transform);
        healthBar.offset = healthBarOffset;
        UpdateHealthBar();

        Debug.Log($"Health bar created for customer at {transform.position}");
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar((float)health / maxHealth);
        }
        else
        {
            Debug.LogWarning("Tried to update health bar, but it's null!");
        }
    }

    public void ReceivePizza(int damage)
    {
        if (state == State.Angry)
        {
            health -= damage;
            UpdateHealthBar();
            if (health <= 0)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(dieSound);
                Destroy(healthBar.gameObject);
                if (circularTimer != null)
                {
                    Destroy(circularTimer.gameObject);
                }
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

            if (circularTimer != null)
            {
                Destroy(circularTimer.gameObject);
            }

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (circularTimer != null)
        {
            Destroy(circularTimer.gameObject);
        }
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }
    }
}
