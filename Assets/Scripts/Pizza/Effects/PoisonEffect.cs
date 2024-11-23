public class PoisonEffect: IEffect
{
    public EffectType Type => EffectType.Poison;
    public float Value { get; }
    public int Duration { get; set; }

    public PoisonEffect(float damage = 5, int duration = 3)
    {
        Value = damage;
        Duration = duration;
    }
}