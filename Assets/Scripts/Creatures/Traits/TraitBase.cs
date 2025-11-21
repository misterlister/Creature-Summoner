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
    public List<PassiveTraitEffect> PassiveEffects => passiveEffects;
    public List<TriggeredTraitEffect> TriggeredEffects => triggeredEffects;

}


[System.Serializable]
public class LearnableTrait
{
    [SerializeField] TraitBase trait;
    [SerializeField] int level;

    public TraitBase Trait => trait;
    public int Level => level;
}
