using System;
using UnityEngine;

namespace Game.Traits.Triggers
{
    [Serializable]
    public class OnApplyConditionTrigger : TraitTrigger
    {
        [SerializeField] ActionTiming timing;
        [SerializeField] Perspective applier;
        public override BattleEventType GetEventType()
        {
            return (applier, timing) switch
            {
                (Perspective.Self, ActionTiming.Before) => BattleEventType.BeforeIApplyCondition,
                (Perspective.Self, ActionTiming.After) => BattleEventType.AfterIApplyCondition,
                (Perspective.Ally, ActionTiming.Before) => BattleEventType.BeforeAllyAppliesCondition,
                (Perspective.Ally, ActionTiming.After) => BattleEventType.AfterAllyAppliesCondition,
                (Perspective.Opponent, ActionTiming.Before) => BattleEventType.BeforeOpponentAppliesCondition,
                (Perspective.Opponent, ActionTiming.After) => BattleEventType.AfterOpponentAppliesCondition,
                (Perspective.Team, ActionTiming.Before) => BattleEventType.BeforeTeamAppliesCondition,
                (Perspective.Team, ActionTiming.After) => BattleEventType.AfterTeamAppliesCondition,
                _ => BattleEventType.Invalid,
            };
        }

        public override bool CheckTrigger(BattleEventData eventData)
        {
            return eventData is ConditionAppliedEventData;
        }

        public override string GetDescription()
        {
            string timingString = StringUtils.GetTimingString(timing);
            string applierString = StringUtils.GetPerspectiveString(applier);

            return $"{timingString} {applierString} applies a conditionType condition";
        }
    }
}