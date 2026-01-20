using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static GameConstants;
using static UnityEditor.UIElements.ToolbarMenu;

[CreateAssetMenu(fileName = "NewAction", menuName = "Action/Create new Action")]
public class ActionBase : ScriptableObject
{
    [SerializeField] string actionName;
    [TextArea]
    [SerializeField] string description;

    public string ActionName => actionName;
    public string Description => description;

    //
    bool DEBUG = false;
    //

    [SerializeField] CreatureElement element;
    [SerializeField] ActionSlotType slotType;
    [SerializeField] ActionSource source;
    [SerializeField] ActionRole role;
    [SerializeField] ActionRange range;
    [SerializeField] int energyCost = 0;
    [SerializeField] int energyGain = 0;
    [SerializeField] int power = 40;
    [SerializeField] int accuracy = 90;
    [SerializeField] int baseCrit = 5;
    [SerializeField] AOE areaOfEffect = AOE.Single;
    [SerializeField] bool preparation = false;
    [SerializeField] List<ActionTag> tags;

    public CreatureElement Element => element;
    public ActionSlotType SlotType => slotType;
    public ActionSource Source => source;
    public ActionRole Role => role;
    public int Power => power;
    public int Accuracy => accuracy;
    public ActionRange Range => range;
    public bool Preparation => preparation;
    public AOE AreaOfEffect => areaOfEffect;
    public int BaseCrit => baseCrit;
    public List<ActionTag> Tags => tags;
    public int EnergyCost => energyCost;
    public int EnergyGain => energyGain;

    public int CalculateAccuracy(Creature attacker, Creature defender)
    {
        float ratio = ((attacker.Skill - (float)defender.Speed) / defender.Speed);
        float hitChance = accuracy * (1 + ACCURACY_ADJUSTMENT_FACTOR * ratio);
        float clampedHitChance = Mathf.Clamp(hitChance, MIN_HIT, MAX_HIT);
        if (DEBUG)
        {
            Debug.Log($"Hit ratio: {ratio}");
            Debug.Log($"Raw hit Chance: {hitChance}");
            Debug.Log($"Clamped hit Chance: {clampedHitChance}");
        }
        return (int)(clampedHitChance);
    }

    public int CalculateStatusAccuracy(Creature attacker, Creature defender)
    {
        int defenseStat = (Source == ActionSource.Magical)? defender.Defense : defender.Resistance;
        int statusResist = defenseStat + defender.Energy;
        float ratio = ((attacker.Skill - (float)statusResist) / statusResist);
        float statusChance = accuracy * (1 + STATUS_RESIST_ADJUSTMENT_FACTOR * ratio);
        float clampedStatusChance = Mathf.Clamp(statusChance, MIN_HIT, MAX_HIT);
        if (DEBUG)
        {
            Debug.Log($"Status resist ratio: {ratio}");
            Debug.Log($"Raw status Chance: {statusChance}");
            Debug.Log($"Clamped status Chance: {clampedStatusChance}");
        }
        return (int)(clampedStatusChance);
    }

    public int CalculateDamage(Creature attacker, Creature defender, float critMod = 1.0f)
    {
        float attack = 1f;
        float defense = 1f;

        if (source == ActionSource.Physical)
        {
            attack = attacker.Strength;
            defense = defender.Defense;
        }

        if (source == ActionSource.Magical)
        {
            attack = attacker.Magic;
            defense = defender.Resistance;
        }

        if (critMod > 1.0f)
        {
            defense = Mathf.Max(attack, defense * 0.8f);
        }

        float variance = getVariance(attacker, defender);

        float damage = ((((attacker.Level / 3 + 1) * power * attack / defense) / 50 + 1) * critMod * variance);

        return Mathf.Max((int)damage, 1);
    }

    public int CalculateHealing(Creature healer, int healingValue, float critMod = 1.0f)
    {
        float creature_power = 1f;

        if (source == ActionSource.Physical)
        {
            creature_power = healer.Compare_stat_to_average(StatType.Strength);
        }

        if (source == ActionSource.Magical)
        {
            creature_power = healer.Compare_stat_to_average(StatType.Magic);
        }
        float variance = getHealVariance(healer);
        float healing = ((((healer.Level / 3 + 1) * power * creature_power) / 50 + 1) * critMod * variance);

        return Mathf.Max((int)healing, 1);
    }

