
public class TurnStartEventData : BattleEventData
{
    public int TurnNumber { get; set; }
    public Creature CurrentCreature => SourceCreature;
    public TurnStartEventData(Creature sourceCreature, BattleContext battleContext)
       : base(sourceCreature, battleContext) { }
}
