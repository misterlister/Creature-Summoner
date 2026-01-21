using Game.Battle.Modifiers;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Traits
{

    [CreateAssetMenu(fileName = "NewTrait", menuName = "Traits/Create new Trait")]
    public class TraitBase : ScriptableObject
    {
        [SerializeField] string traitName;
        [TextArea]
        [SerializeField] string description;
        //[SerializeField] Sprite icon;

        [Header("Passive Combat Modifiers (Always Active)")]
        [SerializeField] private List<PassiveCombatModifier> passiveCombatModifiers = new();

        [Header("Conditional Stat Modifiers")]
        [SerializeField] List<ConditionalStatModifier> statModifiers = new();

        [Header("Conditional Combat Modifiers")]
        [SerializeField] List<ConditionalCombatModifier> combatModifiers = new();

        [Header("Triggered Effects (Event-Driven)")]
        [SerializeReference]
        [SerializeField] List<TriggeredTraitEffect> triggeredEffects = new();

        public string TraitName => traitName;
        public string Description => description;
        //public Sprite Icon => icon;

        public void CollectPassiveCombatModifiers(List<CombatModifier> mods)
        {
            foreach (var passiveMod in passiveCombatModifiers)
            {
                mods.Add(new CombatModifier(
                    passiveMod.combatModifierType,
                    passiveMod.value,
                    passiveMod.mode,
                    traitName,
                    this
                ));
            }
        }

        public void CollectStatModifiers(
            Creature creature,
            BattleContext context,
            List<StatModifier> mods)
        {
            foreach (var conditionalMod in statModifiers)
            {
                if (conditionalMod.TryGetModifier(creature, context, traitName, this, out var modifier))
                {
                    mods.Add(modifier);
                }
            }
        }

        public void CollectCombatModifiers(
            Creature creature,
            BattleContext context,
            List<CombatModifier> mods)
        {
            foreach (var conditionalMod in combatModifiers)
            {
                if (conditionalMod.TryGetModifier(creature, context, traitName, this, out var modifier))
                {
                    mods.Add(modifier);
                }
            }
        }

        public IReadOnlyList<TriggeredTraitEffect> TriggeredEffects => triggeredEffects;
    }
}