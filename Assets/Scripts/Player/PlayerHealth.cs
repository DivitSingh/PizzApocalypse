using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float startingHealth = 100f;

    private float currentHealth;
    public float CurrentHpPct => Mathf.Max(0, CurrentHealth / startingHealth);
    public float CurrentHealth
    {
        get => currentHealth;
        private set
        {
            currentHealth = Mathf.Min(value, startingHealth);
            OnHpPctChanged?.Invoke(CurrentHpPct);
            OnHealthChanged?.Invoke(CurrentHealth);
            if (currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }

    public event Action<float> OnHpPctChanged;
    public event Action<float> OnHealthChanged;
    public event Action OnDamageTaken;
    public event Action OnDeath;

    private void Start()
    {
        currentHealth = startingHealth;
        OnHpPctChanged?.Invoke(CurrentHpPct);
        OnHealthChanged?.Invoke(CurrentHealth);
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
    }
}