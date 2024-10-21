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
        if (Time.timeScale == 0) return;
        
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
        Score++;
        OnProgressChanged?.Invoke(Score, passScore);

    }

    private void HandleRoundEnd()
    {
        foreach (var customer in FindObjectsOfType<Customer>())
        {
            Destroy(customer.gameObject);
        }

        if (Score < passScore)
        {
            OnRoundFailed?.Invoke();    
        }
        else
        {
            OnRoundChanged?.Invoke(++Round);
        }
    }

    public void NextRound()
    {
        // TODO: Need to fine tune these values, make sure round is still possible
        // NOTE: Values are currently temporary, anything past first round should be ignored
        Score = 0;
        passScore++;
        timeRemaining = roundDuration + 5;
        customerPatience--;
        totalCustomers++;
        
        // TODO: Modify other customer stats
        OnProgressChanged?.Invoke(Score, passScore);
        OnTimeRemainingChanged?.Invoke(timeRemaining);
        customerSpawner.StartSpawning(spawnInterval, totalCustomers, customerHealth, customerPatience, customerAttackDmg);
    }
}