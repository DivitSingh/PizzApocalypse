using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private RoundManager roundManager;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMP_Text scoreText;
    
    // TODO: [Remove]: Temporary references for completed round screen
    [SerializeField] private GameObject roundPassedScreen;
    [SerializeField] private TMP_Text roundScoreText;

    public event Action OnGameOver;

    private void Awake()
    {
        Instance = this;
        GameObject.FindWithTag("Player").GetComponent<PlayerHealth>().OnDeath += HandleGameOver;
        roundManager.OnRoundFailed += HandleGameOver;
        roundManager.OnRoundChanged += HandleRoundPassed;

        // Undo changes from paused state
        Time.timeScale = 1;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        OnGameOver?.Invoke();
        
        // Delete customers
        foreach (var customer in FindObjectsOfType<Customer>())
        {
            Destroy(customer.gameObject);
        }
        
        Show(roundManager.Score);
    }

    // TODO: REMOVE, Temporarily shows round passed screen, remove
    private void HandleRoundPassed(int _)
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        OnGameOver?.Invoke();
        roundScoreText.text = $"You made {roundManager.Score} {(roundManager.Score == 1 ? "delivery" : "deliveries")}.";
        roundPassedScreen.SetActive(true);
    }

    public void HandleFedCustomerScoring(Customer customer)
    {
        roundManager.HandleFedCustomer(customer);
    }

    public void Show(int score)
    {
        scoreText.text = $"You made {score} {(score == 1 ? "delivery" : "deliveries")}.";
        gameOverScreen.SetActive(true);
    }

    public void Restart()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerHealth>().OnDeath -= HandleGameOver;
        SceneManager.LoadScene(0);
    }
}