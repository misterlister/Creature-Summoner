using Game.Battle.Modifiers;
using Game.Statuses;
using System;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
using Game.Traits;
using Game.Creatures.Managers;

public class Creature
{
    private CreatureBase species;
    private int level;
    private CreatureClassInstance currentClass;
    private List<TraitBase> traits;

    public CreatureBase Species => species;
    public int Level => level;
    public CreatureClassInstance CurrentClass => currentClass;
    public List<TraitBase> Traits => traits;
    public List<CreatureClassInstance> ClassList { get; private set; }

    private List<RuntimeTrait> runtimeTraits = new();
    public CreatureEventProxy EventProxy { get; private set; }

    public string ClassName
    {
        get
        {
            if (currentClass != null && currentClass.CreatureClass != null)
            {
                return currentClass.CreatureClass.CreatureClassName;
            }
            return "No Class";
        }
    }

    // Managers
    public StatManager Stats { get; private set; }
    public ActionManager Actions { get; private set; }
    public CreatureStatusManager Statuses { get; private set; }
    public CreatureCombatModifiers CombatModifiers { get; private set; }

    private int currentHP;
    private int currentEnergy;
    private bool isDefeated;

    // Base Stats
    public int MaxHP => Stats.GetBaseStat(StatType.HP);
    public int MaxEnergy => Stats.GetBaseStat(StatType.Energy);
    public int BaseStrength => Stats.GetBaseStat(StatType.Strength);
    public int BaseMagic => Stats.GetBaseStat(StatType.Magic);
    public int BaseSkill => Stats.GetBaseStat(StatType.Skill);
    public int BaseSpeed => Stats.GetBaseStat(StatType.Speed);
    public int BaseDefense => Stats.GetBaseStat(StatType.Defense);
    public int BaseResistance => Stats.GetBaseStat(StatType.Resistance);

    // Current Stats
    public int HP
    {
        get => currentHP;
        private set
        {
            int oldHP = currentHP;
            currentHP = Mathf.Clamp(value, 0, MaxHP);
            if (currentHP != oldHP)
            {
                OnHPChanged?.Invoke(currentHP, MaxHP);

                if (currentHP <= 0 && !isDefeated)
                {
                    Defeated();
                }
            }
        }
    }
    public int Energy
    {
        get => currentEnergy;
        private set
        {
            int oldEnergy = currentEnergy;
            currentEnergy = Mathf.Clamp(value, 0, MaxEnergy);
            if (currentEnergy != oldEnergy)
            {
                OnEnergyChanged?.Invoke(currentEnergy, MaxEnergy);
            }
        }
    }
    public int Strength => Stats.GetCurrentStat(StatType.Strength);
    public int Magic => Stats.GetCurrentStat(StatType.Magic);
    public int Skill => Stats.GetCurrentStat(StatType.Skill);
    public int Speed => Stats.GetCurrentStat(StatType.Speed);
    public int Defense => Stats.GetCurrentStat(StatType.Defense);
    public int Resistance => Stats.GetCurrentStat(StatType.Resistance);

    // Progression
    public int XP { get; set; }
    public int TotalXP { get; set; }
    public int XPForNextLevel => XPSystem.GetXPForNextCreatureLevel(Level);


    public string Nickname { get; set; }
    public bool IsDefeated => isDefeated;

    // REMOVE ONCE IMPLEMENTATION OF BATTLECONTEXT IS COMPLETE
    public float GlancingDamageReduction { get; set; }
    public float StartingEnergy { get; private set; }
    public float CritDamageBonus { get; set; }
    public float CritDamageResistance { get; set; }
    //

    public int Initiative { get; set; }
    public BattleSlot BattleSlot { get; private set; }


    public event Action<int, int> OnHPChanged;
    public event Action<int> OnDamageTaken;
    public event Action<int> OnHealed;
    public event Action<int, int> OnEnergyChanged;
    public event Action<int> OnEnergySpent;
    public event Action<int> OnEnergyGained;
    public event Action OnClassChanged;
    public event Action OnLevelUp;
    public event Action OnDefeated;
    public event Action OnRevived;
    public event Action<StatusEffect> OnStatusAdded;
    public event Action<StatusEffect> OnStatusRemoved;

