using System;
using UnityEngine;

[Serializable]
public class OnConditionRemovedTrigger : TraitTrigger
{
    public override BattleEventType GetEventType()
    {
        return  BattleEventType.ConditionRemovedFromMe;
    }

    public override bool CheckTrigger(BattleEventData eventData)
    {
        return eventData is ConditionRemovedEventData;
    }

    public override string GetDescription()
    {
        return $"When a conditionType condition is removed from this creature";
    }
}
