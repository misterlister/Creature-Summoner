using UnityEngine;

public class MoveEventData : BattleEventData
{
    public Creature ForcibleMover { get; set; } = null;
    public Creature MovingCreature => SourceCreature;
    public MoveEventData(Creature sourceCreature, BattleContext battleContext)
       : base(sourceCreature, battleContext) { }
}
