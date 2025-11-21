using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class PassiveTraitEffect
{
    [SerializeField] protected string effectName;
    public abstract Dictionary<StatType, StatModification> GetStatModifications(Creature creature, BattleContext context);
    public abstract string GetDescription();
}

