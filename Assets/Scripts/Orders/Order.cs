using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Order
{
    public Dictionary<PizzaType, int> Items { get; private set; }

    public readonly int Value;

    public Order(Dictionary<PizzaType, int> items)
    {
        Items = items;
        Value = CalculateValue();
    }

    private int CalculateValue()
    {
        // Calculate value based on number of pizza types and number of total pizzas
        var baseMoneyRate = 2;
        var quantity = Items.Values.Sum();
        var modifier = CalculateModifier(Items.Count);
        return (int)(quantity * modifier + baseMoneyRate);
    }

    private static float CalculateModifier(int pizzaTypes)
    {
        return 1 + (pizzaTypes switch
        {
            1 => 0,
            2 => 0.2f,
            3 => 0.5f,
            _ => 0
        });
    }

    public bool IsFulfilled()
    {
        return Items.Values.Sum() == 0;
    }

    public void Receive(IPizza pizza)
    {
        if (Items.ContainsKey(pizza.Type) && Items[pizza.Type] > 0)
        {
            Items[pizza.Type] = Mathf.Max(0, Items[pizza.Type] - pizza.Quantity);
        }
    }
}
