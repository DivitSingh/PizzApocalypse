public class Order
{
    public PizzaType PizzaType { get; private set; }

    public Order(PizzaType pizzaType)
    {
        PizzaType = pizzaType;
    }

    public bool IsSatisfied(IPizza deliveredPizza)
    {
        return deliveredPizza.Type == PizzaType;
    }
}
