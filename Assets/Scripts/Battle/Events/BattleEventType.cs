using UnityEngine;

public enum BattleEventType
{
    // Action Events
    BeforeIAct,
    AfterIAct,
    BeforeOpponentActs,
    AfterOpponentActs,
    BeforeAllyActs,
    AfterAllyActs,
    BeforeIAmTargetedByAction,
    AfterIAmTargetedByAction,
    BeforeIAmTargetedByAOE,
    AfterIAmTargetedByAOE,
    BeforeTeamActs,
    AfterTeamActs,

    // Damage Events
    BeforeIReceiveDamage,
    AfterIReceiveDamage,
    BeforeOpponentReceivesDamage,
    AfterOpponentReceivesDamage,
    BeforeAllyReceivesDamage,
    AfterAllyReceivesDamage,
    BeforeTeamReceivesDamage,
    AfterTeamReceivesDamage,
    BeforeIDealDamage,
    AfterIDealDamage,

    // Healing Events
    BeforeIHeal,
    AfterIHeal,
    BeforeIAmHealed,
    AfterIAmHealed,
    BeforeAllyHeals,
    AfterAllyHeals,
    BeforeAllyIsHealed,
    AfterAllyIsHealed,
    BeforeOpponentHeals,
    AfterOpponentHeals,
    BeforeOpponentIsHealed,
    AfterOpponentIsHealed,
    BeforeTeamHeals,
    AfterTeamHeals,
    BeforeTeamIsHealed,
    AfterTeamIsHealed,

    // Condition Events
    BeforeIApplyCondition,
    AfterIApplyCondition,
    BeforeIReceiveCondition,
    AfterIReceiveCondition,
    BeforeAllyAppliesCondition,
    AfterAllyAppliesCondition,
    BeforeOpponentAppliesCondition,
    AfterOpponentAppliesCondition,
    BeforeAllyReceivesCondition,
    AfterAllyReceivesCondition,
    BeforeTeamReceivesCondition,
    AfterTeamReceivesCondition,
    BeforeTeamAppliesCondition,
    AfterTeamAppliesCondition,
    BeforeOpponentReceivesCondition,
    AfterOpponentReceivesCondition,
    ConditionRemovedFromMe,

    // Movement Events
    BeforeIMove,
    AfterIMove,
    BeforeIAmForciblyMoved,
    AfterIAmForciblyMoved,
    BeforeIForciblyMoveAnother,
    AfterIForciblyMoveAnother,
    BeforeAllyMoves,
    AfterAllyMoves,
    BeforeTeamMoves,
    AfterTeamMoves,
    BeforeOpponentMoves,
    AfterOpponentMoves,

    // Turn Events
    MyTurnStart,
    MyTurnEnd,
    AllyTurnStart,
    AllyTurnEnd,
    OpponentTurnStart,
    OpponentTurnEnd,
    TeamTurnStart,
    TeamTurnEnd,

    // Defeat Events
    IAmDefeated,
    IDefeatAnother,
    AllyIsDefeated,
    OpponentIsDefeated,
    TeamIsDefeated,

    // Used to indicate errors
    Invalid
}
