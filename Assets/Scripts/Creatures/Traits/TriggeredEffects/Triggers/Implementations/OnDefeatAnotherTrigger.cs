using System;
using UnityEngine;

[Serializable]
public class OnDefeatAnotherTrigger : TraitTrigger
{
    public override BattleEventType GetEventType()
    {
        return BattleEventType.IDefeatAnother;
    }

    public override bool CheckTrigger(BattleEventData eventData)
    {
        return eventData is CreatureDefeatEventData;
    }

    public override string GetDescription()
    {
        return $"When this creature defeats another";
    }
}
