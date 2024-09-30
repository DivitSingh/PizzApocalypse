public interface IEffect
{
    public Stat AffectedStat { get; }
    public EffectType Type { get; }
    public float Value { get; } // Strength of effect
    public int Duration { get; set; } // Duration in seconds
}

