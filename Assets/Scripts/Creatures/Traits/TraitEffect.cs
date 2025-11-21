using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class TraitEffect
{
    // Is this effect currently active for the given creature?
    public abstract bool IsActive(Creature creature, BattleContext context);
    public abstract Dictionary<StatType, StatModification> GetModifications(Creature creature, BattleContext context);

    public abstract string GetDescription();
}