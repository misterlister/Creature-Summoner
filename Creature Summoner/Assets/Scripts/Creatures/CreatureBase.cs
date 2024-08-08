using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Creatures", menuName = "Creatures/Create new creature")]

public class CreatureBase : ScriptableObject
{
    [SerializeField] string creatureName;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite frontSprite;
    [SerializeField] CreatureType type1;
    [SerializeField] CreatureType type2;

    // Base Stats
    [SerializeField] int HP;
    [SerializeField] int Energy;
    [SerializeField] int Strength;
    [SerializeField] int Magic;
    [SerializeField] int Skill;
    [SerializeField] int Speed;
    [SerializeField] int Defense;
    [SerializeField] int Resistance;
}

public enum CreatureType
{
    None,
    Air,
    Arcane,
    Beast,
    Cold,
    Earth,
    Electric,
    Fire,
    Metal,
    Necrotic,
    Plant,
    Radiant,
    Water
}