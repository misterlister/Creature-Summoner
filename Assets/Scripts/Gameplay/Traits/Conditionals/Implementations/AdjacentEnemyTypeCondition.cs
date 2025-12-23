using UnityEngine;
using System;

namespace Game.Traits.Conditionals
{
    [Serializable]
    public class AdjacentEnemyTypeCondition : TraitConditional
    {
        [SerializeField] private CreatureType requiredType;

        public override bool CheckConditional(BattleEventData eventData)
        {
            if (eventData.SourceCreature == null || eventData.SourceCreature.BattleSlot == null)
            {
                Debug.LogWarning("AdjacentTypeEnemyCondition: SourceCreature or BattleSlot is null in event data.");
                return false;
            }
            var adjacentSlots = eventData.SourceCreature.BattleSlot.GetAdjacentSlots();
            foreach (var slot in adjacentSlots)
            {
                var creature = slot.Creature;
                if (creature != null && creature != eventData.SourceCreature && creature.IsType(requiredType) && creature.IsEnemy(eventData.SourceCreature))
                {
                    return true;
                }
            }
            return false;
        }

        public override string GetDescription()
        {
            return $"when adjacent to {requiredType}-type enemy";
        }
    }
}