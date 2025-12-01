

public class TurnEndEventData : BattleEventData
{
    public int TurnNumber { get; set; }
    public Creature CurrentCreature => SourceCreature;
    public TurnEndEventData(Creature sourceCreature, BattleContext battleContext)
       : base(sourceCreature, battleContext) { }
}
