using UnityEngine;
using Game.Traits.Triggers;
using Game.Traits.Conditionals;
using Game.Traits.Triggers.Results;

namespace Game.Traits
{
    [System.Serializable]
    public class TriggeredTraitEffect
    {
        [SerializeField] string effectName;
        [SerializeReference] public TraitTrigger trigger;
        [SerializeReference] public TraitConditional conditional;
        [SerializeReference] public TraitResult result;

        // Checks if the effect should execute for a given event
        public bool ShouldExecute(BattleEventData eventData)
        {
            if (trigger == null || result == null)
            {
                Debug.LogWarning($"TriggeredTraitEffect '{effectName}' is missing a trigger or effect.");
                return false;
            }

            if (!trigger.CheckTrigger(eventData))
            {
                return false;
            }

            if (conditional == null)
            {
                return true;
            }

            return conditional.CheckConditional(eventData);
        }

        public void Execute(BattleEventData eventData)
        {
            result?.Execute(eventData);
        }

        public string GetDescription()
        {
            string triggerDesc = trigger != null ? $"{trigger.GetDescription()} " : "";
            string conditionalDesc = conditional != null ? $"{conditional.GetDescription()}, then " : "";
            string resultDesc = result != null ? result.GetDescription() : "";

            if (triggerDesc == "" && conditionalDesc != "")
            {
                conditionalDesc = conditionalDesc.CapitalizeFirst();
            }
            else if (triggerDesc != "" && conditionalDesc != "")
            {
                triggerDesc = ", " + conditionalDesc;
            }
            return $"{triggerDesc}{conditional}{resultDesc}.";
        }

    }
}