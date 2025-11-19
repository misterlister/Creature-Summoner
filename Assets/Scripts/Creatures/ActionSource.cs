using UnityEngine;

public enum ActionSource
{
    None,
    Physical,
    Magical
}

public enum ActionClass
{
    Attack,
    Support
}

public enum ActionCategory
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
    Healing
}