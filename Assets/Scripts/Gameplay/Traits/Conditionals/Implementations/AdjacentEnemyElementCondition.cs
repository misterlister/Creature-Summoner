using UnityEngine;
using System;

namespace Game.Traits.Conditionals
{
    [Serializable]
    public class AdjacentEnemyElementCondition : TraitConditional
    {
        [SerializeField] private CreatureElement requiredElement;

        public override bool CheckConditional(BattleEventData eventData)
        {
            if (eventData.SourceCreature == null || eventData.SourceCreature.CurrentTile == null)
            {
                Debug.LogWarning("AdjacentElementEnemyCondition: SourceCreature or BattleSlot is null in event data.");
                return false;
            }
            var adjacentEnemies = eventData.SourceCreature.CurrentTile.GetAdjacentEnemies();
            foreach (var enemy in adjacentEnemies)
            {
                if (enemy != null && enemy.IsElement(requiredElement))
                {
                    return true;
                }
            }
            return false;
        }

        public override string GetDescription()
        {
            return $"when adjacent to {requiredElement}-element enemy";
        }
    }
}