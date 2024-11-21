using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

/// <summary>
/// Factory used to generate orders with given constraints.
/// </summary>
public class OrderFactory
{
    private static Random random = new Random();
    
    /// <summary>
    /// Creates an order with a maximum of maxSize pizzas.
    /// </summary>
    /// <param name="maxSize">Maximum number of pizzas available in an order.</param>
    /// <returns></returns>
    public static Order CreateOrder(int maxSize)
    {
        // TODO: Can limit amount of a specific pizza as well, left this out for now
        
        // Randomly generate amounts such that sum if <= maxSize
        var items = new Dictionary<PizzaType, int>();
        var remainingSize = maxSize;
        var pizzaTypes = Enum.GetValues(typeof(PizzaType)).Cast<PizzaType>();
        pizzaTypes = pizzaTypes.OrderBy(_ => random.Next()); // Shuffle pizza types to ensure even distribution
        foreach (var type in pizzaTypes)
        {
            var amount = random.Next(1, remainingSize + 1);
            remainingSize -= amount;
            
            if (amount > 0) items.Add(type, amount);
            if (remainingSize == 0) break;
        }
        
        // TODO: Fix issue where order is empty
        
        return new Order(items);
    }
}