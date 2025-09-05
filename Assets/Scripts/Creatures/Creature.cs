using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;


[System.Serializable]
public class Creature
{

    [SerializeField] CreatureBase species;
    [SerializeField] int level;

    public CreatureBase Species {
        get {
            return species;
        }
    }

    public int Level {
        get {
            return level;
        } 
    }

    public int HP { get; set; }
    public int Energy { get; set; }
    public int XP { get; set; }
    public int TotalXP { get; set; }
    public int XPForNextLevel { get; set; }
    public string Nickname { get; set; }
    public bool IsDefeated { get; set; }
    private int VitalityTraining;
    private int PowerTraining;
    private int NimbleTraining; 
    private int ResilienceTraining;

    public float GlancingDamageReduction { get; set; }

    public float StartingEnergy { get; private set; }

    public float CritBonus { get; set; }
    public float CritResistance { get; set; }

    public List<CreatureAction> KnownCoreActions { get; set; }
    public List<CreatureAction> KnownEmpoweredActions { get; set; }
    public List<CreatureAction> KnownMasteryActions { get; set; }

    public CreatureAction[] EquippedCoreActions { get; set; }
    public CreatureAction[] EquippedEmpoweredActions { get; set; }
    public CreatureAction[] EquippedMasteryActions { get; set; }

    public int Initiative { get; set; }
    public BattleSlot BattleSlot { get; private set; }

    public void Init()
    {
        HP = MaxHP;
        IsDefeated = false;
        Energy = 0;
        XP = 0;
        TotalXP = Level * 100; //Temp Simplified Placeholder
        XPForNextLevel = (Level + 1) * 100; //Temp Simplified Placeholder

        //if (nickname != "")
        //{
        //    Nickname = nickname;
        //} 
        //else
        //{
        Nickname = species.CreatureName;
        //}

        VitalityTraining = 0;
        PowerTraining = 0;
        NimbleTraining = 0;
        ResilienceTraining = 0;

        GlancingDamageReduction = GLANCE_REDUCTION;

        StartingEnergy = DEFAULT_STARTING_ENERGY;

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

    public float compare_stat_to_average(Stat stat)
    {
        int statVal = 0;
        switch (stat)
        {
            case Stat.Strength:
                statVal = Strength;
                break;
            case Stat.Magic:
                statVal = Magic;
                break;
            case Stat.Skill:
                statVal = Skill;
                break;
            case Stat.Speed:
                statVal = Speed;
                break;
            case Stat.Defense:
                statVal = Defense;
                break;
            case Stat.Resistance:
                statVal = Resistance;
                break;
        }
        int averageStat = Mathf.FloorToInt(((AVERAGE_STAT * 4) * Level) / 100f) + 5;
        return (float)statVal / averageStat;
    }

    private int calc_hp(int baseHp)
    {
        return Mathf.FloorToInt(((baseHp * 4 + (VitalityTraining / 5)) * Level) / 100f) + 10 + Level;
    }

    private int calc_energy(int baseEnergy)
    {
        return Mathf.FloorToInt(((baseEnergy * 4 + (VitalityTraining / 5)) * Level) / 100f) + 10 + Level;
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
                CreatureAction currentAction = KnownCoreActions[i];
                if (currentAction.Action.Source == ActionSource.Physical && currentAction.Action.ActionClass == ActionClass.Attack)
                {
                    if (EquippedCoreActions[0] == null)
                    {
                        EquippedCoreActions[0] = currentAction; // Equip last learned Physical Attack Core Action
                    }
                } 
                else if (currentAction.Action.Source == ActionSource.Magical && currentAction.Action.ActionClass == ActionClass.Attack)
                {
                    if (EquippedCoreActions[1] == null)
                    {
                        EquippedCoreActions[1] = currentAction; // Equip last learned Magical Attack Core Action
                    }
                } 
                else if (currentAction.Action.ActionClass == ActionClass.Support)
                {
                    if (EquippedCoreActions[2] == null)
                    {
                        EquippedCoreActions[2] = currentAction; // Equip last learned Supoort Core Action
                    }
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

    private void Defeated()
    {
        IsDefeated = true;
    }

    private void Revive()
    {
        IsDefeated = false;
    }

    public void RollInitiative()
    {
        Initiative = Random.Range(Speed / 2, Speed);
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
        if (HP <= 0)
        {
            Defeated();
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

    public void AddXP(int amount)
    {
        /*
        if (amount < 0)
        {
            amount = -amount;
        }
        if (amount + XP > 100)
        {
            Energy = MaxEnergy;
        }
        else
        {
            Energy += amount;
        }
        */
    }
    public bool IsSingleType()
    {
        if (Species.Type2 == CreatureType.None || Species.Type1 == Species.Type2)
        {
            return true;
        }
        return false;
    }

    public void SetBattleSlot(BattleSlot slot)
    {
        BattleSlot = slot;
    }
}

