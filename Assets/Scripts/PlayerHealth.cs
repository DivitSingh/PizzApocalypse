using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float startingHealth = 100f;
    private float currentHealth;
    public event Action<float> OnHpPctChanged;
    public event Action OnDeath;
    [SerializeField] private UnityEngine.UI.Text debugText;

    private void Start()
    {
        currentHealth = startingHealth;
        UpdateDebugText();
        OnHpPctChanged?.Invoke(CurrentHpPct); // Add this line
    }

    private float CurrentHpPct => Mathf.Max(0, currentHealth / startingHealth);

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnHpPctChanged?.Invoke(CurrentHpPct);
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");
        UpdateDebugText();

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Debug.Log("Player died!");
        }
    }

    private void UpdateDebugText()
    {
        if (debugText != null)
        {
            debugText.text = $"Health: {currentHealth} / {startingHealth}";
        }
    }

    public void TestDamage(float amount)
    {
        TakeDamage(amount);
    }
}