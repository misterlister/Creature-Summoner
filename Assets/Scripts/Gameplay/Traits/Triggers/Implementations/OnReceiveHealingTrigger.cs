using System;
using UnityEngine;

namespace Game.Traits.Triggers
{
    [Serializable]
    public class OnReceiveHealingTrigger : TraitTrigger
    {
        [SerializeField] ActionTiming timing;
        [SerializeField] Perspective patient;
        public override BattleEventType GetEventType()
        {
            return (patient, timing) switch
            {
                (Perspective.Self, ActionTiming.Before) => BattleEventType.BeforeIAmHealed,
                (Perspective.Self, ActionTiming.After) => BattleEventType.AfterIAmHealed,
                (Perspective.Ally, ActionTiming.Before) => BattleEventType.BeforeAllyIsHealed,
                (Perspective.Ally, ActionTiming.After) => BattleEventType.AfterAllyIsHealed,
                (Perspective.Opponent, ActionTiming.Before) => BattleEventType.BeforeOpponentIsHealed,
                (Perspective.Opponent, ActionTiming.After) => BattleEventType.AfterOpponentIsHealed,
                (Perspective.Team, ActionTiming.Before) => BattleEventType.BeforeTeamIsHealed,
                (Perspective.Team, ActionTiming.After) => BattleEventType.AfterTeamIsHealed,
                _ => BattleEventType.Invalid,
            };
        }

        public override bool CheckTrigger(BattleEventData eventData)
        {
            return eventData is HealEventData;
        }

        public override string GetDescription()
        {
            string timingString = StringUtils.GetTimingString(timing);
            string patientString = StringUtils.GetPerspectiveString(patient);

            return $"{timingString} {patientString} receives healing";
        }
    }
}