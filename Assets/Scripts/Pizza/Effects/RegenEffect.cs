public class RegenEffect : IEffect
{
    public EffectType Type => EffectType.Regen;
    public float Value => 2;
    public int Duration { get; set; } = 5;
}