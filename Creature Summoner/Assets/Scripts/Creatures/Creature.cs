using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature
{
    CreatureBase species;
    int level;

    public Creature(CreatureBase creatureBase, int creatureLevel)
    {
        species = creatureBase;
        level = creatureLevel;
    }

    public int HP {  get { return calc_hp(species.HP); } }
    public int Energy { get { return calc_energy(species.Energy); } }
    public int Strength { get { return calc_stat(species.Strength); } }
    public int Magic { get {  return calc_stat(species.Magic); } }
    public int Skill { get  { return calc_stat(species.Skill); } }
    public int Speed { get { return calc_stat(species.Speed); } }
    public int Defense { get { return calc_stat(species.Defense); } }
    public int Resistance { get { return calc_stat(species.Resistance); } }


    private int calc_stat(int baseStat)
    {
        return Mathf.FloorToInt((baseStat * level) / 100f) + 5;
    }

    private int calc_hp(int baseHp)
    {
        return Mathf.FloorToInt((baseHp * level) / 100f) + 10 + level;
    }

    private int calc_energy(int baseEnergy)
    {
        return Mathf.FloorToInt((baseEnergy * level) / 100f) + 30 + level;
    }
}

