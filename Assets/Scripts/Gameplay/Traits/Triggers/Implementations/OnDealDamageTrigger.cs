using UnityEngine;
using System;

namespace Game.Traits.Triggers
{
    [Serializable]
    public class OnDamagedTrigger : TraitTrigger
    {
        [SerializeField] ActionTiming timing;
        [SerializeField] Perspective damagedCreature;
        public override BattleEventType GetEventType()
        {
            return (damagedCreature, timing) switch
            {
                (Perspective.Self, ActionTiming.Before) => BattleEventType.BeforeIReceiveDamage,
                (Perspective.Self, ActionTiming.After) => BattleEventType.AfterIReceiveDamage,
                (Perspective.Ally, ActionTiming.Before) => BattleEventType.BeforeAllyReceivesDamage,
                (Perspective.Ally, ActionTiming.After) => BattleEventType.AfterAllyReceivesDamage,
                (Perspective.Opponent, ActionTiming.Before) => BattleEventType.BeforeOpponentReceivesDamage,
                (Perspective.Opponent, ActionTiming.After) => BattleEventType.AfterOpponentReceivesDamage,
                (Perspective.Team, ActionTiming.Before) => BattleEventType.BeforeTeamReceivesDamage,
                (Perspective.Team, ActionTiming.After) => BattleEventType.AfterTeamReceivesDamage,
                _ => BattleEventType.Invalid,
            };
        }

        public override bool CheckTrigger(BattleEventData eventData)
        {
            return eventData is DamageEventData;
        }

        public override string GetDescription()
        {
            string timingString = StringUtils.GetTimingString(timing);
            string damagedString = StringUtils.GetPerspectiveString(damagedCreature);

            return $"{timingString} {damagedString} is damaged";
        }
    }
}