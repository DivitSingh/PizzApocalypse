public class StunEffect : IEffect
{
    public Stat AffectedStat => Stat.Speed;
    public EffectType Type => EffectType.Multiplier;
    public float Value => 0;
    public int Duration { get; set; } = 2;
}