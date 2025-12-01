using System.Collections.Generic;

public class ActionEventData : BattleEventData
{
    public ActionBase Action { get; set; }
    public Creature TargetCreature { get; set; }
    public Creature ActingCreature => SourceCreature;
    public List<Creature> AOETargetCreatures { get; set; }

    public ActionEventData(Creature sourceCreature, BattleContext battleContext)
       : base(sourceCreature, battleContext) { }

}
