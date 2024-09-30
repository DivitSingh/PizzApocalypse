public class RegenEffect : IEffect
{
    public Stat AffectedStat => Stat.Health;
    public EffectType Type => EffectType.ConstantIncrease;
    public float Value => 2;
    public int Duration { get; set; } = 5;
}