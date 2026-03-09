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

    // ACTION SLOTS CONSTANTS
    public const int CORE_SLOTS = 3;
    public const int EMPOWERED_SLOTS = 3;
    public const int MASTERY_SLOTS = 1;

    // XP CONSTANTS
    public const int MAX_LEVEL = 100;
    public const int MAX_CLASS_LEVEL = 10;
    public const int MAX_XP = 1000000;

    // CREATURE STATUS CONSTANTS
    public const int HEALTHY_THRESHOLD = 50;
    public const int ENERGIZED_THRESHOLD = 75;
    public const int TIRED_THRESHOLD = 25;

    // BATTLEFIELD CONSTANTS

    public const int BATTLE_ROWS = 3; // Number of Rows on the battlefield
    public const int BATTLE_COLS = 6; // Number of Columns on the battlefield

    public const int ENEMY_COL = BATTLE_COLS / 2;

    public const int BATTLE_MENU_COLS = 2; // Number of Columns in the battle selection menu

    // STAT CONSTANTS

    public const int MIN_STAT_VALUE = 1;
    public const int AVERAGE_BASE_STAT = 40;

    public const float GLANCE_REDUCTION = 0.25f;
    public const float CRIT_DAMAGE_BONUS = 0.4f;
    public const float CRIT_DAMAGE_RESISTANCE = 0.0f;

    public const float DEFAULT_STARTING_ENERGY = 0.25f;

    // ELEMENTAL EFFECTIVENESS CONSTANTS

    public const float V_EFF_SINGLE = 4f;
    public const float V_EFF_DUAL = 2.25f;
    public const float EFF_SINGLE = 2f;
    public const float EFF_DUAL = 1.5f;
    public const float NEUTRAL = 1f;
    public const float INEFF_DUAL = 0.66f;
    public const float INEFF_SINGLE = 0.5f;
    public const float V_INEFF_DUAL = 0.44f;
    public const float V_INEFF_SINGLE = 0.25f;

    public static readonly Dictionary<CreatureElement, Color> ElementColours = new Dictionary<CreatureElement, Color>()
    {
        { CreatureElement.Fire,     new Color(1f,    0.278f, 0.200f) },     // Red-orange
        { CreatureElement.Water,    new Color(0.302f,0.549f, 1f) },         // Blue
        { CreatureElement.Earth,    new Color(0.435f, 0.298f, 0.169f) },    // Brown
        { CreatureElement.Air,      new Color(0.722f,0.902f, 1f) },         // Sky blue
        { CreatureElement.Beast,    new Color(0.878f, 0.541f, 0.278f) },    // Orange-tan
        { CreatureElement.Plant,    new Color(0.259f,0.780f, 0.341f) },     // Green
        { CreatureElement.Electric, new Color(1f,    0.878f, 0.200f) },     // Yellow
        { CreatureElement.Radiant,  new Color(1f,    0.886f, 0.478f) },     // Pale yellow
        { CreatureElement.Necrotic, new Color(0.227f,0.031f, 0.188f) },     // Dark purple
        { CreatureElement.Arcane,   new Color(0.522f,0.322f, 1f) },         // Arcane violet
        { CreatureElement.Metal,    new Color(0.478f,0.510f, 0.541f) },     // Steel grey
        { CreatureElement.Cold,     new Color(0.251f,0.922f, 0.878f) },     // Icy cyan
        { CreatureElement.None,     new Color(0.949f,0.949f, 0.949f) }      // Off-white
    };

    public static readonly Color HP_COLOUR = new Color(0.5f, 1f, 0.5f);
    public static readonly Color ENERGY_COLOUR = new Color(1f, 0f, 1f);
    public static readonly Color XP_COLOUR = new Color(1f, 0.85f, 0f);

    // Keeps track of how many options each AOE type has for targeting
    public static readonly Dictionary<AOE, (int y, int x)> AOEOptions = new Dictionary<AOE, (int y, int x)>
    {
        { AOE.Single, (1,1)},
        { AOE.SmallArc, (2,1)},
        { AOE.WideArc, (1,1)},
        { AOE.FullArc, (1,1)},
        { AOE.SmallLine, (1,1)},
        { AOE.LargeLine, (1,1)},
        { AOE.FullLine, (1,1)},
        { AOE.SmallCone, (2,1)},
        { AOE.MediumCone, (1,1)},
        { AOE.LargeCone, (1,1)},
        { AOE.SmallBurst, (1,1)},
        { AOE.LargeBurst, (1,1)}
    };
}

