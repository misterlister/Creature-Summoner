using UnityEngine;


public class TraitEventData
{
    public Creature SourceCreature { get; set; }
    public BattleContext BattleContext { get; set; }
    public Creature Attacker { get; set; }
    public Creature Defender { get; set; }
    public ActionBase Action { get; set; }
    public int DamageAmount { get; set; }
    public int EnergyAmount { get; set; }
    public int TurnNumber { get; set; }

    public Creature TurnCreature { get; set; }

    public bool IsCriticalHit { get; set; } = false;

    public bool IsGlancingHit { get; set; } = false;

    public bool IsAOETarget { get; set; } = false;

    public float DamageMultiplier { get; set; } = 1.0f;
    public float EnergyCostMultiplier { get; set; } = 1.0f;
    public bool PreventAction { get; set; } = false;
    public TraitEventData(Creature sourceCreature, BattleContext battleContext)
    {
        SourceCreature = sourceCreature;
        BattleContext = battleContext;
    }
}
