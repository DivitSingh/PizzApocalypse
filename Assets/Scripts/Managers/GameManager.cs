using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private RoundManager roundManager;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private ShopUI shopUI;
    
    // TODO: Move this to menu manager to set active again?
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
            HandleGameOver();;
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
        Show(roundManager.Score);
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
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void Unpause()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    public void Show(int score)
    {
        scoreText.text = $"You made {score} {(score == 1 ? "delivery" : "deliveries")}.";
        gameOverScreen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(gameOverScreen.transform.Find("RestartButton").gameObject);
    }

    public void Restart()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerHealth>().OnDeath -= HandleGameOver;
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScene");
    }
}