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
            if (eventData.SourceCreature == null || eventData.SourceCreature.CurrentTile == null)
            {
                Debug.LogWarning("AdjacentElementAllyCondition: SourceCreature or BattleSlot is null in event data.");
                return false;
            }
            var adjacentAllies = eventData.SourceCreature.CurrentTile.GetAdjacentAllies();
            foreach (var ally in adjacentAllies)
            {
                if (ally != null && ally != eventData.SourceCreature && ally.IsElement(requiredElement))
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