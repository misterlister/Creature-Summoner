using System;
using UnityEngine;
using Game.Battle.Modifiers;

namespace Game.Traits
{
    [Serializable]
    public class PassiveCombatModifier
    {
        [TextArea]
        public string description;
        public CombatModifierType combatModifierType;
        public CombatModifierMode mode;
        public int value;
    }
}