    private float getVariance(Creature attacker, Creature defender)
    {
        float ratio = (float)attacker.Skill / defender.Speed;
        int varianceRange = (int)(BASE_VARIANCE + ((ratio - 1) * VARIANCE_ADJUSTMENT));
        int clampedVarianceRange = Mathf.Clamp(varianceRange, MIN_VARIANCE, MAX_VARIANCE);
        int variance = Random.Range(clampedVarianceRange, ROLL_CEILING);
        if (DEBUG)
        {
            Debug.Log($"Ratio: {ratio}");
            Debug.Log($"VarianceRange: {varianceRange}");
            Debug.Log($"clampedVarianceRange: {clampedVarianceRange}");
            Debug.Log($"variance roll: {variance}");
        }
        return variance / 100f;
    }

    private float getHealVariance(Creature healer)
    {
        float ratio = healer.Compare_stat_to_average(StatType.Skill);
        int varianceRange = (int)(BASE_VARIANCE + ((ratio - 1) * VARIANCE_ADJUSTMENT));
        int clampedVarianceRange = Mathf.Clamp(varianceRange, MIN_VARIANCE, MAX_VARIANCE);
        int variance = Random.Range(clampedVarianceRange, ROLL_CEILING);
        if (DEBUG)
        {
            Debug.Log($"Ratio: {ratio}");
            Debug.Log($"VarianceRange: {varianceRange}");
            Debug.Log($"clampedVarianceRange: {clampedVarianceRange}");
            Debug.Log($"variance roll: {variance}");
        }
        return variance / 100f;
    }

    private bool rollToHit(int accuracy)
    {
        int roll = Random.Range(1, ROLL_CEILING);
        if (DEBUG)
        {
            Debug.Log($"accuracy: {accuracy}");
            Debug.Log($"to hit roll: {roll}");
        }
        if (roll <= accuracy)
        {
            return true;
        }
        return false;
    }

    public List<string> UseAction(BattleSlot attacker, BattleSlot defender)
    {
        if (DEBUG)
        {
            Debug.Log($"-------attacker: {attacker.Creature.Nickname}-------");
        }

        if (role == ActionRole.Attack)
        {
            return UseOffensiveAction(attacker, defender);
        }

        if (tags.Contains(ActionTag.Healing))
        {
            return UseHealingAction(attacker, defender);
        }

        else
        {
            return UseDefenseAction(attacker, defender);
        }

    }

    public List<string> UseOffensiveAction(BattleSlot attacker, BattleSlot defender)
    {
        // Create the ActionDetails object
        ActionDetails actionDetails = new ActionDetails(attacker.Creature.Nickname, defender.Creature.Nickname);
        int accuracy = CalculateAccuracy(attacker.Creature, defender.Creature);
        bool hit = rollToHit(accuracy);
        bool isCrit = false;
        float critMod = 1f;
        int damage = 0;

        // Get the effectiveness category
        Effectiveness effectRating = ElementalEffectivenessChart.GetEffectiveness(
            element, 
            attacker.Creature.IsSingleElement(), 
            defender.Creature.Species.Element1, 
            defender.Creature.Species.Element2
            );

        actionDetails.EffectRating = effectRating;

        // Get the effectiveness modifier based on the category
        float effectMod = ElementalEffectivenessChart.EffectiveMod[effectRating];

        // Check if the attack was a hit or glancing blow
        if (hit)
        {
            isCrit = RollForCrit(attacker.Creature, defender.Creature);
            if (isCrit)
            {
                actionDetails.IsCrit = true;
                critMod = CalculateCritBonus(attacker.Creature, defender.Creature);
            }
            damage = CalculateDamage(attacker.Creature, defender.Creature, critMod);
        }
        else
        {
            actionDetails.IsGlancingBlow = true;
            damage = CalculateDamage(attacker.Creature, defender.Creature);
            damage = Mathf.CeilToInt(damage * attacker.Creature.GlancingDamageReduction);
        }

        // Adjust damage for type effectiveness
        damage = Mathf.Max((int)((float)damage * effectMod), 1);

        // Deal damage to target
        defender.HitByAttack(damage);
        if (defender.IsDefeated)
        {
            actionDetails.IsDefeated = true;
        }
        if (DEBUG)
        {
            Debug.Log($"Effectiveness: {effectMod}");
            Debug.Log($"hit?: {hit}");
            Debug.Log($"isCrit: {isCrit}");
            Debug.Log($"critMod: {critMod}");
            Debug.Log($"damage: {damage}");
        }
        return actionDetails.GetMessages();
    }

