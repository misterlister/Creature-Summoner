using System;
using UnityEngine;

namespace Game.Traits
{
    [Serializable]
    public class PassiveCombatModifier
    {
        [TextArea]
        public string description;
        public CombatModifierType combatModifierType;
        public int value;
    }
}
