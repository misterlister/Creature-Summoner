using UnityEngine;

public static class BattleUIConstants
{
    // Animation constants
    public const float SUMMON_DURATION = 1f;
    public const float DEFEAT_DURATION = 1.5f;
    public const float ATTACK_DISTANCE = 40f;
    public const float ATTACK_SPEED = 0.2f;
    public const float HIT_DURATION = 0.1f;
    public const float HEAL_DURATION = 0.5f;
    public const float SLIDER_DURATION = 0.5f;

    // Sprite sizing
    public const float HUGE_SPRITE_SIZE = 170f;
    public const float LARGE_SPRITE_SIZE = 150f;
    public const float MEDIUM_SPRITE_SIZE = 130f;
    public const float SMALL_SPRITE_SIZE = 115f;
    public const float TINY_SPRITE_SIZE = 100f;

    // Row tile scaling
    public const float TILE_WIDTH_ROW0 = 220f;
    public const float TILE_HEIGHT_ROW0 = 175f;

    public const float TILE_SCALE_ROW1 = 1.1f;
    public const float TILE_SCALE_ROW2 = 1.2f;

    public static readonly Color PositiveColour = Color.green;
    public static readonly Color NegativeColour = Color.red;
    public static readonly Color NeutralColour = Color.black;

    // Menu Navigation
    public const int MENU_COLS = 3;
}

