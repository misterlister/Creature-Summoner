using System;
using System.Collections.Generic;

public static class BattleSystemConstants
{
    // ACTION CONSTANTS
    public const float ACCURACY_ADJUSTMENT_FACTOR = 0.5f;    // Value that controls how much accuracy changes based on stat ratio
    public const float STATUS_RESIST_ADJUSTMENT_FACTOR = 1f;    // Value that controls how much accuracy changes based on stat ratio
    public const float MAX_HIT = 100f;         // Ceiling for accuracy of actions
    public const float MIN_HIT = 10f;          // Floor for accuracy of actions
    public const int MIN_DAMAGE = 1;           // Minimum damage that can be dealt by actions
    public const int BASE_VARIANCE = 85;       // Base floor for damage variance
    public const int VARIANCE_ADJUSTMENT = 10; // Max distance that damage variance ranges can have from base
    public const int MIN_VARIANCE = BASE_VARIANCE - VARIANCE_ADJUSTMENT;   // Minimum floor for damage variance
    public const int MAX_VARIANCE = BASE_VARIANCE + VARIANCE_ADJUSTMENT;   // Maximum ceiling for damage variance
    public const int ROLL_CEILING = 101;       // Maximum value that can be rolled for random die rolls
    public const float SECONDARY_DAMAGE_MULTIPLIER = 0.6f; // Damage multiplier for secondary AOE targets
    public const float TERTIARY_DAMAGE_MULTIPLIER = 0.3f; // Damage multiplier for tertiary AOE targets

    public enum BattleState
    {
        NotStarted,
        Start,
        NewRound,
        PlayerTurn,
        EnemyTurn,
        Ended,
        Busy
    }
    public enum PlayerTurnState
    {
        ActionCategorySelect,
        CoreActionSelect,
        EmpoweredActionSelect,
        MasteryActionSelect,
        MovementSelect,
        Examine,
        TargetSelect
    }

    // AOE Constants
    public enum AOE
    {
        Single,
        Arc,
        Line,
        SmallCone,
        LargeCone,
        SmallBurst,
        LargeBurst
    }

    public readonly struct AOEPattern
    {
        public readonly (int row, int col)[] SecondaryOffsets;
        public readonly (int row, int col)[] TertiaryOffsets;

        public AOEPattern(
            (int row, int col)[] secondary,
            (int row, int col)[] tertiary = null)
        {
            SecondaryOffsets = secondary;
            TertiaryOffsets = tertiary ?? Array.Empty<(int, int)>();
        }
    }

    public static readonly Dictionary<AOE, AOEPattern> AOEPatterns = new()
    {
        { AOE.Single, new AOEPattern(
            secondary: Array.Empty<(int, int)>())
        },
        { AOE.Arc, new AOEPattern(
            secondary: new[] { (-1, 0), (1, 0) })
        },
        { AOE.Line, new AOEPattern(
            secondary: new[] { (0, 1) },
            tertiary:  new[] { (0, 2) })
        },
        { AOE.SmallCone, new AOEPattern(
            secondary: new[] { (-1, 1), (0, 1), (1, 1) })
        },
        { AOE.LargeCone, new AOEPattern(
            secondary: new[] { (-1, 1), (0, 1), (1, 1) },
            tertiary:  new[] { (-2, 2), (-1, 2), (0, 2), (1, 2), (2, 2) })
        },
        { AOE.SmallBurst, new AOEPattern(
            secondary: new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
        },
        { AOE.LargeBurst, new AOEPattern(
            secondary: new[] { (-1, 0), (1, 0), (0, -1), (0, 1) },
            tertiary:  new[] { (-2, 0), (2, 0), (0, -2), (0, 2), (-1, -1), (-1, 1), (1, -1), (1, 1) })
        },
    };
}
