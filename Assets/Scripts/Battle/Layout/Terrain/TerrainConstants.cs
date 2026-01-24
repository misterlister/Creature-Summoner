
public static class TerrainConstants
{
    public const float LIGHT_COVER_DEFENSE_BONUS = 0.1f; // defense bonus from light cover
    public const float HEAVY_COVER_DEFENSE_BONUS = 0.25f; // defense bonus from heavy cover
    public const float HELPLESS_TERRAIN_DEFENSE_PENALTY = -0.25f; // defense penalty from helpless terrain
    public const float LIGHT_ROUGH_TERRAIN_MOVEMENT_COST = 0.1f; // energy movement cost for light rough terrain
    public const float HEAVY_ROUGH_TERRAIN_MOVEMENT_COST = 0.25f; // energy movement cost for heavy rough terrain
    public const float HAZARDOUS_TERRAIN_DAMAGE_PER_TURN = 0.25f; // Max HP Damage taken per turn in hazardous terrain

    public const float EARTH_DEFENSE_BONUS_MULTIPLIER = 1.5f; // Multiplier for Earth creature defense bonuses from cover
}

/// <summary>
/// Determines how terrain effects are calculated and applied
/// </summary>
public enum TerrainTypeEnum
{
    Regular,            // No effect
    LightCover,         // Defensive buff from ranged attacks
    HeavyCover,         // Large defensive buff, tile is impassable
    LightRough,         // Movement cost penalty
    HeavyRough,         // Large movement cost penalty
    Water,              // Heavy rough + defensive penalty
    Lava,               // Heavy rough + damage per turn
    Chasm               // Impassable, instant defeat
}

/// <summary>
/// When surface effects trigger
/// </summary>
public enum SurfaceTriggerTiming
{
    OnEnter,      // When creature enters tile
    OnTurnStart,  // At start of creature's turn
    OnTurnEnd,    // At end of creature's turn
    OnExit        // When creature leaves tile
}

/// <summary>
/// How terrain should be generated
/// </summary>
public enum TerrainGenerationMode
{
    None,       // Empty grid
    UsePreset,  // Use predefined layout
    Procedural  // Generate based on rules
}

/// <summary>
/// Patterns for procedural generation
/// </summary>
public enum TerrainGenerationPattern
{
    Random,         // Random placement
    Symmetric,      // Mirror layout
    FrontlineHeavy, // More terrain in front
    BacklineHeavy,  // More terrain in back
    Scattered,      // Spread out placement
    Clustered       // Grouped terrain
}

/// <summary>
/// Cardinal directions for grid movement
/// </summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}