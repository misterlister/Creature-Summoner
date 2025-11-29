
public class HealEventData : BattleEventData
{
    public Creature HealingCreature { get; set; }
    public Creature HealedCreature => SourceCreature;
    public int HealAmount { get; set; }
    public float HealMultiplier { get; set; }

    public HealEventData(Creature sourceCreature, BattleContext battleContext)
       : base(sourceCreature, battleContext) { }
}
