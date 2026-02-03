
public static class TerrainConstants
{
    public const float LIGHT_COVER_DEFENSE_BONUS = 0.1f; // defense bonus from light cover
    public const float HEAVY_COVER_DEFENSE_BONUS = 0.25f; // defense bonus from heavy cover
    public const float HELPLESS_TERRAIN_DEFENSE_PENALTY = -0.25f; // defense penalty from helpless terrain
    public const float LIGHT_ROUGH_TERRAIN_MOVEMENT_COST = 0.1f; // energy movement cost for light rough terrain
    public const float HEAVY_ROUGH_TERRAIN_MOVEMENT_COST = 0.25f; // energy movement cost for heavy rough terrain
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
    Default,        // Standard map pattern
    Mirrored,       // Mirror layout
    FrontlineFocus, // More terrain in front
    BacklineFocus,  // More terrain in back
    Clustered,      // Grouped terrain
    CoverFocus,     // Emphasize cover terrain
    RoughFocus,     // Emphasize rough terrain
    Constricted,    // Can include narrow paths
    LightFocus,     // Emphasize light terrain
    HeavyFocus      // Emphasize heavy terrain
}

public enum TerrainFocusCategory
{
    Cover,
    Rough,
    Light,
    Heavy
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