using UnityEngine;
using System;

namespace Game.Traits.Conditionals
{
    [Serializable]
    public class AdjacentEnemyNumberCondition : TraitConditional
    {
        [SerializeField][Range(0, 3)] private int requiredNum;
        [SerializeField] private ComparisonType comparisonType;


        public override bool CheckConditional(BattleEventData eventData)
        {
            Creature thisCreature = eventData.SourceCreature;
            if (thisCreature == null || thisCreature.CurrentTile == null)
            {
                Debug.LogWarning("AdjacentEnemyNumberCondition: SourceCreature or BattleSlot is null in event data.");
                return false;
            }
            var adjacentEnemies = thisCreature.CurrentTile.GetAdjacentEnemies();
            return MathUtils.Compare(adjacentEnemies.Count, requiredNum, comparisonType);
        }

        public override string GetDescription()
        {
            string numText = (requiredNum == 0) ? "no" : requiredNum.ToString();
            string comparisonText = comparisonType switch
            {
                ComparisonType.Equal => "",
                ComparisonType.GreaterThan => "more than ",
                ComparisonType.LessThan => "less than ",
                ComparisonType.GreaterThanOrEqual => "at least ",
                ComparisonType.LessThanOrEqual => "at most ",
                _ => ""
            };
            return $"when {comparisonText}{numText} enemies are adjacent";
        }
    }
}