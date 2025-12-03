using System;
using UnityEngine;

[Serializable]
public class OnReceiveConditionTrigger : TraitTrigger
{
    [SerializeField] ActionTiming timing;
    [SerializeField] Perspective receiver;
    public override BattleEventType GetEventType()
    {
        return (receiver, timing) switch
        {
            (Perspective.Self, ActionTiming.Before) => BattleEventType.BeforeIReceiveCondition,
            (Perspective.Self, ActionTiming.After) => BattleEventType.AfterIReceiveCondition,
            (Perspective.Ally, ActionTiming.Before) => BattleEventType.BeforeAllyReceivesCondition,
            (Perspective.Ally, ActionTiming.After) => BattleEventType.AfterAllyReceivesCondition,
            (Perspective.Opponent, ActionTiming.Before) => BattleEventType.BeforeOpponentReceivesCondition,
            (Perspective.Opponent, ActionTiming.After) => BattleEventType.AfterOpponentReceivesCondition,
            (Perspective.Team, ActionTiming.Before) => BattleEventType.BeforeTeamReceivesCondition,
            (Perspective.Team, ActionTiming.After) => BattleEventType.AfterTeamReceivesCondition,
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
        string receiverString = StringUtils.GetPerspectiveString(receiver);

        return $"{timingString} {receiverString} receives a conditionType condition";
    }
}
