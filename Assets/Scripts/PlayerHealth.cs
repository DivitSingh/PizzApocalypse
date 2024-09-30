using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float startingHealth = 100f;

    private float _currentHealth; // Backing Field
    private float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            OnHpPctChanged?.Invoke(CurrentHpPct);
        }
    }

    public event Action<float> OnHpPctChanged;
    public event Action OnDeath;

    private void Start()
    {
        _currentHealth = startingHealth;
        OnHpPctChanged?.Invoke(CurrentHpPct); // Add this line
    }

    private float CurrentHpPct => Mathf.Max(0, CurrentHealth / startingHealth);

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, startingHealth);
    }
}