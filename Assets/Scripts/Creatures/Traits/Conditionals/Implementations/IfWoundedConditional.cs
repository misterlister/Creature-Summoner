using System;
using UnityEngine;
[Serializable]
public class IfWoundedConditional : TraitConditional
{
    public override bool CheckConditional(BattleEventData eventData)
    {
        if (eventData.SourceCreature == null)
        {
            Debug.LogWarning("IfWoundedConditional: SourceCreature is null in event data.");
            return false;
        }

        return eventData.SourceCreature.IsWounded();
    }

    public override string GetDescription()
    {
        return "if wounded";
    }
}
