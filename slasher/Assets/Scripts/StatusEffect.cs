public enum StatusEffectType
{
    None,
    Burn,
    Drown,
    Root,
    Bleed,
    Shock,
    Freeze,
    Illusion,
    Blind,
    Poison
}

public class StatusEffect
{
    public StatusEffectType type;
    public float duration;
    public float remainingDuration;
    public float damage;
    public float tickTimer;
    public float tickInterval;

    public StatusEffect(StatusEffectType type, float duration, float damage)
    {
        this.type = type;
        this.duration = duration;
        remainingDuration = duration;
        this.damage = damage;
        tickTimer = 1f;
        tickInterval = 1f;
    }
}