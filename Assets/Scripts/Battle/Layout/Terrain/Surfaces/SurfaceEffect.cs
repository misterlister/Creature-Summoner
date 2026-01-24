using UnityEngine;

/// <summary>
/// Base class for surface effects that appear on battle tiles.
/// Each specific surface type is a concrete implementation.
/// </summary>
public abstract class SurfaceEffect
{
    public abstract string SurfaceName { get; }
    public abstract string Description { get; }

    // Duration tracking
    public int RemainingDuration { get; protected set; }
    public bool IsExpired => RemainingDuration <= 0;

    // When does this trigger?
    public abstract SurfaceTriggerTiming TriggerTiming { get; }

    // Visual data
    public abstract Sprite SurfaceSprite { get; }
    public abstract Color TintColor { get; }
    public virtual GameObject ParticleEffectPrefab => null;

    // Constructor requires a duration
    protected SurfaceEffect(int duration)
    {
        RemainingDuration = Mathf.Max(duration, 1); // ensure at least 1 turn
    }

    // Core functionality
    public void TickDuration()
    {
        if (RemainingDuration > 0)
            RemainingDuration--;
    }

    public void ResetDuration(int duration)
    {
        RemainingDuration = Mathf.Max(duration, 1);
    }

    public bool ShouldTrigger(SurfaceTriggerTiming timing)
    {
        return TriggerTiming == timing;
    }

    // Apply the surface effect to a creature
    public abstract void Apply(Creature creature, BattleContext context);
}


/// <summary>
/// Fire surface - deals damage each turn
/// </summary>
public class FireSurface : SurfaceEffect
{
    public override string SurfaceName => "Fire";
    public override string Description => "Flames that burn creatures standing in them.";
    public override SurfaceTriggerTiming TriggerTiming => SurfaceTriggerTiming.OnTurnStart;

    public override Sprite SurfaceSprite => null;
    public override Color TintColor => new Color(1f, 0.4f, 0f, 0.7f);

    private float damagePercent;

    // duration is now required
    public FireSurface(int duration, float damagePercent = 0.15f) : base(duration)
    {
        this.damagePercent = damagePercent;
    }

    public override void Apply(Creature creature, BattleContext context)
    {
        if (creature == null || creature.IsDefeated) return;
        if (creature.IsElement(CreatureElement.Fire)) return;

        int damage = Mathf.CeilToInt(creature.MaxHP * damagePercent);
        // Apply damage here
    }
}

/// <summary>
/// Smoke surface - reduces accuracy
/// </summary>
public class SmokeSurface : SurfaceEffect
{
    public override string SurfaceName => "Smoke";
    public override string Description => "Thick smoke that obscures vision.";
    public override SurfaceTriggerTiming TriggerTiming => SurfaceTriggerTiming.OnEnter;

    public SmokeSurface(int duration = 2) : base(duration) { }

    public override Sprite SurfaceSprite => null;
    public override Color TintColor => new Color(0.3f, 0.3f, 0.3f, 0.6f);

    private float accuracyReduction = 0.3f; // 30% accuracy penalty

    public override void Apply(Creature creature, BattleContext context)
    {
        if (creature == null || creature.IsDefeated) return;

        /*
        // Apply Obscured status
        var obscuredStatus = new ObscuredStatus(accuracyReduction, 2);
        creature.Statuses.AddStatus(obscuredStatus);
        */
    }
}

/// <summary>
/// Healing surface - restores HP each turn
/// </summary>
public class HealingSurface : SurfaceEffect
{
    public override string SurfaceName => "Healing Pool";
    public override string Description => "Restorative energy that heals creatures.";
    public override SurfaceTriggerTiming TriggerTiming => SurfaceTriggerTiming.OnTurnStart;

    public override Sprite SurfaceSprite => null;
    public override Color TintColor => new Color(0.3f, 1f, 0.5f, 0.7f);

    private float healingPercent = 0.1f; // 10% Max HP

    public HealingSurface(int duration, float healingPercent = 0.1f) : base(duration)
    {
        this.healingPercent = healingPercent;
    }

    public override void Apply(Creature creature, BattleContext context)
    {
        if (creature == null || creature.IsDefeated) return;

        int healing = Mathf.CeilToInt(creature.MaxHP * healingPercent);
        creature.Heal(healing);
    }
}
