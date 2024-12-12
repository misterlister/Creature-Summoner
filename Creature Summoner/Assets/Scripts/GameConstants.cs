using UnityEngine;

public static class GameConstants
{
    public const int BATTLE_ROWS = 3; // Number of Rows on each side of the battlefield
    public const int BATTLE_COLS = 2; // Number of Columns on each side of the battlefield
    public const int TEAM_SPACES = BATTLE_COLS * BATTLE_ROWS;

    public const int BATTLE_MENU_COLS = 2; // Number of Columns in the battle selection menu

    public const int CORE_SLOTS = 3;
    public const int EMPOWERED_SLOTS = 3;
    public const int MASTERY_SLOTS = 1;

    public const float GLANCE_REDUCTION = 0.25f;
    public const int GLANCE_CHANCE = 51;
    public const float CRIT_BONUS = 0.4f;
    public const float CRIT_RESISTANCE = 0.0f;
}
