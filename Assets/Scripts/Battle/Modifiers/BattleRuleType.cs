namespace Game.Battle.Modifiers
{
    public enum BattleRuleType
    {
        // Movement
        ExtendedMovementRange,
        CanMoveAfterAttack,
        CanMoveDiagonally,
        CanSwapWithAlly,
        CanPassOverAlly,

        // Targeting
        IgnoreRowRestriction,
        IgnoresInterception,
        TouchActionsTargetDiagonally,
        ExtendReachTargetPassover,

        // Defensive rules
        PreventsPassOver,
        PreventsDisplacement,
        IgnoresDefensiveTerrain,
        IgnoresCoverBonus,
    }
}