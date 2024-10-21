using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;

    // TODO: Refactor
    [Header("UI Elements")]
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private GameObject healthContainer;
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

    private BuffComponents healthComponents;
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

    private void OnEnable()
    {
        // Update UI on first load
        if (firstLoad)
        {
            healthComponents = ExtractComponents(healthContainer);
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
    }

    private void OnDisable()
    {
        shopManager.OnBuffPurchased -= HandlePurchasedBuff;
        shopManager.OnBalanceChanged -= HandleBalanceChanged;
    }

    public void Show()
    {
        var firstButton = GetComponentsInChildren<Button>()[0];
        EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void HandleHealthClicked()
    {
        shopManager.UpgradeHealth();
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
    }
    
    #endregion
   

    private TMP_Text GetLevelText(BuffType type)
    {
        return (type) switch
        {
            BuffType.Health => healthComponents.LevelText,
            BuffType.Capacity => capacityComponents.LevelText,
            BuffType.Damage => damageComponents.LevelText
        };
    }
    
    private TMP_Text GetCostText(BuffType type)
    {
        return (type) switch
        {
            BuffType.Health => healthComponents.CostText,
            BuffType.Capacity => capacityComponents.CostText,
            BuffType.Damage => damageComponents.CostText
        };
    }

    private void UpdateDescription(Buff buff)
    {
        switch (buff.Type)
        {
            case BuffType.Health:
                healthComponents.DescriptionText.text = $"+{buff.IncreaseAmount} Max Health";
                break;
            case BuffType.Capacity:
                capacityComponents.DescriptionText.text = $"+{buff.IncreaseAmount} Pizza Capacity";
                break;
            case BuffType.Damage:
                damageComponents.DescriptionText.text = $"+{buff.IncreaseAmount} Base Damage";
                break;
        }
    }
}