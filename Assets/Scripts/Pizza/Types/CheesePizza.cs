public struct CheesePizza : IPizza
{
    public PizzaType Type => PizzaType.Cheese;
    public int Quantity { get; }

    public float Damage { get; }
    public float Healing => 10;

    public IEffect PlayerEffect => null;
    public IEffect CustomerEffect => null;

    public CheesePizza(int baseDamage, int quantity)
    {
        Quantity = quantity;
        Damage = baseDamage * quantity;
    }
}