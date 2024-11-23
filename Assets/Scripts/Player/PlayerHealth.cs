using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] public float startingHealth = 100f;

    public float currentHealth;
    private float CurrentHpPct => Mathf.Max(0, CurrentHealth / startingHealth);

    private float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;
            OnHpPctChanged?.Invoke(CurrentHpPct);
            if (currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }

    public event Action<float> OnHpPctChanged;
    public event Action OnDamageTaken;
    public event Action OnDeath;

    public void Start()
    {
        currentHealth = startingHealth;
        OnHpPctChanged?.Invoke(CurrentHpPct);
        GameManager.Instance.OnRoundStarting += () => CurrentHealth = startingHealth;
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        OnDamageTaken?.Invoke();
    }

    public void Heal(float amount)
    {
        CurrentHealth += amount;
        if (currentHealth > startingHealth)
            currentHealth = startingHealth;
    }
}