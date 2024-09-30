public struct PineapplePizza : IPizza
{
    public PizzaType Type => PizzaType.Pineapple;
    public int Quantity { get; }
    public float Damage => 15;
    public float Healing => 15;

    public IEffect PlayerEffect => null;
    public IEffect CustomerEffect => new PoisonEffect(5, 3);

    // TODO: Eventually we may need initialization with different damage/healing values based on difficulty or perks?
    public PineapplePizza(int quantity)
    {
        Quantity = quantity;
    }
}