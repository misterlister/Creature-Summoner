using System;
using UnityEngine;

[Serializable]
public class IfTargetIsIsolatedConditional : TraitConditional
{
    public override bool CheckConditional(BattleEventData eventData)
    {
        if (eventData is not ActionEventData actionData)
        {
            return false;
        }

        Creature target = actionData.TargetCreature;

        if (target == null)
        {
            return false;
        }


        var adjacentSlots = target.BattleSlot.GetAdjacentSlots();
        int allyCount = 0;
        foreach (var slot in adjacentSlots)
        {
            var creature = slot.Creature;
            if (creature != null && creature != target && target.IsEnemy(creature))
            {
                allyCount++;
            }
        }
        return allyCount == 0;
    }

    public override string GetDescription()
    {
        return $"when the target enemy has no adjacent allies";
    }
}
