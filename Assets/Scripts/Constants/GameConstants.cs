using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
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

    public const int CORE_SLOTS = 3;
    public const int EMPOWERED_SLOTS = 3;
    public const int MASTERY_SLOTS = 1;

    // BATTLEFIELD CONSTANTS

    public const int BATTLE_ROWS = 3; // Number of Rows on the battlefield
    public const int BATTLE_COLS = 6; // Number of Columns on the battlefield

    public const int ENEMY_COL = BATTLE_COLS / 2;

    public const int BATTLE_MENU_COLS = 2; // Number of Columns in the battle selection menu

    // STAT CONSTANTS

    public const int AVERAGE_STAT = 40;

    public const float GLANCE_REDUCTION = 0.25f;
    public const float CRIT_BONUS = 0.4f;
    public const float CRIT_RESISTANCE = 0.0f;

    public const float DEFAULT_STARTING_ENERGY = 0.25f;

    // TYPE CONSTANTS

    public const float V_EFF_SINGLE = 4f;
    public const float V_EFF_DUAL = 2.25f;
    public const float EFF_SINGLE = 2f;
    public const float EFF_DUAL = 1.5f;
    public const float NEUTRAL = 1f;
    public const float INEFF_DUAL = 0.66f;
    public const float INEFF_SINGLE = 0.5f;
    public const float V_INEFF_DUAL = 0.44f;
    public const float V_INEFF_SINGLE = 0.25f;


    public static readonly Dictionary<CreatureType, Color> TypeColours = new Dictionary<CreatureType, Color>()
    {
        { CreatureType.Fire, new Color(1f, 0.3f, 0.3f) },           // Red
        { CreatureType.Water, new Color(0.3f, 0.5f, 1f) },          // Blue
        { CreatureType.Earth, new Color(0.6f, 0.4f, 0.2f) },        // Brown
        { CreatureType.Air, new Color(0.7f, 0.9f, 1f) },            // Light Blue
        { CreatureType.Beast, new Color(0.8f, 0.6f, 0.4f) },        // Tan
        { CreatureType.Plant, new Color(0.3f, 0.8f, 0.3f) },        // Green
        { CreatureType.Electric, new Color(1f, 1f, 0.3f) },         // Yellow
        { CreatureType.Radiant, new Color(1f, 0.9f, 0.6f) },        // Light Yellow
        { CreatureType.Necrotic, new Color(0.25f, 0.05f, 0.25f) },  // Dark Purple
        { CreatureType.Arcane, new Color(0.6f, 0.4f, 0.8f) },       // Violet
        { CreatureType.Metal, new Color(0.7f, 0.7f, 0.7f) },        // Grey
        { CreatureType.Cold, new Color(0.4f, 1f, 0.8f) },           // Cyan
        { CreatureType.None, new Color(0f, 0f, 0f) }                // Black
    };

    public static readonly Color HP_COLOUR = new Color(0.5f, 1f, 0.5f);
    public static readonly Color ENERGY_COLOUR = new Color(1f, 0f, 1f);
    public static readonly Color XP_COLOUR = new Color(1f, 0.85f, 0f);

    public static readonly Dictionary<AOE, (int y, int x)> AOEOptions = new Dictionary<AOE, (int y, int x)>
    {
        { AOE.Single, (1,1)},
        { AOE.SmallArc, (2,1)},
        { AOE.WideArc, (1,1)},
        { AOE.Line, (1,1)},
        { AOE.SmallCone, (2,1)},
        { AOE.LargeCone, (1,1)},
        { AOE.Square, (2,1)},
        { AOE.Field, (1,1)},
        { AOE.Burst, (1,1)},
    };
}

public enum ActionRange
{
    Melee,
    ShortRanged,
    LongRanged,
    Self
}

public enum Stat
{
    HP,
    Energy,
    Strength,
    Magic,
    Skill,
    Speed,
    Defense,
    Resistance
}

public enum ClassStatBuffLevel
{
    None,
    Low,
    MediumLow,
    Moderate,
    MediumHigh,
    High
}