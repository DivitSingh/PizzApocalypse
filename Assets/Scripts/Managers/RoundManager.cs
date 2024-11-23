using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // Tutorial
    private string sceneName = "";
    private TutorialState currentTutorialState = TutorialState.FindCustomer;
    private TextMeshProUGUI tutorialText;

    private enum TutorialState
    {
        FindCustomer,
        FeedCustomer,
        Restock,
        AngryCustomer,
        Complete
    }

    private void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;
        customerSpawner.OnSpawned += HandleCustomerSpawned;
        customersManager.OnAllCustomersHandled += HandleRoundEnd;
        customersManager.OnOrderStatusChanged += HandleOrderStatusChanged;
    }

    private void Start()
    {
        if (sceneName != "TutorialScene")
        {
            timeRemaining = roundProgression.Configuration.Duration;
            OnTimeRemainingChanged?.Invoke(timeRemaining);
            OnProgressChanged?.Invoke(Score, roundProgression.Configuration.PassScore);
            customerSpawner.StartSpawning(roundProgression.Configuration.SpawnConfiguration, roundProgression.Configuration.CustomerConfiguration);
            customersManager.Configure(roundProgression.Configuration.SpawnConfiguration.Count);
        }
        else
        {
            timeRemaining = -1;
            Round = -1;
            tutorialText = GameObject.Find("Tutorial Text").GetComponent<TextMeshProUGUI>();
            customerSpawner.SpawnHungryCustomer();
        }
    }

    private void Update()
    {
        if (sceneName == "TutorialScene")
        {
            if (currentTutorialState == TutorialState.FindCustomer)
            {
                tutorialText.text = "Look at the order number on the order display to identify the customer's order number. Use the navigation guide with the matching order number to find the customer's location.";
                if (GameObject.Find("Hungry Customer") == null || Vector3.Distance(GameObject.Find("Player").transform.position, GameObject.Find("Hungry Customer").transform.position) < 5.0f)
                    currentTutorialState = TutorialState.FeedCustomer;
            }
            else if (currentTutorialState == TutorialState.FeedCustomer)
            {
                tutorialText.text = "Now that you have found the customer, use the swap keys to switch between pizzas till you get the right one for the order. Then shoot at the customer to complete the delivery!";
                if (GameObject.Find("Hungry Customer") == null)
                    currentTutorialState = TutorialState.Restock;
            }
            else if (currentTutorialState == TutorialState.Restock)
            {
                tutorialText.text = "There are 2 Pizza Centres, located in the North and South of the town. Go to one of these centres and stand on the pizza icon to restock pizzas.";
                if (GameObject.Find("Player").GetComponent<PlayerInventory>().PizzasAreFull())
                {
                    customerSpawner.SpawnAngryCustomer();
                    currentTutorialState = TutorialState.AngryCustomer;
                }
            }
            else if (currentTutorialState == TutorialState.AngryCustomer)
            {
                if (GameObject.Find("Angry Customer") == null)
                {
                    tutorialText.gameObject.SetActive(false);
                    GameObject.Find("Game Manager").GetComponent<GameManager>().HandleGameOver();
                    currentTutorialState = TutorialState.Complete;
                }
                else
                    tutorialText.text = "An angry customer has arrived! Normally, a customer only becomes angry if you don't deliver on time. Fire at the customer to increase their satisfaction so they no longer bother you. Different pizzas have different effects!";
            }

            if (Time.timeScale == 0 || timeRemaining == -1) return;

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

        customersManager.Configure(roundProgression.Configuration.SpawnConfiguration.Count);
        customerSpawner.StartSpawning(roundProgression.Configuration.SpawnConfiguration, roundProgression.Configuration.CustomerConfiguration);
        timeRemaining = roundProgression.Configuration.Duration;
    }

    private void HandleCustomerSpawned(Customer customer)
    {
        customersManager.Add(customer);
    }
}