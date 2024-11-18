using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private RoundManager roundManager;

    [Header("UI Screens")]
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private GameObject roundUI;

    public event Action OnGameOver;
    public event Action OnRoundStarting;

    private void Awake()
    {
        Instance = this;
        GameObject.FindWithTag("Player").GetComponent<PlayerHealth>().OnDeath += HandleGameOver;
        roundManager.OnRoundEnd += HandleRoundEnd;
        Unpause();
    }

    private void HandleRoundEnd(bool didPass)
    {
        if (didPass)
        {
            HandleRoundPassed();
        }
        else
        {
            HandleGameOver(); ;
        }
    }

    private void HandleGameOver()
    {
        foreach (var customer in FindObjectsOfType<Customer>())
        {
            Destroy(customer.gameObject);
        }

        Pause();
        OnGameOver?.Invoke();
        gameOverScreen.Show(roundManager.Round);
    }

    private void HandleRoundPassed()
    {
        Pause();
        shopUI.Show();
    }

    public void StartNextRound()
    {
        roundUI.SetActive(true);
        shopUI.Hide();
        roundManager.NextRound();
        OnRoundStarting?.Invoke();
        Unpause();
    }

    private void Pause()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void Unpause()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    public void Restart()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerHealth>().OnDeath -= HandleGameOver;
        SceneManager.LoadScene("LevelScene");
    }

    /// <summary>
    /// Exits the scene and loads the main menu.
    /// </summary>
    public void Exit()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerHealth>().OnDeath -= HandleGameOver;
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }
}