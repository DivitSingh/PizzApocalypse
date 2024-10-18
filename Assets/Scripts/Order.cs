using System;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    private Dictionary<PizzaType, int>[] Orders;
    private static System.Random random = new System.Random();
    private static int maxPizzaAmountOrder = 3;
    private static int maxPizzaTypePerOrder = 3;

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
        }
    }

    public Dictionary<PizzaType, int>[] GetOrders()
    {
        return Orders;
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

    public bool IsOrderFulfilled()
    {
        for (int i = 0; i < Orders.Length; i++)
        {
            if (Orders[i].Count > 0)  // If any pizza type still has remaining pizzas, the order is not fulfilled
            {
                return false;
            }
        }
        return true;  // If all pizzas are fulfilled, return true
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
