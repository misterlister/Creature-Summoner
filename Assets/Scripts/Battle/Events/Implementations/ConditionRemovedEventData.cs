
public class ConditionRemovedEventData : BattleEventData
{
    public Creature AffectedCreature => SourceCreature;
    //public Condition condition { get; set; }
    public ConditionRemovedEventData(Creature sourceCreature, BattleContext battleContext)
       : base(sourceCreature, battleContext) { }
}