    /*
    public static Creature FromConfig(CreatureConfig config)
    {
        if (config == null || config.Species == null)
        {
            Debug.LogError("Invalid CreatureConfig provided");
            return null;
        }

        Creature creature = new Creature
        {
            species = config.Species,
            level = Mathf.Clamp(config.Level, 1, MAX_LEVEL),
            currentClass = config.StartingClass,
            traits = new List<TraitBase>(config.Traits),
            ClassList = new List<CreatureClassInstance>(config.ClassList)
        };

        creature.Nickname = string.IsNullOrEmpty(config.Nickname) 
            ? creature.Species.CreatureName 
            : config.Nickname;

        bool hasLoadout = config.Loadout != null;
        creature.Init(autoEquipActions: !hasLoadout);

        if (hasLoadout)
        {
            creature.Actions.LoadActionLoadout(config.Loadout, fillEmptySlots: true);
        }

        return creature;
    }
    */

    public void Init(bool autoEquipActions = true)
    {
        // Initialize Managers
        Stats = new StatManager(this);
        Actions = new ActionManager(this);
        Statuses = new CreatureStatusManager(this);
        CombatModifiers = new CreatureCombatModifiers(this);

        // Set starting resources
        currentHP = MaxHP;
        currentEnergy = 0;
        isDefeated = false;

        // Set XP
        XP = 0;
        TotalXP = XPSystem.GetTotalXPForCreatureLevel(Level);

        if (Nickname == "")
        { 
            Nickname = species.CreatureName;
        }

        GlancingDamageReduction = GLANCE_REDUCTION;
        StartingEnergy = DEFAULT_STARTING_ENERGY;
        CritDamageBonus = CRIT_DAMAGE_BONUS;
        CritDamageResistance = CRIT_DAMAGE_RESISTANCE;

        Actions.InitializeKnownActions();

        if (autoEquipActions)
        {
            Actions.SetupDefaultEquippedActions();
        }
    }

    public void InitializeBattle(BattleEventManager eventManager)
    {
        EventProxy = new CreatureEventProxy(this, eventManager);

        foreach (var trait in Traits)
        {
            RuntimeTrait runtimeTrait = new RuntimeTrait(trait, this);
            runtimeTrait.Subscribe(EventProxy);
            runtimeTraits.Add(runtimeTrait);
        }
    }

    public void CleanupBattle()
    {
        foreach (var runtimeTrait in runtimeTraits)
        {
            runtimeTrait.Unsubscribe(EventProxy);
        }

        runtimeTraits.Clear();

        EventProxy?.Cleanup();
        EventProxy = null;
    }

    public void ChangeClass(CreatureClassInstance newClass)
    {
        if (newClass == currentClass)
        {
            return;
        }

        int oldMaxHP = MaxHP;
        int oldMaxEnergy = MaxEnergy;

        bool wasDefeated = IsDefeated;

        currentClass = newClass;
        Stats.MarkBaseStatsDirty();

        int hpChange = MaxHP - oldMaxHP;
        int energyChange = MaxEnergy - oldMaxEnergy;

        if ((HP - hpChange <= 0)) 
        {
            HP = 1;
        }
        else
        {
            HP += hpChange;
        }

        Energy += energyChange;

        OnClassChanged?.Invoke();

        string hpChangeStr = hpChange >= 0 ? $"+{hpChange}" : hpChange.ToString();
        string energyChangeStr = energyChange >= 0 ? $"+{energyChange}" : energyChange.ToString();
        Debug.Log($"{Nickname} changed class to {currentClass.CreatureClass.CreatureClassName}! " +
                  $"HP {hpChangeStr}, Energy {energyChangeStr}");
    }

    public void AddTrait(TraitBase trait)
    {
        if (!Traits.Contains(trait))
        {
            Traits.Add(trait);
        }

        if (EventProxy != null)
        {
            RuntimeTrait runtimeTrait = new RuntimeTrait(trait, this);
            runtimeTrait.Subscribe(EventProxy);
            runtimeTraits.Add(runtimeTrait);
        }
    }

    public void RemoveTrait(TraitBase trait)
    {
        if (Traits.Contains(trait))
        {
            Traits.Remove(trait);
        }

        RuntimeTrait toRemove = runtimeTraits.Find(rt => rt.TraitData == trait);
        if (toRemove != null)
        {
            toRemove.Unsubscribe(EventProxy);
            runtimeTraits.Remove(toRemove);
        }
    }

    // Get all stat modifiers from all sources
    public void GetAllStatModifiers(BattleContext context, List<StatModifier> mods)
    {
        mods.Clear();

        // From traits
        foreach (var trait in traits)
        {
            trait.CollectStatModifiers(this, context, mods);
        }

        // From statuses
        // Statuses.CollectStatModifiers(this, context, mods);

        // From terrain
    }

    // Get all combat modifiers from all sources
    public void GetAllCombatModifiers(BattleContext context, List<CombatModifier> mods)
    {
        mods.Clear();

        // From traits
        foreach (var trait in traits)
        {
            trait.CollectCombatModifiers(this, context, mods);
        }

        // From statuses (if you add them later)
        // Statuses.CollectCombatModifiers(this, context, mods);
    }

