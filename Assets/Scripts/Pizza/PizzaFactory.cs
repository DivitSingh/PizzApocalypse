public class PizzaFactory
{
    public static IPizza CreatePizza(PizzaType pizzaType, int baseDamage, int quantity = 1)
    {
        return pizzaType switch
        {
            PizzaType.Cheese => new CheesePizza(baseDamage, quantity),
            PizzaType.Pineapple => new PineapplePizza(baseDamage + 5, quantity),
            PizzaType.Mushroom => new MushroomPizza(baseDamage + 5, quantity),
            _ => new CheesePizza()
        };
    }
}