using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private RoundManager roundManager;
    [SerializeField] private UIManager uiManager;

    public event Action OnGameOver;
    public event Action OnRoundStarting;

    public bool isPaused { get; private set; }

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

    public void HandleGameOver()
    {
        foreach (var customer in FindObjectsOfType<Customer>())
        {
            Destroy(customer.gameObject);
        }

        Pause();
        OnGameOver?.Invoke();
        uiManager.HandleGameOver(roundManager.Round);
    }

    private void HandleRoundPassed()
    {
        Pause();
        uiManager.HandleRoundPassed();
    }

    public void StartNextRound()
    {
        uiManager.HandleNewRound();
        roundManager.NextRound();
        OnRoundStarting?.Invoke();
        Unpause();
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void Unpause()
    {
        isPaused = false;
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