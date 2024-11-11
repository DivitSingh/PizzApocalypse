using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public int money = 0;

    // Maximum ammo limit
    [SerializeField] private int maxAmmo = 25;

    // Inventory to hold pizza amounts
    private Dictionary<PizzaType, int> pizzaInventory = new Dictionary<PizzaType, int>();

    // Currently equipped pizza
    private PizzaType equippedPizza;

    // List of all pizza types to facilitate cycling
    private List<PizzaType> pizzaTypes;

    // UI Elements
    public Image pizzaIcon; // To display the pizza icon
    public Image pizzaIconUnequipped_2; // To display the pizza icon of the second but unequipped type
    public Image pizzaIconUnequipped_3; // To display the pizza icon of the third but unequipped type
    public Image pizzaReticule;
    public TextMeshProUGUI ammoText; // To display the ammo count

    // Pizza icons for UI
    public Sprite cheeseIcon;
    public Sprite pineappleIcon;
    public Sprite mushroomIcon;
    public Sprite cheeseReticule;
    public Sprite pineappleReticule;
    public Sprite mushroomReticule;

    public void InitializeInventory()
    {
        // Initialize pizza inventory with some default values
        pizzaInventory[PizzaType.Cheese] = maxAmmo;
        pizzaInventory[PizzaType.Pineapple] = maxAmmo;
        pizzaInventory[PizzaType.Mushroom] = maxAmmo;

        // Equip default pizza (Cheese)
        equippedPizza = PizzaType.Cheese;

        // Create a list of all available pizza types
        pizzaTypes = new List<PizzaType>
        {
            PizzaType.Cheese,
            PizzaType.Pineapple,
            PizzaType.Mushroom
        };

        // Update the UI at the start
        UpdateUI();
    }

    // Method to cycle forward in the inventory
    public void SwitchPizzaForward()
    {
        int currentIndex = pizzaTypes.IndexOf(equippedPizza);
        int nextIndex = (currentIndex + 1) % pizzaTypes.Count;
        equippedPizza = pizzaTypes[nextIndex];
        Debug.Log("Switched to: " + equippedPizza);
        UpdateUI(); // Update UI after switching
    }

    // Method to cycle backward in the inventory
    public void SwitchPizzaBackward()
    {
        int currentIndex = pizzaTypes.IndexOf(equippedPizza);
        int previousIndex = (currentIndex - 1 + pizzaTypes.Count) % pizzaTypes.Count;
        equippedPizza = pizzaTypes[previousIndex];
        Debug.Log("Switched to: " + equippedPizza);
        UpdateUI(); // Update UI after switching
    }

    // Getter for the currently equipped pizza
    public PizzaType GetEquippedPizza()
    {
        return equippedPizza;
    }

    // Getter for current ammo for a specific pizza type
    public int GetPizzaAmmo(PizzaType pizzaType)
    {
        return pizzaInventory[pizzaType];
    }

    public void LosePizzas(int quantity)
    {
        if (pizzaInventory[equippedPizza] >= quantity)
        {
            pizzaInventory[equippedPizza] -= quantity;
            Debug.Log(quantity + " amount of type:" + equippedPizza + " pizza used! Remaining: " + pizzaInventory[equippedPizza]);
        }
        else
        {
            Debug.Log("No more " + equippedPizza + " pizza left.");
        }
        UpdateUI();
    }

    // Method to add more pizzas for specific type (reload)
    // public void RestockSpecificPizzaType(PizzaType pizzaType, int amount)
    // {
    //     if (pizzaInventory[pizzaType] + amount > maxAmmo)
    //     {
    //         pizzaInventory[pizzaType] = maxAmmo;
    //     }
    //     else
    //     {
    //         pizzaInventory[pizzaType] += amount;
    //     }

    //     UpdateUI();
    //     Debug.Log(pizzaType + " pizza restocked. Current ammo: " + pizzaInventory[pizzaType]);
    // }

    // Method to restock pizzas
    public void RestockPizzas()
    {
        pizzaInventory[PizzaType.Cheese] = maxAmmo;
        pizzaInventory[PizzaType.Pineapple] = maxAmmo;
        pizzaInventory[PizzaType.Mushroom] = maxAmmo;

        UpdateUI();
        Debug.Log("All pizzas restocked to max ammo.");
    }

    // Method to update the UI when pizza is switched
    public void UpdateUI()
    {
        // Update the pizza icon based on the equipped pizza
        switch (equippedPizza)
        {
            case PizzaType.Cheese:
                pizzaReticule.sprite = cheeseIcon;
                pizzaIcon.sprite = cheeseIcon;
                pizzaIconUnequipped_2.sprite = pineappleIcon;
                pizzaIconUnequipped_3.sprite = mushroomIcon;
                break;
            case PizzaType.Pineapple:
                pizzaReticule.sprite = pineappleIcon;
                pizzaIcon.sprite = pineappleIcon;
                pizzaIconUnequipped_2.sprite = mushroomIcon;
                pizzaIconUnequipped_3.sprite = cheeseIcon;
                break;
            case PizzaType.Mushroom:
                pizzaReticule.sprite = mushroomIcon;
                pizzaIcon.sprite = mushroomIcon;
                pizzaIconUnequipped_2.sprite = cheeseIcon;
                pizzaIconUnequipped_3.sprite = pineappleIcon;
                break;
        }

        // Update the ammo text to show the current amount of pizza ammo
        ammoText.text = pizzaInventory[equippedPizza].ToString();
    }

    public void IncreaseCapacity(int amount)
    {
        maxAmmo += amount;
        RestockPizzas();
        UpdateUI();
    }

    public void IncreaseMoney(int amount)
    {
        money += amount;
    }
}
