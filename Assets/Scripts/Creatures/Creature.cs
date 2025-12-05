using System.Collections.Generic;
using UnityEngine;
using static GameConstants;


[System.Serializable]
public class Creature : ISerializationCallbackReceiver
{

    [SerializeField] CreatureBase species;
    [SerializeField] int level;
    [SerializeField] CreatureClassInstance currentClass;
    [SerializeField] private List<TraitBase> traits;

    public CreatureBase Species => species;
    public int Level => level;
    public CreatureClassInstance CurrentClass => currentClass;

    public List<CreatureClassInstance> classList;

    public List<TraitBase> Traits => traits;

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

    public int HP { get; set; }
    public int Energy { get; set; }
    public int XP { get; set; }
    public int TotalXP { get; set; }
    public string Nickname { get; set; }
    public bool IsDefeated { get; set; }

    public float GlancingDamageReduction { get; set; }

    public float StartingEnergy { get; private set; }

    public float CritDamageBonus { get; set; }
    public float CritDamageResistance { get; set; }

    public List<CreatureAction> KnownCoreActions { get; set; }
    public List<CreatureAction> KnownEmpoweredActions { get; set; }
    public List<CreatureAction> KnownMasteryActions { get; set; }

    public CreatureAction[] EquippedCoreActions { get; set; }
    public CreatureAction[] EquippedEmpoweredActions { get; set; }
    public CreatureAction[] EquippedMasteryActions { get; set; }

    public int Initiative { get; set; }
    public BattleSlot BattleSlot { get; private set; }

    public int XPForNextLevel => XPSystem.GetXPForNextCreatureLevel(Level);

