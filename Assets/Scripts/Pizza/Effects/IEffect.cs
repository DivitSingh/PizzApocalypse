public interface IEffect
{
    public EffectType Type { get; }
    public float Value { get; } // Strength of effect
    public int Duration { get; set; } // Duration in seconds
}

public enum EffectType
{
    Poison,
    Stun,
    Regen
}
