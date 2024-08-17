using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Creature
{
    const int CORE_SLOTS = 2;
    const int EMPOWERED_SLOTS = 3;

    CreatureBase species;
    int level;

    public List<CoreAction> KnownCoreActions { get; set; }

    public List<EmpoweredAction> KnownEmpoweredActions { get; set; }

    public List<CoreAction> EquippedCoreActions { get; set; }

    public List<EmpoweredAction> EquippedEmpoweredActions { get; set; }

    public Creature(CreatureBase creatureBase, int creatureLevel)
    {
        species = creatureBase;
        level = creatureLevel;

        KnownCoreActions = new List<CoreAction>();
        KnownEmpoweredActions = new List<EmpoweredAction>();

        initTalents();

        equipTalents();

    }

    public int HP { get { return calc_hp(species.HP); } }
    public int Energy { get { return calc_energy(species.Energy); } }
    public int Strength { get { return calc_stat(species.Strength); } }
    public int Magic { get { return calc_stat(species.Magic); } }
    public int Skill { get { return calc_stat(species.Skill); } }
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

    private void initTalents()
    {
        foreach (var talent in species.LearnableTalents)
        {
            if (talent.Level <= level)
            {
                if (talent.TalentBase is CoreActionBase coreActionBase)
                {
                    KnownCoreActions.Add(new CoreAction(coreActionBase));
                }
                else if (talent.TalentBase is EmpoweredActionBase empoweredActionBase)
                {
                    KnownEmpoweredActions.Add(new EmpoweredAction(empoweredActionBase));
                }
                else
                {
                    Debug.Log("Learnable Talent doesn't match known types");
                }
            }
        }
    }

    private void equipTalents()
    {
        if (KnownCoreActions.Count > 0)
        {
            int i = KnownCoreActions.Count - 1;
            int coreCount = 0;

            while (i - coreCount >= 0 && coreCount < CORE_SLOTS)
            {
                EquippedCoreActions.Add(KnownCoreActions[i]);
                coreCount++;
            }
        }

        if (KnownEmpoweredActions.Count > 0)
        {
            int i = KnownEmpoweredActions.Count - 1;
            int empoweredCount = 0;

            while (i - empoweredCount >= 0 && empoweredCount < EMPOWERED_SLOTS)
            {
                EquippedEmpoweredActions.Add(KnownEmpoweredActions[i]);
                empoweredCount++;
            }
        }
    }
}

