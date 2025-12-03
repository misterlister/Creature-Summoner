using System;
using UnityEngine;

[Serializable]
public class OnMovingTrigger : TraitTrigger
{
    [SerializeField] private ActionTiming timing;
    [SerializeField] private Perspective creatureWhoMoves;
    [SerializeField] private Perspective? forcedMovementSource = null;

    public override BattleEventType GetEventType()
    {
        bool isForced = forcedMovementSource != null;

        if (isForced)
        {
            if (forcedMovementSource == Perspective.Self)
            {
                return (timing == ActionTiming.Before)
                    ? BattleEventType.BeforeIForciblyMoveAnother
                    : BattleEventType.AfterIForciblyMoveAnother;
            }
            else if (creatureWhoMoves == Perspective.Self)
            {
                return (timing == ActionTiming.Before)
                    ? BattleEventType.BeforeIAmForciblyMoved
                    : BattleEventType.AfterIAmForciblyMoved;

            }
            else
            {
                Debug.LogError(
                    $"[OnMovingTrigger] Forced movement state reached with " +
                    $"creatureWhoMoves={creatureWhoMoves}, forcedMovementSource={forcedMovementSource}.");
                return BattleEventType.Invalid;
            }
        } 
        else
        {
            var eventType = (creatureWhoMoves, timing) switch
            {
                (Perspective.Self, ActionTiming.Before) => BattleEventType.BeforeIMove,
                (Perspective.Self, ActionTiming.After) => BattleEventType.AfterIMove,
                (Perspective.Ally, ActionTiming.Before) => BattleEventType.BeforeAllyMoves,
                (Perspective.Ally, ActionTiming.After) => BattleEventType.AfterAllyMoves,
                (Perspective.Opponent, ActionTiming.Before) => BattleEventType.BeforeOpponentMoves,
                (Perspective.Opponent, ActionTiming.After) => BattleEventType.AfterOpponentMoves,
                (Perspective.Team, ActionTiming.Before) => BattleEventType.BeforeTeamMoves,
                (Perspective.Team, ActionTiming.After) => BattleEventType.AfterTeamMoves,
                _ => BattleEventType.Invalid
            };

            if (eventType == BattleEventType.Invalid )
            {
                Debug.LogError(
                    $"[OnMovingTrigger] Invalid trigger for movement state reached with " +
                    $"creatureWhoMoves={creatureWhoMoves}, timing={timing}.");
            }

            return eventType;
        }
    }

    public override bool CheckTrigger(BattleEventData eventData)
    {
        return eventData is MoveEventData;
    }

    public override string GetDescription()
    {
        string timingString = StringUtils.GetTimingString(timing);
        string actor = StringUtils.GetPerspectiveString(creatureWhoMoves);
        string mover = StringUtils.GetPerspectiveString(forcedMovementSource);


        return forcedMovementSource == null ?
            $"{timingString} {actor} moves" :
            $"{timingString} {actor} is moved by {mover}";
    }
}
