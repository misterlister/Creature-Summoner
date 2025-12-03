using UnityEngine;
using System;

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
            AttackTriggerCategory.Fire => actionData.Action.IsType(CreatureType.Fire),
            AttackTriggerCategory.Earth => actionData.Action.IsType(CreatureType.Earth),
            AttackTriggerCategory.Water => actionData.Action.IsType(CreatureType.Water),
            AttackTriggerCategory.Air => actionData.Action.IsType(CreatureType.Air),
            AttackTriggerCategory.Radiant => actionData.Action.IsType(CreatureType.Radiant),
            AttackTriggerCategory.Metal => actionData.Action.IsType(CreatureType.Metal),
            AttackTriggerCategory.Cold => actionData.Action.IsType(CreatureType.Cold),
            AttackTriggerCategory.Electric => actionData.Action.IsType(CreatureType.Electric),
            AttackTriggerCategory.Arcane => actionData.Action.IsType(CreatureType.Arcane),
            AttackTriggerCategory.Beast => actionData.Action.IsType(CreatureType.Beast),
            AttackTriggerCategory.Necrotic => actionData.Action.IsType(CreatureType.Necrotic),
            AttackTriggerCategory.Plant => actionData.Action.IsType(CreatureType.Plant),
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