    public List<string> UseDefenseAction(BattleSlot attacker, BattleSlot defender)
    {
        // Create the ActionDetails object
        ActionDetails actionDetails = new ActionDetails(attacker.Creature.Nickname, defender.Creature.Nickname);
        actionDetails.IsDefensive = true;
        return actionDetails.GetMessages();
    }

    public List<string> UseHealingAction(BattleSlot attacker, BattleSlot defender)
    {
        // Create the ActionDetails object
        ActionDetails actionDetails = new ActionDetails(attacker.Creature.Nickname, defender.Creature.Nickname);

        bool isCrit = false;
        float critMod = 1f;

        int healing = 0;

        isCrit = RollForCrit(attacker.Creature, defender.Creature);
        if (isCrit)
        {
            actionDetails.IsCrit = true;
            critMod = CalculateCritBonus(attacker.Creature, defender.Creature);
        }
        healing = CalculateHealing(attacker.Creature, power, critMod);

        // Heal damage from target
        defender.HitByHealing(healing);

        if (DEBUG)
        {
            Debug.Log($"isCrit: {isCrit}");
            Debug.Log($"critMod: {critMod}");
            Debug.Log($"healing: {healing}");
        }
        return actionDetails.GetMessages();
    }

    public int CalculateCritChance(Creature attacker, Creature defender, float k = 0.5f)
    {
        int attackerSkill = attacker.Skill;
        int defenderSpeed = defender.Speed;
        if (defenderSpeed == 0) 
        {
            defenderSpeed = 1;
        }
        // Calculate the adjusted crit chance
        float adjustedCritChance = baseCrit * (1 + k * ((attackerSkill - (float)defenderSpeed) / defenderSpeed));

        // Clamp the result to be between half of baseCrit and 100%
        float clampedCritChance = Mathf.Clamp(adjustedCritChance, (baseCrit / 2f), 100f);
        int critPercent = Mathf.FloorToInt(clampedCritChance);
        if (DEBUG)
        {
            Debug.Log($"adjustedCritChance: {adjustedCritChance}");
            Debug.Log($"clampedCritChance: {clampedCritChance}");
            Debug.Log($"critPercent: {critPercent}");
        }
        return critPercent;
    }

    private bool RollForCrit(Creature attacker, Creature defender)
    {
        int critChance = CalculateCritChance(attacker, defender);
        int roll = Random.Range(1, ROLL_CEILING);
        if (DEBUG)
        {
            Debug.Log($"critChance: {critChance}");
            Debug.Log($"Crit Roll: {roll}");
        }
        if (roll <= critChance)
        {
            return true;
        }
        return false;
    }

    private float CalculateCritBonus(Creature attacker, Creature defender)
    {
        float critBonus = 1f;
        critBonus += attacker.CritDamageBonus;
        critBonus -= defender.CritDamageResistance;
        float clampedCrit = Mathf.Clamp(critBonus, 1f, 2f);
        if (DEBUG)
        {
            Debug.Log($"critBonus: {critBonus}");
            Debug.Log($"clampedCrit: {clampedCrit}");
        }
        return clampedCrit;
    }

    private int CalculateEnergyGain(Creature attacker)
    {
        return (int)(attacker.MaxEnergy * (energyGain / 100f));
    }

    public void PayEnergyCost(BattleSlot attacker)
    {
        if (EnergyCost != 0)
        {
            attacker.AdjustEnergy(-EnergyCost);
        }
    }

    public void GenerateEnergy(BattleSlot attacker)
    {
        if (EnergyGain != 0)
        {
            attacker.AdjustEnergy(CalculateEnergyGain(attacker.Creature));
        }
    }

    public bool IsMelee()
    {
        if (Tags.Contains(ActionTag.NoContact))
        {
            return false;
        }
        return range == ActionRange.Melee;
    }

    public bool IsRanged()
    {
        switch (range)
        {
            case ActionRange.ShortRanged:
            case ActionRange.LongRanged:
                return true;
            default:
                return false;
        }
    }

    public bool IsMagic()
    {
        return source == ActionSource.Magical;
    }

    public bool IsPhysical()
    {
        return source == ActionSource.Physical;
    }

    public bool IsElement(CreatureElement checkType)
    {
        return element == checkType;
    }
}

[System.Serializable]
public class LearnableAction
{
    [SerializeField] ActionBase action;
    [SerializeField] int level;

    public ActionBase Action => action;
    public int Level => level;

}