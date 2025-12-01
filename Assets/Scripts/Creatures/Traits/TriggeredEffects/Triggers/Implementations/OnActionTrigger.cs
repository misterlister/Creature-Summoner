using UnityEngine;
using System;

[Serializable]
public class OnActionTrigger : TraitTrigger
{
    [SerializeField] private Perspective actionOwner = Perspective.Self;
    [SerializeField] private AttackTriggerCategory attackCategory = AttackTriggerCategory.Any;
    [SerializeField] private ActionTiming timing = ActionTiming.After;




    public override BattleEventType GetEventType()
    {
        return (actionOwner, timing) switch
        {
            (Perspective.Self, ActionTiming.Before) => BattleEventType.BeforeIAct,
            (Perspective.Self, ActionTiming.After) => BattleEventType.AfterIAct,
            (Perspective.Ally, ActionTiming.Before) => BattleEventType.BeforeAllyActs,
            (Perspective.Ally, ActionTiming.After) => BattleEventType.AfterAllyActs,
            (Perspective.Enemy, ActionTiming.Before) => BattleEventType.BeforeOpponentActs,
            (Perspective.Enemy, ActionTiming.After) => BattleEventType.AfterOpponentActs,
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
        string actor = (actionOwner) switch
        {
            Perspective.Self => "this creature",
            Perspective.Ally => "an allied creature",
            Perspective.Enemy => "an enemy",
            Perspective.Team => "a member of this team",
            _ => "this creature",
        };

        string timingString = timing.ToString();
        string categoryString = attackCategory.ToString();
        string article = StringUtils.GetIndefiniteArticle(categoryString);

        return attackCategory == AttackTriggerCategory.Any ? 
            $"{timingString} {actor} uses an action" : 
            $"{timingString} {actor} uses {article} {categoryString} action";
    }
}
