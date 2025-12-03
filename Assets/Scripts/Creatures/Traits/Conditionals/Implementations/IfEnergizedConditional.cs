using System;
using UnityEngine;

[Serializable]
public class IfEnergizedConditional : TraitConditional
{
    public override bool CheckConditional(BattleEventData eventData)
    {
        if (eventData.SourceCreature == null)
        {
            Debug.LogWarning("IfEnergizedConditional: SourceCreature is null in event data.");
            return false;
        }

        return eventData.SourceCreature.IsEnergized();
    }

    public override string GetDescription()
    {
        return "if energized";
    } 
}
