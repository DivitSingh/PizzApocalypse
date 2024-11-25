using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;

/// <summary>
/// Handles updating the UI to reflect the player stats (Health, Money).
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text moneyText;

    [Header("Dependencies")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerInventory playerInventory;

    private void Awake()
    {
        playerHealth.OnHealthChanged += UpdateHealth;
        playerInventory.OnBalanceChanged += UpdateBalance;
    }

    private void OnEnable()
    {
        // Need to update when shown as script is disabled between rounds
        UpdateHealth(playerHealth.CurrentHealth);
        UpdateBalance(playerInventory.money);
    }

    private void UpdateHealth(float health)
    {
        healthText.text = $"{health}";
    }

    private void UpdateBalance(int balance)
    {
        moneyText.text = $"{balance}";
    }
}