using System;
using System.Collections.Generic;
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
    public const float MAX_SPRITE_SIZE = 135f;
    public const float BASE_SPRITE_SIZE = 105f;
    public const float SIZE_INCREMENT = 15f;

    public static readonly Color PositiveColour = Color.green;
    public static readonly Color NegativeColour = Color.red;
    public static readonly Color NeutralColour = Color.black;

    // Menu Navigation
    public const int MENU_COLS = 3;
}