public enum TeamSide
{
    Player,
    Enemy
}

public enum PositionRole
{
    Frontline,
    Midline,
    Backline
}

public enum ActionRange
{
    // Offensive - always targets enemy grid
    Melee,          // First enemy in the same or adjacent row
    Reach,          // Can pass over 1-2 creatures in the same or adjacent row
    Distant,        // Any enemy in the same or adjacent row

    // Support - always targets own grid
    Self,           // Only the acting creature itself
    Touch,          // Any creature on the same tile or adjacent tiles
    Team            // All creatures on the same grid
}

public enum Perspective
{
    Self,
    Ally,
    Opponent,
    Team,
}

public enum TargetType
{
    Any,
    Self,
    AllyIncludingSelf,
    Enemy,
    Ally,
    EmptySpace,
    OccupiedSpace
}

public enum StatType
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

public enum HitType
{
    Hit,
    Glance,
    Critical
}

public enum CombatModifierType
{
    // Damage dealt modifiers
    PhysicalDamageMod,
    MagicalDamageMod,
    MeleeDamageMod,
    RangedDamageMod,
    AOEDamageMod,
    ShieldDamageMod,
    CriticalDamageMod,
    GlancingDamageMod,
    EffectiveDamageMod,
    IneffectiveDamageMod,
    ShieldBreakerDamageMod,

    // Damage taken modifiers
    PhysicalDamageResist,
    MagicalDamageResist,
    MeleeDamageResist,
    RangedDamageResist,
    AOEDamageResist,
    ShieldDamageResist,
    CriticalDamageResist,
    GlancingDamageResist,
    EffectiveDamageResist,
    IneffectiveDamageResist,

    // Special mechanics
    PhysicalVamp,
    MagicalVamp,
    CriticalChanceMod,
    EvasionMod,
    AccuracyMod,
    DamageVarianceMod,
    BaneResist,
    ShieldGivenMod,
    ShieldReceivedMod,
    HealingGivenMod,
    HealingReceivedMod,
    StartingEnergyMod,

    // Elemental Resistances
    FireResist,
    RadiantResist,
    WaterResist,
    ColdResist,
    EarthResist,
    MetalResist,
    AirResist,
    ElectricResist,
    BeastResist,
    PlantResist,
    NecroticResist,
    ArcaneResist,
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

public enum ActionSource
{
    Physical,
    Magical
}

public enum ActionRole
{
    Offensive,
    Support,
    Defensive
}

public enum ActionSlotType
{
    Core,
    Empowered,
    Mastery
}

public enum AOE
{
    Single,
    SmallArc,
    WideArc,
    FullArc,
    SmallLine,
    LargeLine,
    FullLine,
    SmallCone,
    MediumCone,
    LargeCone,
    SmallBurst,
    LargeBurst
}

public enum ActionTag
{
    Healing,
    NoContact,
}

public enum ComparisonType
{
    LessThan,
    LessThanOrEqual,
    Equal,
    GreaterThanOrEqual,
    GreaterThan
}

public static class XPSystem
{
    public static int GetXPForNextCreatureLevel(int level)
    {
        // Placeholder XP curve
        return (level + 1) * (level + 1) * 100;
    }

    public static int GetXPForNextClassLevel(int classLevel)
    {
        // Placeholder XP curve
        return (classLevel + 1) * (classLevel + 1) * 100;
    }

    public static int GetTotalXPForCreatureLevel(int level)
    {
        int totalXP = 0;
        for (int i = 1; i < level; i++)
        {
            totalXP += GetXPForNextCreatureLevel(i);
        }
        return totalXP;
    }
}
