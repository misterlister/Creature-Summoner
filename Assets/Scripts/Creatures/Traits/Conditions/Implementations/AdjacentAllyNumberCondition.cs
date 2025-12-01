using UnityEngine;
using System;

[Serializable]
public class AdjacentAllyNumberCondition : TraitCondition
{
    [SerializeField] [Range(0, 9)] private int requiredNum;
    [SerializeField] private ComparisonType comparisonType;


    public override bool CheckCondition(BattleEventData eventData)
    {
        Creature thisCreature = eventData.SourceCreature;
        if (thisCreature == null || thisCreature.BattleSlot == null)
        {
            Debug.LogWarning("AdjacentAllyNumberCondition: SourceCreature or BattleSlot is null in event data.");
            return false;
        }
        var adjacentSlots = thisCreature.BattleSlot.GetAdjacentSlots();
        int allyCount = 0;
        foreach (var slot in adjacentSlots)
        {
            var creature = slot.Creature;
            if (creature != null && creature != thisCreature && thisCreature.IsAlly(creature))
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
        return $"when {comparisonText}{numText} allies are adjacent";
    }
}
