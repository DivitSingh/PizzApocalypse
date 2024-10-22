using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Order
{
    private static int maxPizzaAmountOrder = 3;
    private static int maxPizzaTypePerOrder = 3;

    private Dictionary<PizzaType, int>[] Orders;
    private static System.Random random = new System.Random();

    private int originalOrderAmount;
    private float orderModifier;
    // Implement OG Count and MOdifier

    public Order()
    {
        int numOrders = random.Next(1, maxPizzaTypePerOrder);  // Randomly pick between 1 and maxPizzaTypePerOrder (static int) pizzas in the order
        Orders = new Dictionary<PizzaType, int>[numOrders];

        var availablePizzaTypes = Enum.GetValues(typeof(PizzaType));
        for (int i = 0; i < numOrders; i++)
        {
            Orders[i] = new Dictionary<PizzaType, int>();

            // Randomly choose a pizza type
            PizzaType selectedPizza = (PizzaType)availablePizzaTypes.GetValue(random.Next(availablePizzaTypes.Length));

            // Randomly assign count between 1 and maxPizzaAmountOrder (static int)
            int pizzaCount = random.Next(1, maxPizzaAmountOrder);

            Orders[i].Add(selectedPizza, pizzaCount);

            // Create originalOrderAmount
            originalOrderAmount += Orders[i].Values.Sum();
            Debug.Log("Original order amount: " + originalOrderAmount);

            // Calculate orderModifier
            orderModifier = CalculateModifier(Orders, originalOrderAmount);
        }
    }

    public Dictionary<PizzaType, int>[] GetOrders()
    {
        return Orders;
    }

    public float GetOrderModifier()
    {
        return orderModifier;
    }
    public float GetOriginalOrderAmount()
    {
        return originalOrderAmount;
    }

    public float CalculateModifier(Dictionary<PizzaType, int>[] Orders, int totalPizzas)
    {
        // Base modifier
        float modifier = 1.0f;

        // Calculate the variety modifier based on the number of different pizza types
        int differentTypes = 0;
        foreach (var pizzaTypeOrder in Orders)
        {
            differentTypes++;
        }
        Debug.Log("Different types: " + differentTypes);

        switch (differentTypes)
        {
            case 2:
                modifier += 0.2f;
                break;
            case 3:
                modifier += 0.5f;
                break;
            default:
                if (differentTypes > 3)
                {
                    modifier += 0.8f + (differentTypes - 3) * 0.3f;
                }
                break;
        }

        // Calculate the quantity modifier based on the number of pizzas in the order
        switch (totalPizzas)
        {
            case int n when (n >= 3 && n <= 6):
                modifier += 0.3f;
                break;
            case int n when (n > 6):
                modifier += 0.2f;
                break;
        }

        Debug.Log("Modifier: " + modifier);
        return modifier;
    }

    public void DeductPizzaFromOrder(IPizza pizzaReceived)
    {
        for (int i = 0; i < Orders.Length; i++)
        {
            if (Orders[i].ContainsKey(pizzaReceived.Type))
            {
                if (Orders[i][pizzaReceived.Type] > 0)
                {
                    Orders[i][pizzaReceived.Type]--;
                    Debug.Log($"Hit by {pizzaReceived.Type}. Deducting 1. Remaining: {Orders[i][pizzaReceived.Type]}");

                    // If the count reaches zero, remove the pizza type from the dictionary
                    if (Orders[i][pizzaReceived.Type] == 0)
                    {
                        Orders[i].Remove(pizzaReceived.Type);
                        Debug.Log($"{pizzaReceived.Type} removed from the order.");
                    }
                }
                return;
            }
            else
            {
                // Wrong pizza received
                Debug.Log($"Customer received wrong pizza. Wanted: {PasteOrderContents()}, Got: {pizzaReceived.Type}");
            }
        }
        Debug.Log($"No {pizzaReceived.Type} left in the order.");
    }

    public int IsOrderFulfilled()
    {
        for (int i = 0; i < Orders.Length; i++)
        {
            if (Orders[i].Count > 0)  // If any pizza type still has remaining pizzas, the order is not fulfilled
            {
                return -1;
            }
        }

        return (int)(GetOriginalOrderAmount() * GetOrderModifier());  // If all pizzas are fulfilled, return true
    }

    public string PasteOrderContents() //Debugging method to paste Order array
    {
        List<string> orderContents = new List<string>();

        foreach (var order in Orders)
        {
            foreach (var item in order)
            {
                // Format: PizzaType: count
                orderContents.Add($"{item.Key}: {item.Value}");
            }
        }
        // Join the contents with a comma and space
        return string.Join(", ", orderContents);
    }
}
