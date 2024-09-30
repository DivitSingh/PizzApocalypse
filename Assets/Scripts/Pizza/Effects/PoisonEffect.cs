public class PoisonEffect: IEffect
{
    public Stat AffectedStat => Stat.Health;
    public EffectType Type => EffectType.ConstantDecrease;
    public float Value { get; }
    public int Duration { get; set; }

    public PoisonEffect(float damage = 5, int duration = 3)
    {
        Value = damage;
        Duration = duration;
    }
}