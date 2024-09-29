using JetBrains.Annotations;

public interface IPizza
{
    public PizzaType Type { get; }
    public int Quantity { get; }
    
    public float Damage { get; }
    public float Healing { get; }
    
    [CanBeNull] public IEffect PlayerEffect { get; }
    [CanBeNull] public IEffect CustomerEffect { get; }
}