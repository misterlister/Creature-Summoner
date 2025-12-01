
public class DamageEventData : BattleEventData
{
    public Creature Attacker { get; set; }
    public Creature Defender => SourceCreature;
    public ActionBase ActionUsed { get; set; }
    public int DamageAmount { get; set; }
    public bool IsCriticalHit { get; set; }
    public bool IsGlancingHit { get; set; }
    public bool IsAOETarget { get; set; }
    public float DamageMultiplier { get; set; }
    public bool PreventDamage { get; set; }
    public DamageEventData(Creature sourceCreature, BattleContext battleContext)        
        : base(sourceCreature, battleContext) {}
}
