using UnityEngine;
using System;

namespace Game.Traits.Conditionals
{
    [Serializable]
    public class AdjacentAllyElementCondition : TraitConditional
    {
        [SerializeField] private CreatureElement requiredElement;

        public override bool CheckConditional(BattleEventData eventData)
        {
            if (eventData.SourceCreature == null || eventData.SourceCreature.BattleSlot == null)
            {
                Debug.LogWarning("AdjacentElementAllyCondition: SourceCreature or BattleSlot is null in event data.");
                return false;
            }
            var adjacentSlots = eventData.SourceCreature.BattleSlot.GetAdjacentSlots();
            foreach (var slot in adjacentSlots)
            {
                var creature = slot.Creature;
                if (creature != null && creature != eventData.SourceCreature && creature.IsElement(requiredElement) && creature.IsAlly(eventData.SourceCreature))
                {
                    return true;
                }
            }
            return false;
        }

        public override string GetDescription()
        {
            return $"when adjacent to {requiredElement}-element ally";
        }
    }
}