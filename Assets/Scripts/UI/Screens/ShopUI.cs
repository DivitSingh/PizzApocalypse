using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO: Need to reference text and price elements
// TODO: Add onBalanceChanged delegate
public class ShopUI : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;


    [Header("UI Elements")]
    [SerializeField] private TMP_Text balanceText;
    
    [SerializeField] private GameObject healthButton;
    [SerializeField] private GameObject damageButton;
    [SerializeField] private GameObject capacityButton;

    public void Show()
    {
        Debug.Log(EventSystem.current.currentInputModule.ToString());
        EventSystem.current.SetSelectedGameObject(healthButton.gameObject);
        gameObject.SetActive(true);
    }

    public void HandleHealthClicked()
    {
        Debug.Log("Health called");
        shopManager.UpgradeHealth();
    }

    public void HandleAttackClicked()
    {
        Debug.Log("Damage clicked");
        shopManager.UpgradeDamage();
    }

    public void HandleCapacityClicked()
    {
        Debug.Log("Capacity called");
        // TODO: Update UI on success?
        shopManager.UpgradeCapacity();
    }
}