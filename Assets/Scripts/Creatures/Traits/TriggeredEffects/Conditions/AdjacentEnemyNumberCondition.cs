using UnityEngine;
using System;

[Serializable]
public class AdjacentEnemyNumberCondition : TraitCondition
{
    [SerializeField][Range(0, 3)] private int requiredNum;
    [SerializeField] private ComparisonType comparisonType;


    public override bool CheckCondition(TraitEventData eventData)
    {
        Creature thisCreature = eventData.SourceCreature;
        if (thisCreature == null || thisCreature.BattleSlot == null)
        {
            Debug.LogWarning("AdjacentEnemyNumberCondition: SourceCreature or BattleSlot is null in event data.");
            return false;
        }
        var adjacentSlots = thisCreature.BattleSlot.GetAdjacentSlots();
        int allyCount = 0;
        foreach (var slot in adjacentSlots)
        {
            var creature = slot.Creature;
            if (creature != null && creature != thisCreature && thisCreature.IsEnemy(creature))
            {
                allyCount++;
            }
        }
        return MathUtils.Compare(allyCount, requiredNum, comparisonType);
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
