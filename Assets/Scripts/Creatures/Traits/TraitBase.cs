using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrait", menuName = "Traits/Trait")]
public class TraitBase : ScriptableObject
{
    [SerializeField] string traitName;
    [TextArea]
    [SerializeField] string description;

    [Header("Trait Effects")]
    [SerializeField] List<TraitEffect> effects;
    public string TraitName => traitName;
    public string Description => description;
    public List<TraitEffect> Effects => effects;

    public Dictionary<StatType, StatModification> GetStatModifications(Creature creature, BattleContext context = null)
    {
        var modifications = new Dictionary<StatType, StatModification>();
        foreach (var effect in effects)
        {
            if (effect.IsActive(creature, context))
            {
                var effectMods = effect.GetModifications(creature, context);
                foreach (var keyValPair in effectMods)
                {
                    if (!modifications.ContainsKey(keyValPair.Key))
                    {
                        modifications[keyValPair.Key] = new StatModification();
                    }
                    modifications[keyValPair.Key].Combine(keyValPair.Value);
                }
            }
        }
        return modifications;
    }

}


[System.Serializable]
public class LearnableTrait
{
    [SerializeField] TraitBase trait;
    [SerializeField] int level;

    public TraitBase Trait => trait;
    public int Level => level;
}
