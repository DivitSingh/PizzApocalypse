public struct PineapplePizza : IPizza
{
    public PizzaType Type => PizzaType.Pineapple;
    public int Quantity { get; }
    public float Damage { get; }
    public float Healing => 15;

    public IEffect PlayerEffect => null;
    public IEffect CustomerEffect => new PoisonEffect(5, 3);

    public PineapplePizza(int baseDamage, int quantity)
    {
        Quantity = quantity;
        Damage = baseDamage * quantity;
    }
}