    public float Compare_stat_to_average(StatType stat)
    {
        int statVal = 0;
        switch (stat)
        {
            case StatType.Strength:
                statVal = Strength;
                break;
            case StatType.Magic:
                statVal = Magic;
                break;
            case StatType.Skill:
                statVal = Skill;
                break;
            case StatType.Speed:
                statVal = Speed;
                break;
            case StatType.Defense:
                statVal = Defense;
                break;
            case StatType.Resistance:
                statVal = Resistance;
                break;
        }
        int averageStat = Mathf.FloorToInt(((AVERAGE_BASE_STAT * 4) * Level) / 100f) + 5;
        return (float)statVal / averageStat;
    }

    private void Defeated()
    {
        isDefeated = true;
        OnDefeated?.Invoke();
    }

    private void Revive()
    {
        isDefeated = false;
    }

    public void RollInitiative()
    {
        Initiative = UnityEngine.Random.Range(Speed / 2, Speed);
    }

    public void TakeDamage(int amount)
    {
        if (amount < 0) amount = -amount;

        int previousHP = HP;
        currentHP = Mathf.Max(0, HP - amount);

        OnHPChanged?.Invoke(HP, MaxHP);

        if (HP <= 0 && !IsDefeated)
        {
            Defeated();
        }
    }

    public void Heal(int amount)
    {
        if (amount < 0)
        {
            amount = -amount;
        }
        if (amount + HP > MaxHP)
        {
           currentHP = MaxHP;
        }
        else
        {
            currentHP += amount;
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
            currentEnergy = 0;
        }
        else
        {
            currentEnergy -= amount;
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
            currentEnergy = MaxEnergy;
        }
        else
        {
            currentEnergy += amount;
        }
    }

    public void AddXP(int amount)
    {
        if (amount < 0)
        {
            return;
        }

        XP += amount;

        if (amount + XP > XPForNextLevel)
        {
            LevelUp();
        }
    }
    private void LevelUp()
    {
        int oldMaxHP = MaxHP;
        int oldMaxEnergy = MaxEnergy;

        level++;
        Stats.MarkBaseStatsDirty();

        // Increase HP and Energy based on new max values
        int hpIncrease = MaxHP - oldMaxHP;
        int energyIncrease = Mathf.CeilToInt((MaxEnergy - oldMaxEnergy) * StartingEnergy);

        HP += hpIncrease;
        Energy += energyIncrease;

        OnLevelUp?.Invoke();

        Debug.Log($"{Nickname} leveled up to {level}! HP +{hpIncrease}, Energy +{energyIncrease}");

    }

    public bool IsSingleElement()
    {
        if (Species.Element2 == CreatureElement.None || Species.Element1 == Species.Element2)
        {
            return true;
        }
        return false;
    }

    public void SetBattleSlot(BattleSlot slot)
    {
        BattleSlot = slot;
    }

    public bool IsElement(CreatureElement type)
    {
        return (Species.Element1 == type || Species.Element2 == type);
    }

    public int GetHPAsPercentage()
    {
        if (MaxHP <= 0 || HP <= 0)
        {
            return 0;
        }
        return Mathf.CeilToInt(((float)HP / MaxHP) * 100f);
    }

    public int GetEnergyAsPercentage()
    {
        if (MaxEnergy <= 0 || Energy <= 0)
        {
            return 0;
        }
        return Mathf.CeilToInt(((float)Energy / MaxEnergy) * 100f);
    }

    public bool IsWounded() => GetHPAsPercentage() < HEALTHY_THRESHOLD;
    public bool IsHealthy() => GetHPAsPercentage() >= HEALTHY_THRESHOLD;
    public bool IsEnergized() => GetEnergyAsPercentage() >= ENERGIZED_THRESHOLD;
    public bool IsTired() => GetEnergyAsPercentage() < TIRED_THRESHOLD;

    public bool IsAlly(Creature other)
    {
        // Ally doesn't include this creature itself
        if (other == this)
        {
            return false;
        }

        if (BattleSlot == null || other.BattleSlot == null)
        {
            return false;
        }

        return BattleSlot.IsPlayerSlot == other.BattleSlot.IsPlayerSlot;
    }

    public bool IsEnemy(Creature other)
    {
        if (BattleSlot == null || other.BattleSlot == null)
        {
            return false;
        }
        return BattleSlot.IsPlayerSlot != other.BattleSlot.IsPlayerSlot;
    }

    public bool IsTurnActive()
    {
        return true;
    }
}