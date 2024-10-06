using System;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Initial Stats")]
    [SerializeField] private float roundDuration = 180f;
    // [SerializeField] private float spawnInterval = 5f;
    // [SerializeField] private int totalCustomers = 10;
    [SerializeField] private int passScore = 3;
    // [SerializeField] private float customerHealth = 100f;
    // [SerializeField] private float customerPatience = 5f;


    public int Score { get; private set; }
    public int Round { get; private set; } = 1;
    private float timeRemaining;

    // Events
    public event Action<float> OnTimeRemainingChanged;
    public event Action<int, int> OnProgressChanged;
    public event Action OnRoundFailed;

    private void Start()
    {
        timeRemaining = roundDuration;
        OnTimeRemainingChanged?.Invoke(timeRemaining);
        OnProgressChanged?.Invoke(Score, passScore);
    }

    private void Update()
    {
        // TODO: Will eventually need to check if game is paused to not reduce timer
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            OnTimeRemainingChanged?.Invoke(timeRemaining);
        }
        else
        {
            HandleRoundEnd();
        }
    }

    public void HandleFedCustomer(Customer customer)
    {
        // TODO: Eventually add tips/score differently based on customer?
        Score++;
        OnProgressChanged?.Invoke(Score, passScore);

    }

    private void HandleRoundEnd()
    {
        if (Score < passScore)
        {
            OnRoundFailed?.Invoke();    
        }
        else
        {
            // TODO: Indicate that round was completed successfully
        }
    }
}