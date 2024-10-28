using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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
    public float Patience { get; private set; }
    private State state = State.Hungry;
    public Order Order { get; private set; }
    private List<IEffect> activeEffects = new List<IEffect>();
    [SerializeField] private CustomerUI customerUI;
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
    
    // Events
    public event Action<Customer> OnBecameAngry;
    public event Action<Customer> OnFed; 

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
        this.Patience = patience;
        this.attackDamage = attackDamage;
        this.Order = order;
        customerUI.maxTime = this.Patience;
        customerUI.currentTime = customerUI.maxTime;
        customerUI.UpdateOrderDisplay(this.Order);
        StartCoroutine(Waiting());
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
        yield return new WaitForSeconds(Patience);
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
        customerUI.orderPanel.SetActive(false);
        GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(rageSound);
        transform.Find("Marker").GetComponent<SpriteRenderer>().sprite = angryMarker;
        customerUI.healthBar.gameObject.SetActive(true);
        Destroy(GetComponent<CustomerIndicator>());
        Chase();
        OnBecameAngry?.Invoke(this);
    }

    public void ReceivePizza(IPizza pizza)
    {
        if (state == State.Angry)
        {
            StartCoroutine(Eat());
            health -= pizza.Damage;
            customerUI.UpdateHealthBar((float)health / maxHealth);
            if (health <= 0)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(dieSound);
                StartCoroutine(RemoveCustomer());
            }
            else
            {
                if (pizza.CustomerEffect != null)
                    StartCoroutine(StartEffect(pizza.CustomerEffect));
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(hurtSound);
            }
        }
        else if (state == State.Hungry)
        {
            transform.LookAt(player);
            Order.DeductPizzaFromOrder(pizza);
            customerUI.UpdateOrderDisplay(Order);
            if (Order.IsOrderFulfilled() > -1)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(completeSound);
                StopAllCoroutines();
                GameManager.Instance.HandleFedCustomerScoring(this);
                player.GetComponent<Player>().playerInventory.IncreaseMoney(Order.IsOrderFulfilled());
                OnFed?.Invoke(this);
                StartCoroutine(RemoveCustomer());
            }
            else
                Debug.Log($"Order not yet fulfilled, still needs: {Order.PasteOrderContents()}");
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
        // DestroyOrderDisplay();
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
                customerUI.UpdateHealthBar((float)health / maxHealth);
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
