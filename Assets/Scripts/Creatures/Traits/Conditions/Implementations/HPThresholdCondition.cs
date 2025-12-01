using UnityEngine;
using System;

[Serializable]
public class HPThresholdCondition : TraitCondition
{
    [SerializeField] [Range(0, 100)] private int thresholdPercent = 50;
    [SerializeField] private bool belowThreshold = true;

    public override bool CheckCondition(BattleEventData eventData)
    {
        if (eventData.SourceCreature == null)
        {
            Debug.LogWarning("HPThresholdCondition: SourceCreature is null in event data.");
            return false;
        }

        int hpPercent = eventData.SourceCreature.GetHPAsPercentage();
        return belowThreshold ? hpPercent <= thresholdPercent : hpPercent >= thresholdPercent;
    }

    public override string GetDescription()
    {
        string condition = belowThreshold ? "below" : "above";
        return $"if HP is {condition} {thresholdPercent}%";
    }
}
