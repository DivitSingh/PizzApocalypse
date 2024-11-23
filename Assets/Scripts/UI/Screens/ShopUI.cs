using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;

    // Add these new audio-related fields
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip completeSound;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private GameObject speedContainer;
    [SerializeField] private GameObject damageContainer;
    [SerializeField] private GameObject capacityContainer;

    private class BuffComponents
    {
        public TMP_Text LevelText;
        public TMP_Text DescriptionText;
        public TMP_Text CostText;

        public BuffComponents(TMP_Text levelText, TMP_Text descriptionText, TMP_Text costText)
        {
            LevelText = levelText;
            DescriptionText = descriptionText;
            CostText = costText;
        }
    }

    private BuffComponents speedComponents;
    private BuffComponents damageComponents;
    private BuffComponents capacityComponents;

    private bool firstLoad = true;

    private BuffComponents ExtractComponents(GameObject buffContainer)
    {
        // Extract relevant UI components for the given buff container
        var textComponents = buffContainer.GetComponentsInChildren<TMP_Text>();
        var levelText = textComponents.FirstOrDefault(t => t.name == "LevelText");
        var descriptionText = textComponents.FirstOrDefault(t => t.name == "BuffDescriptionText");
        var costText = buffContainer.GetComponentInChildren<Button>().GetComponentInChildren<TMP_Text>();
        return new BuffComponents(levelText, descriptionText, costText);
    }

    private void Awake()
    {
        Debug.Log("Awake called");
    }

    private void OnEnable()
    {
        // Update UI on first load
        if (firstLoad)
        {
            speedComponents = ExtractComponents(speedContainer);
            damageComponents = ExtractComponents(damageContainer);
            capacityComponents = ExtractComponents(capacityContainer);
            foreach (var buff in shopManager.Buffs)
            {
                HandlePurchasedBuff(buff);
            }

            firstLoad = false;
        }
        
        HandleBalanceChanged(shopManager.Balance); // Need to update each time screen is shown
        shopManager.OnBuffPurchased += HandlePurchasedBuff;
        shopManager.OnBalanceChanged += HandleBalanceChanged;
        
        // Listen to input changes
        InputDeviceManager.Instance.OnGamepadStatusChanged += HandleDeviceChange;
        Cursor.visible = !InputDeviceManager.Instance.IsGamepad;
    }

    private void OnDisable()
    {
        shopManager.OnBuffPurchased -= HandlePurchasedBuff;
        shopManager.OnBalanceChanged -= HandleBalanceChanged;
        InputDeviceManager.Instance.OnGamepadStatusChanged -= HandleDeviceChange;
    }

    private void HandleDeviceChange(bool isGamepad)
    {
        if (isGamepad)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            // Ensure that screen remains navigable when switching between inputs
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(GetComponentsInChildren<Button>()[1].gameObject);    
            } 
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        HandleDeviceChange(InputDeviceManager.Instance.IsGamepad);
        if (InputDeviceManager.Instance.IsGamepad)
        {
            var firstButton = GetComponentsInChildren<Button>()[0];
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void HandleSpeedClicked()
    {
        shopManager.UpgradeSpeed();
    }

    public void HandleAttackClicked()
    {
        shopManager.UpgradeDamage();
    }

    public void HandleCapacityClicked()
    {
        shopManager.UpgradeCapacity();
    }

    #region Event Handlers

    private void HandleBalanceChanged(int newBalance)
    {
        balanceText.text = $"${newBalance}";
    }

    /// <summary>
    /// Handles updating the UI when a buff has been purchased.
    /// </summary>
    /// <param name="buff">The newly upgraded buff.</param>
    private void HandlePurchasedBuff(Buff buff)
    {
        var levelText = GetLevelText(buff.Type);
        var costText = GetCostText(buff.Type);
        levelText.text = buff.Level.ToString();
        costText.text = $"${buff.Cost}";
        UpdateDescription(buff);

        // Disable Buff if max level reached
        if (buff.IsMaxed) DisableBuff(buff);
    }

    #endregion

    // Add these new methods for playing sounds
    private void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    private void PlayCompleteSound()
    {
        if (completeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completeSound);
        }
    }

    private TMP_Text GetLevelText(BuffType type)
    {
        return type switch
        {
            BuffType.Speed => speedComponents.LevelText,
            BuffType.Capacity => capacityComponents.LevelText,
            BuffType.Damage => damageComponents.LevelText,
            _ => throw new System.NotImplementedException()
        };
    }

    private TMP_Text GetCostText(BuffType type)
    {
        return type switch
        {
            BuffType.Speed => speedComponents.CostText,
            BuffType.Capacity => capacityComponents.CostText,
            BuffType.Damage => damageComponents.CostText,
            _ => throw new System.NotImplementedException()
        };
    }

    private void UpdateDescription(Buff buff)
    {
        switch (buff.Type)
        {
            case BuffType.Speed:
                speedComponents.DescriptionText.text = "Run Around Faster";
                break;
            case BuffType.Capacity:
                capacityComponents.DescriptionText.text = $"+{buff.IncreaseAmount} Pizza Capacity";
                break;
            case BuffType.Damage:
                damageComponents.DescriptionText.text = $"+{buff.IncreaseAmount} Base Damage";
                break;
        }
    }

    private void DisableBuff(Buff buff)
    {
        var container = buff.Type switch
        {
            BuffType.Speed => speedContainer,
            BuffType.Damage => damageContainer,
            BuffType.Capacity => capacityContainer,
            _ => throw new System.NotImplementedException()
        };
        var costText = buff.Type switch
        {
            BuffType.Speed => speedComponents.CostText,
            BuffType.Damage => damageComponents.CostText,
            BuffType.Capacity => capacityComponents.CostText,
            _ => throw new System.NotImplementedException()
        };

        var button = GetComponentInChildren<Button>(container);
        button.interactable = false;
        EventSystem.current.SetSelectedGameObject(transform.Find("Continue Button").gameObject);
        costText.text = "";
    }
}