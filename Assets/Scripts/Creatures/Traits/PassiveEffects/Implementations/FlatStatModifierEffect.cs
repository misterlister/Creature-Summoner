using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FlatStatModifierEffect : PassiveTraitEffect
{
    [SerializeField] private StatType statType;
    [SerializeField] private int value;

    protected override void AddModifier(
        List<FlatStatModifier> flatMods,
        List<PercentStatModifier> percentMods,
        List<CombatModifier> combatMods)
    {
        flatMods.Add(new FlatStatModifier(statType, value, this));
    }

    public override string GetDescription()
    {
        string sign = value > 0 ? "+" : "";
        return $"{sign}{value} {statType}";
    }
}