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
    [SerializeField] int hp;
    [SerializeField] int energy;
    [SerializeField] int strength;
    [SerializeField] int magic;
    [SerializeField] int skill;
    [SerializeField] int speed;
    [SerializeField] int defense;
    [SerializeField] int resistance;

    [SerializeField] List<LearnableTalent> learnableTalents;

    public string CreatureName { get { return creatureName; } }
    public string Description { get { return description; } }
    public Sprite FrontSprite { get { return frontSprite; } }
    public CreatureType Type1 { get { return type1; } }
    public CreatureType Type2 { get { return type2; } }
    public int HP { get { return hp; } }
    public int Energy { get { return energy; } }
    public int Strength { get { return strength; } }
    public int Magic { get { return magic; } }
    public int Skill { get { return skill; } }
    public int Speed { get { return speed; } }
    public int Defense { get { return defense; } }
    public int Resistance { get { return resistance; } }
    public List<LearnableTalent> LearnableTalents { get { return learnableTalents; } }
}


