using UnityEngine;
using System;

namespace Game.Traits.Conditionals
{
    [Serializable]
    public abstract class TraitConditional
    {
        [SerializeField] string conditionalName;

        public abstract bool CheckConditional(BattleEventData eventData);
        public abstract string GetDescription();
    }
}