using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health;
    [SerializeField] private float waitingTime = 3f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private Order currentOrder;

    [SerializeField] private GameObject orderDisplayPrefab;
    [SerializeField] private Vector3 orderDisplayOffset = new Vector3(0, 2.5f, 0);
    private OrderDisplay orderDisplay;

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
    private float agentBaseSpeed;
    private Animator animator;
    private BoxCollider boxCollider;
    private Transform player;
    private State state = State.Hungry;

    private List<IEffect> activeEffects = new List<IEffect>();

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
        agentBaseSpeed = agent.speed;
        CreateCircularTimer();
        GenerateRandomOrder();
        UpdateOrderDisplay();
        StartCoroutine(Waiting());
    }

    private void GenerateRandomOrder()
    {
        PizzaType randomType = (PizzaType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(PizzaType)).Length);
        currentOrder = new Order(randomType);
    }

    private void UpdateOrderDisplay()
    {
        if (orderDisplay != null)
        {
            orderDisplay.UpdateOrderDisplay(currentOrder.PizzaType);
        }
        else
        {
            Debug.LogWarning("OrderDisplay is null when trying to update!");
        }
    }

    private void UpdateOrderUI()
    {
        if (orderDisplay != null)
        {
            orderDisplay.UpdateOrderDisplay(currentOrder.PizzaType);
        }
        else
        {
            Debug.LogWarning("Tried to update order display, but it's null!");
        }
    }

    private void CreateOrderDisplay()
    {
        Debug.Log($"Creating order display. Prefab assigned: {orderDisplayPrefab != null}");

        if (orderDisplayPrefab == null)
        {
            Debug.LogError("Order Display Prefab is not assigned in the Customer script!");
            return;
        }

        if (healthBarCanvas == null)
        {
            Debug.LogError("Health Bar Canvas is not set for this Customer!");
            return;
        }

        GameObject orderDisplayObject = Instantiate(orderDisplayPrefab, healthBarCanvas.transform);
        orderDisplay = orderDisplayObject.GetComponent<OrderDisplay>();
        if (orderDisplay == null)
        {
            Debug.LogError("OrderDisplay component not found on the instantiated prefab!");
            return;
        }

        orderDisplay.SetTarget(transform);
        orderDisplay.offset = orderDisplayOffset;

        Debug.Log($"Order display created for customer at {transform.position}");
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

    public void SetOrder(Order order)
    {
        currentOrder = order;
        UpdateOrderUI();
    }

    public void SetOrderDisplayPrefab(GameObject prefab)
    {
        orderDisplayPrefab = prefab;
        CreateOrderDisplay();
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
        DestroyOrderDisplay();
        Chase();
    }

    private void DestroyOrderDisplay()
    {
        if (orderDisplay != null)
        {
            Destroy(orderDisplay.gameObject);
            orderDisplay = null;
        }
    }

    public void SetHealthBarCanvas(Canvas canvas)
    {
        healthBarCanvas = canvas;
        CreateOrderDisplay();
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

    public void ReceivePizza(IPizza pizza)
    {
        if (state == State.Angry)
        {
            health -= pizza.Damage;
            UpdateHealthBar();
            if (health <= 0)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(dieSound);
                Destroy(healthBar.gameObject);
                if (circularTimer != null)
                {
                    Destroy(circularTimer.gameObject);
                }
                StopAllCoroutines();
                DestroyOrderDisplay();
                Destroy(gameObject);
            }
            else
            {
                StartCoroutine(StartEffect(pizza.CustomerEffect));
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(hurtSound);
            }
        }
        else if (state == State.Hungry)
        {
            if (pizza.Type == currentOrder.PizzaType)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(happySound);
                StopAllCoroutines();
                GameManager.Instance.HandleFedCustomerScoring(this);
                if (circularTimer != null)
                {
                    Destroy(circularTimer.gameObject);
                }
                DestroyOrderDisplay();
                Destroy(gameObject);
            }
            else
            {
                // Wrong pizza type, do nothing
                Debug.Log($"Customer received wrong pizza. Wanted: {currentOrder.PizzaType}, Got: {pizza.Type}");
            }
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
        DestroyOrderDisplay();
    }

    #region Effects

    private IEnumerator StartEffect(IEffect effect)
    {
        // Check if effect is duplicate
        var existingEffect = activeEffects
            .FirstOrDefault(t => t.Type == effect.Type && t.AffectedStat == effect.AffectedStat);
        if (existingEffect != null)
        {
            existingEffect.Duration = Math.Max(existingEffect.Duration, effect.Duration);
            yield break;
        }

        // Effect is not duplicate, repeat effect while it is active
        activeEffects.Add(effect);
        while (effect.Duration > 0)
        {
            ApplyEffect(effect);
            effect.Duration--;
            yield return new WaitForSeconds(1);
        }

        activeEffects.Remove(effect);
        CleanupEffect(effect);
    }

    /// <summary>
    /// Handles the application of a given effect.
    /// </summary>
    /// <param name="effect"></param>
    private void ApplyEffect(IEffect effect)
    {
        switch (effect.AffectedStat)
        {
            case Stat.Health:
                if (effect.Type == EffectType.Multiplier) return; // This should not be possible, assuming only decrease
                if (effect.Type == EffectType.ConstantDecrease) health -= effect.Value;
                if (effect.Type == EffectType.ConstantIncrease) health += effect.Value;
                UpdateHealthBar(); // TODO: Should do this automatically when health changes, add setter
                break;
            case Stat.Speed:
                if (effect.Type == EffectType.Multiplier)
                {
                    // TODO: If slowness is eventually added, will need to modify animator speed instead of disabling it
                    agent.speed *= effect.Value;
                    animator.enabled = false;
                }
                break;
        }
    }

    /// <summary>
    /// Performs the necessary cleanup when the given effect has finished.
    /// </summary>
    /// <param name="effect"></param>
    private void CleanupEffect(IEffect effect)
    {
        // NOTE: This is currently only for stun effect, may be extended later
        if (effect.AffectedStat == Stat.Speed && effect.Value == 0)
        {
            agent.speed = agentBaseSpeed;
            animator.enabled = true;
        }
    }

    #endregion
}
