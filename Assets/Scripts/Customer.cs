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
    public int Id { get; private set; }

    [Header("Stats")]
    private float maxHealth;
    private float _health;

    private float Health
    {
        get => _health;
        set
        {
            _health = value;
            customerUI.UpdateHealthBar((float)Health / maxHealth);
            if (Health <= 0)
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(dieSound);
                OnDeath?.Invoke(this);
                StartCoroutine(RemoveCustomer());
            }
        }
    }

    private float attackDamage;
    public float Patience { get; private set; }
    private State state = State.Hungry;
    public Order Order { get; private set; }
    private List<IEffect> activeEffects = new List<IEffect>();
    private ParticleSystem poisonParticles;

    [SerializeField] private CustomerUI customerUI;

    [Header("Audio")]
    [SerializeField] private AudioClip rageSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip completeSound;

    [Header("Textures")]
    [SerializeField] private Texture idleTexture;
    [SerializeField] private Texture walkTexture;
    [SerializeField] private Texture attackTexture;
    [SerializeField] private Texture eatTexture_angry;
    [SerializeField] private Texture eatTexture_hungry1;
    [SerializeField] private Texture eatTexture_hungry2;
    [SerializeField] public Sprite faceSprite;

    private enum State
    {
        Angry,
        Hungry
    }

    // Events
    public event Action<Customer> OnBecameAngry;
    public event Action<Customer> OnFed;
    public event Action<Customer> OnDeath;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponentInChildren<BoxCollider>();
        player = GameObject.Find("Player").transform;
        poisonParticles = GetComponentInChildren<ParticleSystem>();
        agentBaseSpeed = agent.speed;
    }

    public void Initialize(float health, float patience, float attackDamage, Order order, int id)
    {
        maxHealth = health;
        this.Health = health;
        Patience = patience;
        this.attackDamage = attackDamage;
        Order = order;
        Id = id;
        customerUI.maxTime = this.Patience;
        customerUI.currentTime = customerUI.maxTime;
        customerUI.UpdateOrderDisplay(this.Order);
        StartCoroutine(Waiting());
    }

    private void Update()
    {
        if (state != State.Angry || Health <= 0) return;

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
        while (Patience > 0)
        {
            Patience -= Time.deltaTime;
            yield return null;
        }

        Patience = 0;
        // yield return new WaitForSeconds(Patience);
        customerUI.timerSlider.gameObject.SetActive(false);
        BecomeAngry();
    }


    private void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        if (GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture != eatTexture_angry)
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
        if (state == State.Angry && other.GetComponent<PlayerHealth>() != null)
        {
            other.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
        }
    }

    private void BecomeAngry()
    {
        state = State.Angry;
        GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = walkTexture;
        customerUI.orderPanel.SetActive(false);
        GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(rageSound);
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
            Health -= pizza.Damage;
            if (Health > 0)
            {
                if (pizza.CustomerEffect != null)
                    StartCoroutine(StartEffect(pizza.CustomerEffect));
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(hurtSound);
            }
        }
        else if (state == State.Hungry)
        {
            transform.LookAt(player);
            StartCoroutine(Eat());
            Order.Receive(pizza);
            customerUI.UpdateOrderDisplay(Order);
            if (Order.IsFulfilled())
            {
                GameObject.Find("Audio Source").GetComponent<AudioSource>().PlayOneShot(completeSound);
                StopAllCoroutines();
                player.GetComponent<Player>().playerInventory.IncreaseMoney(Order.Value);
                OnFed?.Invoke(this);
                StartCoroutine(RemoveCustomer());
            }
            // else
            // Debug.Log($"Order not yet fulfilled, still needs: {Order.PasteOrderContents()}");
        }
    }

    private IEnumerator RemoveCustomer()
    {
        LogComponentStatus();
        if (boxCollider != null)
            boxCollider.enabled = false;

        var customerIndicator = GetComponent<CustomerIndicator>();
        if (customerIndicator != null)
            Destroy(customerIndicator);

        if (agent != null)
            agent.enabled = false;

        var capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
            capsuleCollider.enabled = false;

        if (state == State.Angry)
        {
            var renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer != null && idleTexture != null)
                renderer.material.mainTexture = idleTexture;
        }

        if (animator != null)
            animator.Play("Celebrate");

        if (customerUI != null)
        {
            if (customerUI.healthBar != null)
                customerUI.healthBar.gameObject.SetActive(false);

            if (customerUI.timerSlider != null)
                customerUI.timerSlider.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(1.2f);
        StopAllCoroutines();
        Destroy(gameObject);
    }

    // Add a separate method for debugging if needed
    private void LogComponentStatus()
    {
        Debug.Log($"CustomerUI: {customerUI}, BoxCollider: {boxCollider}, Agent: {agent}, Animator: {animator}");

        if (customerUI == null) Debug.LogError("customerUI is null!");
        if (boxCollider == null) Debug.LogError("boxCollider is null!");
        if (agent == null) Debug.LogError("agent is null!");
        if (animator == null) Debug.LogError("animator is null!");
    }

    private IEnumerator Eat()
    {
        if (state == State.Angry)
        {
            Texture originalTexture = GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture;
            GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = eatTexture_angry;
            yield return new WaitForSeconds(0.25f);
            GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = originalTexture;
        }
        else if (state == State.Hungry)
        {
            Texture originalTexture = GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture;
            //Eating texture loop:
            float eatingDuration = 1f;
            float elapsedTime = 0f;
            float eatingSwtichTimeTextures = 0.3f;
            while (elapsedTime < eatingDuration)
            {
                GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = eatTexture_hungry2;
                yield return new WaitForSeconds(eatingSwtichTimeTextures);
                GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = eatTexture_hungry1;
                yield return new WaitForSeconds(eatingSwtichTimeTextures);
                elapsedTime += 2 * eatingSwtichTimeTextures;
            }
            GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture = originalTexture;
        }
    }

    #region Effects

    private IEnumerator StartEffect(IEffect effect)
    {
        // Check if effect is duplicate
        var existingEffect = activeEffects
            .FirstOrDefault(t => t.Type == effect.Type);
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
        switch (effect.Type)
        {
            case EffectType.Poison:
                poisonParticles.Play();
                Health -= effect.Value;
                break;
            case EffectType.Stun:
                agent.speed *= effect.Value;
                animator.SetTrigger("stun");
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
        if (effect.Type == EffectType.Stun)
        {
            agent.speed = agentBaseSpeed;
            animator.enabled = true;
            animator.SetTrigger("stopStun");
        }
    }

    #endregion
}
