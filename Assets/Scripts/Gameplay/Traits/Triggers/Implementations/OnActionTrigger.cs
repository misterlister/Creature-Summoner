using UnityEngine;
using System;

namespace Game.Traits.Triggers
{
    [Serializable]
    public class OnActionTrigger : TraitTrigger
    {
        [SerializeField] private Perspective actionOwner;
        [SerializeField] private AttackTriggerCategory attackCategory;
        [SerializeField] private ActionTiming timing;

        public override BattleEventType GetEventType()
        {
            return (actionOwner, timing) switch
            {
                (Perspective.Self, ActionTiming.Before) => BattleEventType.BeforeIAct,
                (Perspective.Self, ActionTiming.After) => BattleEventType.AfterIAct,
                (Perspective.Ally, ActionTiming.Before) => BattleEventType.BeforeAllyActs,
                (Perspective.Ally, ActionTiming.After) => BattleEventType.AfterAllyActs,
                (Perspective.Opponent, ActionTiming.Before) => BattleEventType.BeforeOpponentActs,
                (Perspective.Opponent, ActionTiming.After) => BattleEventType.AfterOpponentActs,
                (Perspective.Team, ActionTiming.Before) => BattleEventType.BeforeTeamActs,
                (Perspective.Team, ActionTiming.After) => BattleEventType.AfterTeamActs,
                _ => BattleEventType.AfterIAct,

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
            string actor = StringUtils.GetPerspectiveString(actionOwner);
            string categoryString = attackCategory.ToString();
            string article = StringUtils.GetIndefiniteArticle(categoryString);

            return attackCategory == AttackTriggerCategory.Any ?
                $"{timingString} {actor} uses an action" :
                $"{timingString} {actor} uses {article} {categoryString} action";
        }
    }
}