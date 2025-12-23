using Game.Battle.Modifiers;
using System;
using UnityEngine;
using Game.Traits.Conditionals;

namespace Game.Traits
{
    [Serializable]
    public class ConditionalCombatModifier
    {
        [SerializeField] private string effectDescription;
        [Space]
        [SerializeField] private CombatModifierType combatModifierType;
        [SerializeField] private int value;
        [Space]
        [SerializeReference] private TraitConditional conditional;

        public string EffectDescription => effectDescription;

        public bool TryGetModifier(Creature creature, BattleContext context, string sourceName, object sourceObject, out CombatModifier modifier)
        {
            if (conditional == null || conditional.CheckConditional(new BattleEventData(creature, context)))
            {
                modifier = new CombatModifier(combatModifierType, value, sourceName, sourceObject);
                return true;
            }
            modifier = null;
            return false;
        }
    }
}