using System;
using UnityEngine;

/// <summary>
/// Handles showing and hiding UI elements.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject roundUI;
    [SerializeField] private GameObject shopUI;
    [SerializeField] private GameObject playerStatsUI;
    [SerializeField] private GameObject ammoUI;
    [SerializeField] private GameOverScreen gameOverScreen;

    public void HandleNewRound()
    {
        shopUI.SetActive(false);
        roundUI.SetActive(true);
        ammoUI.SetActive(true);
        playerStatsUI.SetActive(true);
    }
    
    public void HandleRoundPassed()
    {
        roundUI.SetActive(false);
        ammoUI.SetActive(false);
        playerStatsUI.SetActive(false);
        shopUI.SetActive(true);
    }

    public void HandleGameOver(int round)
    {
        roundUI.SetActive(false);
        ammoUI.SetActive(false);
        playerStatsUI.SetActive(false);
        gameOverScreen.Show(round);
    }
}