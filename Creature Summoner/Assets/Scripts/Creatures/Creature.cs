using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Creature
{
    // Universal Creature Defaults
    const int CORE_SLOTS = 3;
    const int EMPOWERED_SLOTS = 3;
    const int MASTERY_SLOTS = 1;

    const float GLANCE_REDUCTION = 0.25f;
    const int GLANCE_CHANCE = 51;
    const float CRIT_BONUS = 0.4f;
    const float CRIT_RESISTANCE = 0.0f;

    //

    public CreatureBase Species { get; set; }
    public int Level { get; set; }
    public int HP { get; set; }
    public int Energy { get; set; }
    public string Nickname { get; set; }
    private int VitalityTraining;
    private int PowerTraining;
    private int NimbleTraining; 
    private int ResilienceTraining;

    public float GlancingDamageReduction { get; set; }
    public int ChanceToBeGlanced { get; set; }

    public float CritBonus { get; set; }
    public float CritResistance { get; set; }

    public List<CreatureAction> KnownCoreActions { get; set; }
    public List<CreatureAction> KnownEmpoweredActions { get; set; }
    public List<CreatureAction> KnownMasteryActions { get; set; }

    public CreatureAction[] EquippedCoreActions { get; set; }
    public CreatureAction[] EquippedEmpoweredActions { get; set; }
    public CreatureAction[] EquippedMasteryActions { get; set; }

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

        VitalityTraining = 0;
        PowerTraining = 0;
        NimbleTraining = 0;
        ResilienceTraining = 0;

        GlancingDamageReduction = GLANCE_REDUCTION;
        ChanceToBeGlanced = GLANCE_CHANCE;

        CritBonus = CRIT_BONUS;
        CritResistance = CRIT_RESISTANCE;

        KnownCoreActions = new List<CreatureAction>();
        KnownEmpoweredActions = new List<CreatureAction>();
        KnownMasteryActions = new List<CreatureAction>();
        EquippedCoreActions = new CreatureAction[CORE_SLOTS];
        EquippedEmpoweredActions = new CreatureAction[EMPOWERED_SLOTS];
        EquippedMasteryActions = new CreatureAction[MASTERY_SLOTS];

        initTalents();
        equipTalents();

    }

    public int MaxHP { get { return calc_hp(Species.HP); } }
    public int MaxEnergy { get { return calc_energy(Species.Energy); } }
    public int Strength { get { return calc_stat(Species.Strength, PowerTraining); } }
    public int Magic { get { return calc_stat(Species.Magic, PowerTraining); } }
    public int Skill { get { return calc_stat(Species.Skill, NimbleTraining); } }
    public int Speed { get { return calc_stat(Species.Speed, NimbleTraining); } }
    public int Defense { get { return calc_stat(Species.Defense, ResilienceTraining); } }
    public int Resistance { get { return calc_stat(Species.Resistance, ResilienceTraining); } }


    private int calc_stat(int baseStat, int trainingVal)
    {
        return Mathf.FloorToInt(((baseStat * 4 + (trainingVal / 5)) * Level) / 100f) + 5;
    }

    private int calc_hp(int baseHp)
    {
        return Mathf.FloorToInt(((baseHp * 4 + (VitalityTraining / 5)) * Level) / 100f) + 10 + Level;
    }

    private int calc_energy(int baseEnergy)
    {
        return Mathf.FloorToInt(((baseEnergy * 4 + (VitalityTraining / 5)) * Level) / 100f) + 30 + Level;
    }

    private void initTalents()
    {
        foreach (var talent in Species.LearnableTalents)
        {
            if (talent.Level <= Level)
            {
                if (talent.TalentBase is ActionBase actionBase)
                {
                    if (actionBase.Category == ActionCategory.Core)
                    {
                        KnownCoreActions.Add(new CreatureAction(actionBase));
                    }
                    else if (actionBase.Category == ActionCategory.Empowered)
                    {
                        KnownEmpoweredActions.Add(new CreatureAction(actionBase));
                    }
                    else if (actionBase.Category == ActionCategory.Mastery)
                    {
                        KnownMasteryActions.Add(new CreatureAction(actionBase));
                    }
                }
            }
        }
    }

    private void equipTalents()
    {
        if (KnownCoreActions.Count > 0)
        {
            int i = KnownCoreActions.Count - 1;

            while (i >= 0 && (EquippedCoreActions[0] == null || EquippedCoreActions[1] == null || EquippedCoreActions[2] == null))
            {
                if (KnownCoreActions[i].Action.Source == ActionSource.Physical && EquippedCoreActions[0] == null)
                {
                    EquippedCoreActions[0] = KnownCoreActions[i]; // Equip last learned Physical Core Action
                } 
                else if (KnownCoreActions[i].Action.Source == ActionSource.Magical && EquippedCoreActions[1] == null)
                {
                    EquippedCoreActions[1] = KnownCoreActions[i]; // Equip last learned Magical Core Action
                } 
                else if (KnownCoreActions[i].Action.Source == ActionSource.Defensive && EquippedCoreActions[2] == null)
                {
                    EquippedCoreActions[2] = KnownCoreActions[i]; // Equip last learned Defensive Core Action
                }
                i--;
            }
        }

        if (KnownEmpoweredActions.Count > 0)
        {
            int i = KnownEmpoweredActions.Count - 1;
            int slot = 0;

            while (i >= 0 && slot < EMPOWERED_SLOTS)
            {
                EquippedEmpoweredActions[slot] = KnownEmpoweredActions[i];
                slot++;
                i--;
            }
        }

        if (KnownMasteryActions.Count > 0) 
        {
            int i = KnownMasteryActions.Count - 1;
            int slot = 0;

            while (i >= 0 && slot < MASTERY_SLOTS)
            {
                EquippedMasteryActions[slot] = KnownMasteryActions[i];
                slot++;
                i--;
            }
        }
    }

    public void RemoveHP(int amount)
    {
        if (amount < 0)
        {
            amount = -amount;
        }
        if (amount > HP)
        {
            HP = 0;
        }
        else
        {
            HP -= amount;
        }
    }

    public void AddHP(int amount)
    {
        if (amount < 0)
        {
            amount = -amount;
        }
        if (amount + HP > MaxHP)
        {
            HP = MaxHP;
        } 
        else
        {
            HP += amount;
        }
    }
    public void RemoveEnergy(int amount)
    {
        if (amount < 0)
        {
            amount = -amount;
        }
        if (amount > Energy)
        {
            Energy = 0;
        }
        else
        {
            Energy -= amount;
        }
    }

    public void AddEnergy(int amount)
    {
        if (amount < 0)
        {
            amount = -amount;
        }
        if (amount + Energy > MaxEnergy)
        {
            Energy = MaxEnergy;
        }
        else
        {
            Energy += amount;
        }
    }

}

