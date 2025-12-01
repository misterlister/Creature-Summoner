using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class PassiveTraitEffect
{
    [SerializeField] protected string effectName;
    [SerializeReference] public TraitConditional conditional;

    private static readonly Dictionary<StatType, StatModification> EmptyModifications = new();

    public Dictionary<StatType, StatModification> GetStatModifications(Creature creature, BattleContext context)
    {
        BattleEventData eventData = new BattleEventData(creature, context);

        if (conditional == null || conditional.CheckConditional(eventData))
        {
            return GetStatModificationsInternal(creature, context);
        }
        else
        {
            return EmptyModifications;
        }
    }

    public abstract Dictionary<StatType, StatModification> GetStatModificationsInternal(Creature creature, BattleContext context);
    public abstract string GetDescription();
}

