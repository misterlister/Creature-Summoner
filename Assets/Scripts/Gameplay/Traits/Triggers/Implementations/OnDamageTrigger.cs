using UnityEngine;
using System;

namespace Game.Traits.Triggers
{
    [Serializable]
    public class OnDealDamageTrigger : TraitTrigger
    {
        [SerializeField] ActionTiming timing;
        public override BattleEventType GetEventType()
        {
            return (timing) switch
            {
                ActionTiming.Before => BattleEventType.BeforeIDealDamage,
                ActionTiming.After => BattleEventType.AfterIDealDamage,
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

            return $"{timingString} this creature deals damage";
        }
    }
}