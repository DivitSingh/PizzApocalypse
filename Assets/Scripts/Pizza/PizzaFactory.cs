public class PizzaFactory
{
    public static IPizza CreatePizza(PizzaType pizzaType, int quantity = 1)
    {
        return pizzaType switch
        {
            PizzaType.Cheese => new CheesePizza(quantity),
            PizzaType.Pineapple => new PineapplePizza(quantity),
            PizzaType.Mushroom => new MushroomPizza(quantity),
            _ => new CheesePizza()
        };
    }
}