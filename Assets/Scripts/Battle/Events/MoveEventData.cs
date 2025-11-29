using UnityEngine;

public class MoveEventData : BattleEventData
{
    public Creature MoveSource { get; set; } = null;
    public Creature MovingCreature => SourceCreature;
    public MoveEventData(Creature sourceCreature, BattleContext battleContext)
       : base(sourceCreature, battleContext) { }
}
