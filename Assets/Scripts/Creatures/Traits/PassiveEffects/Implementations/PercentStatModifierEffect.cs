using UnityEngine;
using System.Collections.Generic;
using System;

public class PercentStatModifierEffect : PassiveTraitEffect
{
    [SerializeField] private StatType statType;
    [SerializeField] private int percentValue; // Stored as int for inspector (10 = 10%)

    protected override void AddModifier(
        List<FlatStatModifier> flatMods,
        List<PercentStatModifier> percentMods,
        List<CombatModifier> combatMods)
    {
        percentMods.Add(new PercentStatModifier(statType, percentValue / 100f, this));
    }

    public override string GetDescription()
    {
        string sign = percentValue > 0 ? "+" : "";
        return $"{sign}{percentValue}% {statType}";
    }
}