using System;
using UnityEngine;
using UnityEngine.Serialization;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private CustomerSpawner customerSpawner;
    [SerializeField] private CustomersManager customersManager;

    private RoundProgression roundProgression = new RoundProgression();

    public int Score { get; private set; }
    public int Round { get; private set; } = 1;
    private float timeRemaining;

    // Events
    public event Action<float> OnTimeRemainingChanged;
    public event Action<int, int> OnProgressChanged;
    
    public event Action<bool> OnRoundEnd;
    public event Action<int> OnNewRound;

    private void Awake()
    {
        customerSpawner.OnSpawned += HandleCustomerSpawned;
        customersManager.OnAllCustomersHandled += HandleRoundEnd;
        customersManager.OnOrderStatusChanged += HandleOrderStatusChanged;
    }

    private void Start()
    {
        timeRemaining = roundProgression.Configuration.Duration;
        OnTimeRemainingChanged?.Invoke(timeRemaining);
        OnProgressChanged?.Invoke(Score, roundProgression.Configuration.PassScore);
        customerSpawner.StartSpawning(roundProgression.Configuration.SpawnConfiguration, roundProgression.Configuration.CustomerConfiguration);
        customersManager.Configure(roundProgression.Configuration.SpawnConfiguration.Count);
    }

    private void Update()
    {
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

    public void HandleOrderStatusChanged(Customer customer, bool success)
    {
        if (!success) return;
        Score++;
        OnProgressChanged?.Invoke(Score, roundProgression.Configuration.PassScore);
    }

    private void HandleRoundEnd()
    {
        customersManager.Reset();
        var didPass = Score >= roundProgression.Configuration.PassScore;
        OnRoundEnd?.Invoke(didPass);
    }

    public void NextRound()
    {
        Score = 0;
        Round++;
        roundProgression.Next();
        
        // Invoke delegates
        OnNewRound?.Invoke(Round);
        OnProgressChanged?.Invoke(Score, roundProgression.Configuration.PassScore);
        OnTimeRemainingChanged?.Invoke(timeRemaining);
        
        GameObject.Find("Audio Source").GetComponent<AudioSource>().Stop();
        GameObject.Find("Audio Source").GetComponent<AudioSource>().Play();
        
        customersManager.Configure(roundProgression.Configuration.SpawnConfiguration.Count);
        customerSpawner.StartSpawning(roundProgression.Configuration.SpawnConfiguration, roundProgression.Configuration.CustomerConfiguration);
        timeRemaining = roundProgression.Configuration.Duration;
    }

    private void HandleCustomerSpawned(Customer customer)
    {
        customersManager.Add(customer);
    }
}