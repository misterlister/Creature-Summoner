using UnityEngine;
using System;

[Serializable]
public class OnTurnTrigger : TraitTrigger
{
    [SerializeField] private Perspective turnOwner = Perspective.Self;
    [SerializeField] private TurnTiming turnTiming = TurnTiming.Start;

    public override BattleEventType GetEventType()
    {
        return (turnOwner, turnTiming) switch
        {
            (Perspective.Self, TurnTiming.Start) => BattleEventType.MyTurnStart,
            (Perspective.Ally, TurnTiming.Start) => BattleEventType.AllyTurnStart,
            (Perspective.Opponent, TurnTiming.Start) => BattleEventType.OpponentTurnStart,
            (Perspective.Team, TurnTiming.Start) => BattleEventType.TeamTurnStart,
            (Perspective.Self, TurnTiming.End) => BattleEventType.MyTurnEnd,
            (Perspective.Ally, TurnTiming.End) => BattleEventType.AllyTurnEnd,
            (Perspective.Opponent, TurnTiming.End) => BattleEventType.OpponentTurnEnd,
            (Perspective.Team, TurnTiming.End) => BattleEventType.TeamTurnEnd,
            _ => BattleEventType.MyTurnStart,
        };
    }

    public override bool CheckTrigger(BattleEventData eventData)
    {
        return eventData is TurnStartEventData;
    }

    public override string GetDescription()
    {
        string timing = (turnTiming == TurnTiming.Start) ? "start" : "end";
        return turnOwner switch
        {
            Perspective.Self => $"at the {timing} of this creature's turn",
            Perspective.Ally => $"at the {timing} of an ally's turn",
            Perspective.Opponent => $"at the {timing} of an enemy's turn",
            Perspective.Team => $"at the {timing} of each team member's turn",
            _ => $"at the {timing} of any creature's turn",
        };
    }
}
