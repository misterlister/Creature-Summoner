using UnityEngine;
using System;

[Serializable]
public class AdjacentAllyTypeCondition : TraitConditional
{
    [SerializeField] private CreatureType requiredType;

    public override bool CheckConditional(BattleEventData eventData)
    {
        if (eventData.SourceCreature == null || eventData.SourceCreature.BattleSlot == null)
        {
            Debug.LogWarning("AdjacentTypeAllyCondition: SourceCreature or BattleSlot is null in event data.");
            return false;
        }
        var adjacentSlots = eventData.SourceCreature.BattleSlot.GetAdjacentSlots();
        foreach (var slot in adjacentSlots)
        {
            var creature = slot.Creature;
            if (creature != null && creature != eventData.SourceCreature && creature.IsType(requiredType) && creature.IsAlly(eventData.SourceCreature))
            {
                return true;
            }
        }
        return false;
    }

    public override string GetDescription()
    {
        return $"when adjacent to {requiredType}-type ally";
    }
}
