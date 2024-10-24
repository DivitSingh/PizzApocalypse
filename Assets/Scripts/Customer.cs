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
    [Header("References")]
    private NavMeshAgent agent;
    private float agentBaseSpeed;
    private Animator animator;
    private BoxCollider boxCollider;
    private Transform player;

    [Header("Stats")]
    private float maxHealth;
    private float health;
    private float attackDamage;
    private float patience;
    private State state = State.Hungry;
    private Order order;
    private List<IEffect> activeEffects = new List<IEffect>();

    [Header("UI")]
    private Canvas healthBarCanvas;
    [SerializeField] private CustomerUI customerUI;
    [SerializeField] private GameObject orderDisplayPrefab;
    [SerializeField] private Vector3 orderDisplayOffset = new Vector3(0, 2.5f, 0);
    private OrderDisplay orderDisplay;
    [SerializeField] private Sprite angryMarker;

    [Header("Audio")]
    [SerializeField] private AudioClip rageSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip completeSound;

    [Header("Textures")]
    [SerializeField] private Texture idleTexture;
    [SerializeField] private Texture walkTexture;
    [SerializeField] private Texture attackTexture;
    [SerializeField] private Texture eatTexture;

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
        agentBaseSpeed = agent.speed;
    }

    public void Initialize(float health, float patience, float attackDamage, Order order)
    {
        maxHealth = health;
        this.health = health;
        this.patience = patience;
        this.attackDamage = attackDamage;
        this.order = order;
        customerUI.maxTime = patience;

        CreateOrderDisplay();
        UpdateOrderDisplay();
        StartCoroutine(Waiting());
    }

    private void UpdateOrderDisplay()
    {
        if (orderDisplay != null)
            orderDisplay.UpdateOrderDisplay(order);
        else
            Debug.LogWarning("OrderDisplay is null when trying to update!");
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
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 50f);

        if (Vector3.Distance(transform.position, player.position) <= agent.stoppingDistance * 1.5)
            Attack();
        else
            Chase();
    }

    private IEnumerator Waiting()
    {
        state = State.Hungry;
        GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = idleTexture;
        yield return new WaitForSeconds(patience);
        // circularTimer.gameObject.SetActive(false);
        customerUI.timerImage.gameObject.SetActive(false);
        BecomeAngry();
    }


    private void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        if (GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture != eatTexture)
            GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = walkTexture;
        animator.SetBool("isChasing", true);
    }

    private void Attack()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !animator.IsInTransition(0))
        {
            GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = attackTexture;
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
        if (state == State.Angry)
            if (other.GetComponent<Player>() != null)
                other.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
    }

    private void BecomeAngry()
    {
        state = State.Angry;
        GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = walkTexture;
        GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(rageSound);
        transform.Find("Marker").GetComponent<SpriteRenderer>().sprite = angryMarker;
        // CreateHealthBar();
        customerUI.healthBar.gameObject.SetActive(true);
        DestroyOrderDisplay();
        Chase();
    }

    private void DestroyOrderDisplay()
    {
        if (orderDisplay != null)
        {
            orderDisplay.RemoveOrderUI(); // Call RemoveOrderUI instead of destroying directly
            orderDisplay = null;
        }
    }

    public void SetHealthBarCanvas(Canvas canvas)
    {
        healthBarCanvas = canvas;
        CreateOrderDisplay();
    }

    private void UpdateHealthBar()
    {
        customerUI.UpdateHealthBar((float)health / maxHealth);
    }

    public void ReceivePizza(IPizza pizza)
    {
        if (state == State.Angry)
        {
            StartCoroutine(Eat());
            health -= pizza.Damage;
            UpdateHealthBar();
            if (health <= 0)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(dieSound);
                StartCoroutine(RemoveCustomer());
            }
            else
            {
                if (pizza.CustomerEffect != null)
                {
                    StartCoroutine(StartEffect(pizza.CustomerEffect));
                }
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(hurtSound);
            }
        }
        else if (state == State.Hungry)
        {
            transform.LookAt(player);
            order.DeductPizzaFromOrder(pizza);
            UpdateOrderDisplay(); // Update the order display after receiving a pizza
            if (order.IsOrderFulfilled() > -1)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(completeSound);
                StopAllCoroutines();
                GameManager.Instance.HandleFedCustomerScoring(this);
                player.GetComponent<Player>().playerInventory.IncreaseMoney(order.IsOrderFulfilled());
                StartCoroutine(RemoveCustomer());
            }
            else
            {
                // Wrong pizza type, do nothing
                Debug.Log($"Order not yet fulfilled, still needs: {order.PasteOrderContents()}");
            }
        }
    }

    private IEnumerator RemoveCustomer()
    {
        agent.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = idleTexture;
        animator.Play("Celebrate");
        customerUI.healthBar.gameObject.SetActive(false);
        customerUI.timerImage.gameObject.SetActive(false);
        DestroyOrderDisplay();
        yield return new WaitForSeconds(1.2f);
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private IEnumerator Eat()
    {
        Texture originalTexture = GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture;
        GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = eatTexture;
        yield return new WaitForSeconds(0.25f);
        GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = originalTexture;
    }

    private void OnDestroy()
    {
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
