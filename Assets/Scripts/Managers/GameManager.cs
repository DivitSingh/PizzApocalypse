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

    public event Action OnGameOver;

    private void Awake()
    {
        Player.money = 0; // TODO: Refactor this, money should not be static variable
        Instance = this;
        GameObject.FindWithTag("Player").GetComponent<PlayerHealth>().OnDeath += HandleGameOver;
        roundManager.OnRoundFailed += HandleGameOver;
        roundManager.OnRoundChanged += HandleRoundPassed;
        Unpause();
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

    private void HandleRoundPassed(int _)
    {
        Pause();
        shopUI.Show();
    }

    public void StartNextRound()
    {
        shopUI.Hide();
        roundManager.NextRound();
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

    public void HandleFedCustomerScoring(Customer customer)
    {
        roundManager.HandleFedCustomer(customer);
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
        SceneManager.LoadScene(0);
    }
}