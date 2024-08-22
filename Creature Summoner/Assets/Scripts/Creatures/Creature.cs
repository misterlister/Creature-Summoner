using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Creature
{
    const int EMPOWERED_SLOTS = 3;

    public CreatureBase Species { get; set; }
    public int Level { get; set; }
    public int HP { get; set; }
    public int Energy { get; set; }
    public string Nickname { get; set; }

    public List<CoreAction> KnownCoreActions { get; set; }
    public List<EmpoweredAction> KnownEmpoweredActions { get; set; }

    public CoreAction PhysicalCore { get; set; }
    public CoreAction MagicalCore { get; set; }
    public CoreAction DefensiveCore { get; set; }
    public List<EmpoweredAction> EquippedEmpoweredActions { get; set; }

    public Creature(CreatureBase creatureBase, int creatureLevel, string nickname = "")
    {
        Species = creatureBase;
        Level = creatureLevel;
        HP = MaxHP;
        Energy = 0;
        if (nickname != "")
        {
            Nickname = nickname;
        } 
        else
        {
            Nickname = creatureBase.CreatureName;
        }

        KnownCoreActions = new List<CoreAction>();
        KnownEmpoweredActions = new List<EmpoweredAction>();
        PhysicalCore = null;
        MagicalCore = null;
        DefensiveCore = null;
        EquippedEmpoweredActions = new List<EmpoweredAction>();

        initTalents();
        equipTalents();

    }

    public int MaxHP { get { return calc_hp(Species.HP); } }
    public int MaxEnergy { get { return calc_energy(Species.Energy); } }
    public int Strength { get { return calc_stat(Species.Strength); } }
    public int Magic { get { return calc_stat(Species.Magic); } }
    public int Skill { get { return calc_stat(Species.Skill); } }
    public int Speed { get { return calc_stat(Species.Speed); } }
    public int Defense { get { return calc_stat(Species.Defense); } }
    public int Resistance { get { return calc_stat(Species.Resistance); } }


    private int calc_stat(int baseStat)
    {
        return Mathf.FloorToInt((baseStat * Level) / 100f) + 5;
    }

    private int calc_hp(int baseHp)
    {
        return Mathf.FloorToInt((baseHp * Level) / 100f) + 10 + Level;
    }

    private int calc_energy(int baseEnergy)
    {
        return Mathf.FloorToInt((baseEnergy * Level) / 100f) + 30 + Level;
    }

    private void initTalents()
    {
        foreach (var talent in Species.LearnableTalents)
        {
            if (talent.Level <= Level)
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

            while (i >= 0 && (PhysicalCore == null || MagicalCore == null || DefensiveCore == null))
            {
                if (KnownCoreActions[i].Action.Category == ActionCategory.Physical && PhysicalCore == null)
                {
                    PhysicalCore = KnownCoreActions[i];
                } 
                else if (KnownCoreActions[i].Action.Category == ActionCategory.Magical && MagicalCore == null)
                {
                    MagicalCore = KnownCoreActions[i];
                } 
                else if (KnownCoreActions[i].Action.Category == ActionCategory.Defensive && DefensiveCore == null)
                {
                    DefensiveCore = KnownCoreActions[i];
                }
                i--;
            }
        }

        if (KnownEmpoweredActions.Count > 0)
        {
            int i = KnownEmpoweredActions.Count - 1;
            int empoweredCount = 0;

            while (i >= 0 && empoweredCount < EMPOWERED_SLOTS)
            {
                EquippedEmpoweredActions.Add(KnownEmpoweredActions[i]);
                empoweredCount++;
                i--;
            }
        }
    }
}

