using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public const int BATTLE_ROWS = 3; // Number of Rows on the battlefield
    public const int BATTLE_COLS = 4; // Number of Columns on the battlefield
    //public const int TEAM_SPACES = BATTLE_COLS * BATTLE_ROWS;
    public const int ENEMY_COL = BATTLE_COLS / 2;

    public const int BATTLE_MENU_COLS = 2; // Number of Columns in the battle selection menu

    public const int CORE_SLOTS = 3;
    public const int EMPOWERED_SLOTS = 3;
    public const int MASTERY_SLOTS = 1;

    public const float GLANCE_REDUCTION = 0.25f;
    public const int GLANCE_CHANCE = 51;
    public const float CRIT_BONUS = 0.4f;
    public const float CRIT_RESISTANCE = 0.0f;

    public const float DEFAULT_STARTING_ENERGY = 0.25f;

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
}