    private Dictionary<StatType, float> statModifiers;

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        // If currentClass exists but has no CreatureClass assigned, nullify it
        if (currentClass != null && currentClass.CreatureClass == null)
        {
            currentClass = null;
        }
    }

    public void Init()
    {
        HP = MaxHP;
        IsDefeated = false;
        Energy = 0;
        XP = 0;
        TotalXP = XPSystem.GetTotalXPForCreatureLevel(Level);

        //if (nickname != "")
        //{
        //    Nickname = nickname;
        //} 
        //else
        //{
        Nickname = species.CreatureName;
        //}

        GlancingDamageReduction = GLANCE_REDUCTION;
        StartingEnergy = DEFAULT_STARTING_ENERGY;

        CritDamageBonus = CRIT_DAMAGE_BONUS;
        CritDamageResistance = CRIT_DAMAGE_RESISTANCE;

        KnownCoreActions = new List<CreatureAction>();
        KnownEmpoweredActions = new List<CreatureAction>();
        KnownMasteryActions = new List<CreatureAction>();
        EquippedCoreActions = new CreatureAction[CORE_SLOTS];
        EquippedEmpoweredActions = new CreatureAction[EMPOWERED_SLOTS];
        EquippedMasteryActions = new CreatureAction[MASTERY_SLOTS];

        initActions();
        equipActions();
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

    // Get all modifiers from all sources
    public void GetAllModifiers(
        BattleContext context,
        List<FlatStatModifier> flatMods,
        List<PercentStatModifier> percentMods,
        List<CombatModifier> combatMods)
    {
        flatMods.Clear();
        percentMods.Clear();
        combatMods.Clear();

        // From traits
        foreach (var trait in traits)
        {
            trait.CollectModifiers(this, context, flatMods, percentMods, combatMods);
        }

        // Add other sources here:
        // CollectStatusEffectModifiers(context, flatMods, percentMods, combatMods);
        // CollectTerrainModifiers(context, flatMods, percentMods, combatMods);
    }

    // Base Stats
    public int BaseMaxHP => CalculateBaseStat(StatType.HP);
    public int BaseMaxEnergy => CalculateBaseStat(StatType.Energy);
    public int BaseStrength => CalculateBaseStat(StatType.Strength);
    public int BaseMagic => CalculateBaseStat(StatType.Magic);
    public int BaseSkill => CalculateBaseStat(StatType.Skill);
    public int BaseSpeed => CalculateBaseStat(StatType.Speed);
    public int BaseDefense => CalculateBaseStat(StatType.Defense);
    public int BaseResistance => CalculateBaseStat(StatType.Resistance);

    // Current Stats

    public int MaxHP => ApplyBattleModifiers(BaseMaxHP, StatType.HP);
    public int MaxEnergy => ApplyBattleModifiers(BaseMaxEnergy, StatType.Energy);
    public int Strength => ApplyBattleModifiers(BaseStrength, StatType.Strength);
    public int Magic => ApplyBattleModifiers(BaseMagic, StatType.Magic);
    public int Skill => ApplyBattleModifiers(BaseSkill, StatType.Skill);
    public int Speed => ApplyBattleModifiers(BaseSpeed, StatType.Speed);
    public int Defense => ApplyBattleModifiers(BaseDefense, StatType.Defense);
    public int Resistance => ApplyBattleModifiers(BaseResistance, StatType.Resistance);

    private int CalculateBaseStat(StatType statType)
    {
        float stat = GetSpeciesBaseStat(statType);
        stat = ApplyLevelScaling(stat, statType);
        stat += GetClassModifier(statType);
        return Mathf.RoundToInt(Mathf.Max(MIN_STAT_VALUE, stat));
    }

    private float GetSpeciesBaseStat(StatType statType)
    {
        return statType switch
        {
            StatType.HP => species.HP,
            StatType.Energy =>  species.Energy,
            StatType.Strength => species.Strength,
            StatType.Magic => species.Magic,
            StatType.Skill => species.Skill,
            StatType.Speed => species.Speed,
            StatType.Defense => species.Defense,
            StatType.Resistance => species.Resistance,
            _ => 0f,
        };
    }

    private float ApplyLevelScaling(float baseStat, StatType statType)
    {
        if (statType == StatType.HP || statType == StatType.Energy)
        {
            return Mathf.FloorToInt(((baseStat * 4) * Level) / 100f) + 10 + Level;
        }
        return Mathf.FloorToInt(((baseStat * 4) * Level) / 100f) + 5;
    }

    private int GetClassModifier(StatType statType)
    {
        if (currentClass == null)
        {
            return 0;
        }
        return currentClass.GetStatModifier(statType);
    }

    private int ApplyBattleModifiers(int stat, StatType statType, BattleContext context = null)
    {
        if (context == null)
        {
            return stat;
        }

        var flatMods = new List<FlatStatModifier>();
        var percentMods = new List<PercentStatModifier>();
        var combatMods = new List<CombatModifier>();

        GetAllModifiers(context, flatMods, percentMods, combatMods);

        int totalFlat = 0;
        float totalPercent = 0f;

        foreach (var mod in flatMods)
        {
            if (mod.statType == statType)
            {
                totalFlat += mod.value;
            }
        }

        foreach (var mod in percentMods)
        {
            if (mod.statType == statType)
            {
                totalPercent += mod.value;
            }
        }

        float modifiedStat = (stat + totalFlat) * (1f + totalPercent);

        return Mathf.RoundToInt(Mathf.Max(MIN_STAT_VALUE, modifiedStat));
    }

    // Get combat multiplier
    public float GetCombatMultiplier(CombatModifierType modType, BattleContext context)
    {
        var flatMods = new List<FlatStatModifier>();
        var percentMods = new List<PercentStatModifier>();
        var combatMods = new List<CombatModifier>();
        
        GetAllModifiers(context, flatMods, percentMods, combatMods);
        
        float totalPercent = 0f;
        foreach (var mod in combatMods)
        {
            if (mod.modifierType == modType)
            {
                totalPercent += mod.value;
            }
        }

        return 1f + totalPercent;
    }

    public float compare_stat_to_average(StatType stat)
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

    private void initActions()
    {
        foreach (var learnableAction in Species.LearnableActions)
        {
            if (learnableAction.Level <= Level)
            {
                    if (learnableAction.Action.Category == ActionCategory.Core)
                    {
                        KnownCoreActions.Add(new CreatureAction(learnableAction.Action));
                    }
                    else if (learnableAction.Action.Category == ActionCategory.Empowered)
                    {
                        KnownEmpoweredActions.Add(new CreatureAction(learnableAction.Action));
                    }
                    else if (learnableAction.Action.Category == ActionCategory.Mastery)
                    {
                        KnownMasteryActions.Add(new CreatureAction(learnableAction.Action));
                    }
            }
        }
    }

    private void equipActions()
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

    public bool IsType(CreatureType type)
    {
        return (Species.Type1 == type || Species.Type2 == type);
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

    public bool IsWounded()
    { 
        return (GetHPAsPercentage() < HEALTHY_THRESHOLD); 
    }
    public bool IsHealthy()
    {
        return (GetHPAsPercentage() >= HEALTHY_THRESHOLD);
    }

    public bool IsEnergized()
    {
        return (GetEnergyAsPercentage() >= ENERGIZED_THRESHOLD);
    }

    public bool IsTired()
    {
        return (GetEnergyAsPercentage() < TIRED_THRESHOLD);
    }

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

    private void LevelUp()
    {
        // TODO
    }
}

