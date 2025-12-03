using System;
using UnityEngine;

[Serializable]
public class IfTiredConditional : TraitConditional
{
    public override bool CheckConditional(BattleEventData eventData)
    {
        if (eventData.SourceCreature == null)
        {
            Debug.LogWarning("IfTiredConditional: SourceCreature is null in event data.");
            return false;
        }

        return eventData.SourceCreature.IsTired();
    }

    public override string GetDescription()
    {
        return "if tired";
    }
}
