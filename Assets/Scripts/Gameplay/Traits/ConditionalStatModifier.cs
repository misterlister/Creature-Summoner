using Game.Battle.Modifiers;
using System;
using UnityEngine;
using Game.Traits.Conditionals;

namespace Game.Traits
{
    [Serializable]
    public class ConditionalStatModifier
    {
        [SerializeField] private string effectDescription;
        [Space]
        [SerializeField] private StatType statType;
        [SerializeField] private int value;
        [SerializeField] private ModifierMode mode;
        [Space]
        [SerializeReference] private TraitConditional conditional;

        public string EffectDescription => effectDescription;

        public bool TryGetModifier(Creature creature, BattleContext context, string sourceName, object sourceObject, out StatModifier modifier)
        {
            if (conditional == null || conditional.CheckConditional(new BattleEventData(creature, context)))
            {
                modifier = new StatModifier(statType, value, mode, sourceName, sourceObject);
                return true;
            }
            modifier = null;
            return false;
        }
    }
}