using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float startingHealth = 100f;
    private float currentHealth;

    public event Action<float> OnHpPctChanged; // Notifier for health percentage change
    public event Action OnDeath;
    
    private void Start()
    {
        currentHealth = startingHealth;
    }

    private float CurrentHpPct => Mathf.Max(0, currentHealth / startingHealth);

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnHpPctChanged?.Invoke(CurrentHpPct);
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }
}
