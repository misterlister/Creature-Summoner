using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class PassiveTraitEffect
{
    [SerializeField] protected string effectName;
    [SerializeReference] public TraitConditional conditional;
    public void CollectModifier(
        Creature creature,
        BattleContext context,
        List<FlatStatModifier> flatMods,
        List<PercentStatModifier> percentMods,
        List<CombatModifier> combatMods)
    {
        if (conditional == null || conditional.CheckConditional(new BattleEventData(creature, context)))
        {
            AddModifier(flatMods, percentMods, combatMods);
        }
    }

    protected abstract void AddModifier(
        List<FlatStatModifier> flatMods,
        List<PercentStatModifier> percentMods,
        List<CombatModifier> combatMods);

    public abstract string GetDescription();
}

