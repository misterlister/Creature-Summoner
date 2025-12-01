
public class ConditionAppliedEventData : BattleEventData
{
    public Creature ApplyingCreature { get; set; }
    public Creature TargetCreature => SourceCreature;
    //public Condition condition { get; set; }
    public ConditionAppliedEventData(Creature sourceCreature, BattleContext battleContext)
       : base(sourceCreature, battleContext) { }
}
