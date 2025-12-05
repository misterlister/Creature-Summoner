using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrait", menuName = "Traits/Trait")]
public class TraitBase : ScriptableObject
{
    [SerializeField] string traitName;
    [TextArea]
    [SerializeField] string description;
    //[SerializeField] Sprite icon;

    [Header("Passive Effects (Always Active)")]
    [SerializeReference]
    [SerializeField] List<PassiveTraitEffect> passiveEffects = new List<PassiveTraitEffect>();

    [Header("Triggered Effects (Event-Driven)")]
    [SerializeReference]
    [SerializeField] List<TriggeredTraitEffect> triggeredEffects = new List<TriggeredTraitEffect>();

    public string TraitName => traitName;
    public string Description => description;
    //public Sprite Icon => icon;

    // For passive effects - called during stat calculations
    public void CollectModifiers(
        Creature creature,
        BattleContext context,
        List<FlatStatModifier> flatMods,
        List<PercentStatModifier> percentMods,
        List<CombatModifier> combatMods)
    {
        foreach (var effect in passiveEffects)
        {
            effect.CollectModifier(creature, context, flatMods, percentMods, combatMods);
        }
    }

    // For triggered effects - accessed by RuntimeTrait during setup
    public IReadOnlyList<TriggeredTraitEffect> TriggeredEffects => triggeredEffects;
}


[System.Serializable]
public class LearnableTrait
{
    [SerializeField] TraitBase trait;
    [SerializeField] int level;

    public TraitBase Trait => trait;
    public int Level => level;
}
