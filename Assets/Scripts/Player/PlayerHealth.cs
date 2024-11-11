using System;
using UnityEngine;

public class PlayerHealth: MonoBehaviour
{
    [SerializeField] private float startingHealth = 100f;

    private float _currentHealth;
    private float CurrentHpPct => Mathf.Max(0, CurrentHealth / startingHealth);

    private float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            OnHpPctChanged?.Invoke(CurrentHpPct);
            if (_currentHealth <= 0)
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
        _currentHealth = startingHealth;
        OnHpPctChanged?.Invoke(CurrentHpPct);
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