
public class CreatureDefeatEventData : BattleEventData
{
    public Creature DefeatedCreature => SourceCreature;
    public Creature VictoriousCreature { get; set; }

    public CreatureDefeatEventData(Creature sourceCreature, BattleContext battleContext)
       : base(sourceCreature, battleContext) { }
}
