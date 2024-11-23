public class StunEffect : IEffect
{
    public EffectType Type => EffectType.Stun;
    public float Value => 0;
    public int Duration { get; set; } = 2;
}