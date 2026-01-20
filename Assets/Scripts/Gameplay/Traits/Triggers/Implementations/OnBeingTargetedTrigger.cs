using UnityEngine;
using System;

namespace Game.Traits.Triggers
{
    [Serializable]
    public class OnBeingTargetedTrigger : TraitTrigger
    {
        [SerializeField] private ActionTiming timing;
        [SerializeField] private AttackTriggerCategory attackCategory;

        public override BattleEventType GetEventType()
        {
            return timing switch
            {
                ActionTiming.Before => BattleEventType.BeforeIAmTargetedByAction,
                ActionTiming.After => BattleEventType.AfterIAmTargetedByAction,
                _ => BattleEventType.AfterIAmTargetedByAction
            };
        }
        public override bool CheckTrigger(BattleEventData eventData)
        {
            if (eventData is not ActionEventData actionData)
            {
                return false;
            }

            if (actionData.Action == null)
            {
                return false;
            }

            return attackCategory switch
            {
                AttackTriggerCategory.Any => true,
                AttackTriggerCategory.Melee => actionData.Action.IsMelee(),
                AttackTriggerCategory.Ranged => actionData.Action.IsRanged(),
                AttackTriggerCategory.Magic => actionData.Action.IsMagic(),
                AttackTriggerCategory.Physical => actionData.Action.IsPhysical(),
                AttackTriggerCategory.Fire => actionData.Action.IsElement(CreatureElement.Fire),
                AttackTriggerCategory.Earth => actionData.Action.IsElement(CreatureElement.Earth),
                AttackTriggerCategory.Water => actionData.Action.IsElement(CreatureElement.Water),
                AttackTriggerCategory.Air => actionData.Action.IsElement(CreatureElement.Air),
                AttackTriggerCategory.Radiant => actionData.Action.IsElement(CreatureElement.Radiant),
                AttackTriggerCategory.Metal => actionData.Action.IsElement(CreatureElement.Metal),
                AttackTriggerCategory.Cold => actionData.Action.IsElement(CreatureElement.Cold),
                AttackTriggerCategory.Electric => actionData.Action.IsElement(CreatureElement.Electric),
                AttackTriggerCategory.Arcane => actionData.Action.IsElement(CreatureElement.Arcane),
                AttackTriggerCategory.Beast => actionData.Action.IsElement(CreatureElement.Beast),
                AttackTriggerCategory.Necrotic => actionData.Action.IsElement(CreatureElement.Necrotic),
                AttackTriggerCategory.Plant => actionData.Action.IsElement(CreatureElement.Plant),
                _ => false,
            };
        }

        public override string GetDescription()
        {
            string timingString = StringUtils.GetTimingString(timing);
            string categoryString = attackCategory.ToString();
            string article = StringUtils.GetIndefiniteArticle(categoryString);

            return attackCategory == AttackTriggerCategory.Any ?
                $"{timingString} this creature is targeted with any action" :
                $"{timingString} this creature is targeted with {article} {categoryString} action";
        }
    }
}