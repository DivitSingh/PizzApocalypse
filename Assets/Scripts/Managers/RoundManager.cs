using System;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private CustomerSpawner customerSpawner;
    
    [Header("Initial Stats")]
    [SerializeField] private float roundDuration = 120f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int totalCustomers = 20;
    [SerializeField] private int passScore = 3;
    [SerializeField] private float customerHealth = 100f;
    [SerializeField] private float customerPatience = 8f;
    [SerializeField] private float customerAttackDmg = 20f;


    public int Score { get; private set; }
    public int Round { get; private set; } = 1;
    private float timeRemaining;

    // Events
    public event Action<float> OnTimeRemainingChanged;
    public event Action<int, int> OnProgressChanged;
    public event Action<int> OnRoundChanged;
    public event Action OnRoundFailed;
    // TODO: Add onRoundPassed event with stats?

    private void Start()
    {
        timeRemaining = roundDuration;
        OnTimeRemainingChanged?.Invoke(timeRemaining);
        OnProgressChanged?.Invoke(Score, passScore);
        customerSpawner.StartSpawning(spawnInterval, totalCustomers, customerHealth, customerPatience, customerAttackDmg);
    }

    private void Update()
    {
        // TODO: End round early if all customers have been fed or are dead?
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