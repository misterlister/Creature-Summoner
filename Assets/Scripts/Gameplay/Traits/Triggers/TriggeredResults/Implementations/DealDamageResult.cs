using UnityEngine;
using System;

namespace Game.Traits.Triggers.Results
{
    [Serializable]
    public class RetaliateDamageResult : TraitResult
    {
        [SerializeField] private DamageCalcType calculationType;
        [SerializeField] private int flatAmount = 0;
        [SerializeField] private StatType statToScale;
        [SerializeField] private int statScalePercent = 0;
        [SerializeField] private int retaliationPercent = 0;

        public override void Execute(BattleEventData eventData)
        {
            if (eventData is not DamageEventData damageEvent)
            {
                Debug.LogError($"Error: RetaliateDamageResult called from non-damaging event: {eventData}");
                return;
            }
            Creature target = damageEvent.Attacker;

            if (target == null)
            {
                Debug.LogError($"Error: RetaliateDamageResult tried to trigger, but no attacker in DamageEventData: {eventData}");
                return;
            }

            if (damageEvent.Defender == null)
            {
                Debug.LogError($"Error: RetaliateDamageResult tried to trigger, but no defender in DamageEventData: {eventData}");
                return;
            }

            int damage = CalculateDamage(damageEvent);

            target.BattleSlot.HitByAttack(damage);
        }

        private int CalculateDamage(DamageEventData damageEventData)
        {
            return calculationType switch
            {
                DamageCalcType.FlatValue => flatAmount,
                //DamageCalcType.StatScaling =>,
                DamageCalcType.Retaliation => (int)((retaliationPercent / 100f) * damageEventData.DamageAmount),
                _ => 0,
            };
        }

        public override string GetDescription()
        {
            return $"deal damage to the attacker";
        }
    }
}