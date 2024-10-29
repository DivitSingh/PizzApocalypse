using System;
using UnityEngine;
using UnityEngine.Serialization;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private CustomerSpawner customerSpawner;
    [SerializeField] private CustomersManager customersManager;

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
        timeRemaining = roundDuration;
        OnTimeRemainingChanged?.Invoke(timeRemaining);
        OnProgressChanged?.Invoke(Score, passScore);
        customerSpawner.StartSpawning(spawnInterval, totalCustomers, customerHealth, customerPatience, customerAttackDmg);
        customersManager.Configure(totalCustomers);
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
        OnProgressChanged?.Invoke(Score, passScore);
    }

    private void HandleRoundEnd()
    {
        customersManager.Reset();
        var didPass = Score >= passScore;
        OnRoundEnd?.Invoke(didPass);
    }

    public void NextRound()
    {
        // TODO: Need to fine tune these values, make sure round is still possible
        // NOTE: Values are currently temporary, anything past first round should be ignored
        Score = 0;
        Round++;
        // TODO: Temporary hardcoded values
        switch (Round)
        {
            case 2:
                passScore = 4;
                customerPatience = 8;
                totalCustomers = 10;
                spawnInterval = 6;
                timeRemaining = 60;
                customerHealth += 15;
                break;
            case 3:
                passScore = 6;
                customerPatience = 6;
                totalCustomers = 12;
                spawnInterval = 5;
                timeRemaining = 60;
                customerAttackDmg += 5;
                customerHealth += 25;
                break;
            default:
                // TODO: Figure out formula for future rounds
                passScore++;
                timeRemaining = roundDuration + 5;
                totalCustomers++;
                customerAttackDmg += 5;
                customerHealth += 10;
                break;
        };
        

        // TODO: Modify other customer stats
        customersManager.Configure(totalCustomers);
        OnNewRound?.Invoke(Round);
        OnProgressChanged?.Invoke(Score, passScore);
        OnTimeRemainingChanged?.Invoke(timeRemaining);
        GameObject.Find("Audio Source").GetComponent<AudioSource>().Stop();
        GameObject.Find("Audio Source").GetComponent<AudioSource>().Play();
        customerSpawner.StartSpawning(spawnInterval, totalCustomers, customerHealth, customerPatience, customerAttackDmg);
    }

    private void HandleCustomerSpawned(Customer customer)
    {
        customersManager.Add(customer);
    }
}