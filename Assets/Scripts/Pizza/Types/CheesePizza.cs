public struct CheesePizza : IPizza
{
    public PizzaType Type => PizzaType.Cheese;
    public int Quantity { get; }

    public float Damage { get; }
    public float Healing => 10;

    // TODO: Replace with actual effects
    public IEffect PlayerEffect => null;
    public IEffect CustomerEffect => null;

    public CheesePizza(int quantity)
    {
        Quantity = quantity;
        Damage = quantity * 10; // TODO: This should probably be tuned, should there be bonus damage for throwing entire box?
    }
}