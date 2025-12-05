using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatModifierEffect : PassiveTraitEffect
{
    [SerializeField] private CombatModifierType modType;
    [SerializeField] private int percentValue;

    protected override void AddModifier(
        List<FlatStatModifier> flatMods,
        List<PercentStatModifier> percentMods,
        List<CombatModifier> combatMods)
    {
        combatMods.Add(new CombatModifier(modType, percentValue / 100f, this));
    }

    public override string GetDescription()
    {
        string sign = percentValue > 0 ? "+" : "";
        return $"{sign}{percentValue}% {modType}";
    }
}
