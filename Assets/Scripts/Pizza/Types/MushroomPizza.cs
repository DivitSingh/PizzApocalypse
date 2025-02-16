// TODO: Should we have "tiers" of pizza, where Cheese is T1, Mushroom and Pineapple are T2?
public class MushroomPizza : IPizza
{
    public PizzaType Type => PizzaType.Mushroom;
    public int Quantity { get; }
    public float Damage { get; }
    public float Healing => 10;
    public IEffect PlayerEffect => new RegenEffect();
    public IEffect CustomerEffect => new StunEffect();

    public MushroomPizza(float baseDamage, int quantity)
    {
        Quantity = quantity;
        Damage = baseDamage * quantity;
    }
}