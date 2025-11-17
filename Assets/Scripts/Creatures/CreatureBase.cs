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
    [SerializeField] CreatureSize size;

    // Base Stats
    [SerializeField] int hp;
    [SerializeField] int energy;
    [SerializeField] int strength;
    [SerializeField] int magic;
    [SerializeField] int skill;
    [SerializeField] int speed;
    [SerializeField] int defense;
    [SerializeField] int resistance;

    [SerializeField] List<LearnableAction> learnableActions;

    public string CreatureName => creatureName;
    public string Description => description;
    public Sprite FrontSprite => frontSprite;
    public CreatureType Type1 => type1;
    public CreatureType Type2 => type2;
    public CreatureSize Size => size;
    public int HP => hp;
    public int Energy => energy;
    public int Strength => strength;
    public int Magic => magic;
    public int Skill => skill;
    public int Speed => speed;
    public int Defense => defense;
    public int Resistance => resistance;
    public List<LearnableAction> LearnableActions => learnableActions;
}


