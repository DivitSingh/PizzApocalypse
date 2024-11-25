using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles showing and hiding UI elements.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private GameObject roundUI;
    [SerializeField] private GameObject playerStatsUI;
    [SerializeField] private GameObject ammoUI;
    [SerializeField] private GameObject orderBarUI;
    
    
    [Header("Screens")]
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private GameObject shopUI;

    private List<GameObject> hudElements;

    private void Awake()
    {
        hudElements = new List<GameObject> { roundUI, playerStatsUI, ammoUI, orderBarUI };
    }

    public void HandleNewRound()
    {
        shopUI.SetActive(false);
        ToggleHUD();
    }
    
    public void HandleRoundPassed()
    {
        ToggleHUD();
        shopUI.SetActive(true);
    }

    public void HandleGameOver(int round)
    {
        ToggleHUD();
        gameOverScreen.Show(round);
    }

    private void ToggleHUD()
    {
        foreach (var elem in hudElements)
        {
            elem.SetActive(!elem.activeSelf);
        }
    }
}