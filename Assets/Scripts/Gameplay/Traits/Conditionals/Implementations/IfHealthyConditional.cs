using UnityEngine;
using System;

namespace Game.Traits.Conditionals
{
    [Serializable]
    public class IfHealthyConditional : TraitConditional
    {
        public override bool CheckConditional(BattleEventData eventData)
        {
            if (eventData.SourceCreature == null)
            {
                Debug.LogWarning("IfHealthyConditional: SourceCreature is null in event data.");
                return false;
            }

            return eventData.SourceCreature.IsHealthy();
        }

        public override string GetDescription()
        {
            return "if healthy";
        }
    }
}