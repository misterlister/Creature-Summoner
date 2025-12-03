using System;
using UnityEngine;

[Serializable]
public class OnDefeatedTrigger : TraitTrigger
{
    [SerializeField] ActionTiming timing;
    [SerializeField] Perspective healer;
    public override BattleEventType GetEventType()
    {
        return (healer, timing) switch
        {
            (Perspective.Self, ActionTiming.Before) => BattleEventType.BeforeIHeal,
            (Perspective.Self, ActionTiming.After) => BattleEventType.AfterIHeal,
            (Perspective.Ally, ActionTiming.Before) => BattleEventType.BeforeAllyHeals,
            (Perspective.Ally, ActionTiming.After) => BattleEventType.AfterAllyHeals,
            (Perspective.Opponent, ActionTiming.Before) => BattleEventType.BeforeOpponentHeals,
            (Perspective.Opponent, ActionTiming.After) => BattleEventType.AfterOpponentHeals,
            (Perspective.Team, ActionTiming.Before) => BattleEventType.BeforeTeamHeals,
            (Perspective.Team, ActionTiming.After) => BattleEventType.AfterTeamHeals,
            _ => BattleEventType.AfterIHeal
        };
    }

    public override bool CheckTrigger(BattleEventData eventData)
    {
        return eventData is HealEventData;
    }

    public override string GetDescription()
    {
        string timingString = StringUtils.GetTimingString(timing);
        string healerString = StringUtils.GetPerspectiveString(healer);

        return $"{timingString} {healerString} is defeated";
    }
}
