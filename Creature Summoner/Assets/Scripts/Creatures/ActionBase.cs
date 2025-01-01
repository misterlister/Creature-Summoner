using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static GameConstants;
using static UnityEditor.UIElements.ToolbarMenu;

[CreateAssetMenu(fileName = "NewAction", menuName = "Talents/Create new Action")]
public class ActionBase : Talent
{
    //
    bool DEBUG = false;
    //

    [SerializeField] CreatureType type;
    [SerializeField] ActionCategory category;
    [SerializeField] ActionSource source;
    [SerializeField] ActionRange range;
    [SerializeField] int energyCost = 0;
    [SerializeField] int energyGain = 0;
    [SerializeField] int power = 40;
    [SerializeField] int accuracy = 90;
    [SerializeField] int baseCrit = 5;
    [SerializeField] AOE areaOfEffect = AOE.Single;
    [SerializeField] bool offensive = true;
    [SerializeField] bool preparation = false;
    [SerializeField] List<string> tags;

    public CreatureType Type => type;
    public ActionCategory Category => category;
    public ActionSource Source => source;
    public int Power => power;
    public int Accuracy => accuracy;
    public ActionRange Range => range;
    public bool Offensive => offensive;
    public bool Preparation => preparation;
    public AOE AreaOfEffect => areaOfEffect;
    public int BaseCrit => baseCrit;
    public List<string> Tags => tags;
    public int EnergyCost => energyCost;
    public int EnergyGain => energyGain;

    public int CalculateAccuracy(Creature attacker, Creature defender)
    {
        float ratio = ((attacker.Skill - (float)defender.Speed) / defender.Speed);
        float hitChance = accuracy * (1 + HIT_MODIFIER * ratio);
        float clampedHitChance = Mathf.Clamp(hitChance, MIN_HIT, MAX_HIT);
        if (DEBUG)
        {
            Debug.Log($"Hit ratio: {ratio}");
            Debug.Log($"Raw hit Chance: {hitChance}");
            Debug.Log($"Clamped hit Chance: {clampedHitChance}");
        }
        return (int)(clampedHitChance);
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
            creature_power = healer.compare_stat_to_average(Stat.Strength);
        }

        if (source == ActionSource.Magical)
        {
            creature_power = healer.compare_stat_to_average(Stat.Magic);
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
        float ratio = healer.compare_stat_to_average(Stat.Skill);
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

        if (source == ActionSource.Defensive)
        {
            return UseDefensiveAction(attacker, defender);
        }

        if (offensive)
        {
            return UseOffensiveAction(attacker, defender);
        }

        else
        {
            return UseHealingAction(attacker, defender);
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
        Effectiveness effectRating = TypeChart.GetEffectiveness(type, defender.Creature.Species.Type1, defender.Creature.Species.Type2);

        actionDetails.EffectRating = effectRating;

        // Get the effectiveness modifier based on the category
        float effectMod = TypeChart.EffectiveMod[effectRating];

        // Check if the attack was a hit or glancing blow
        if (hit)
        {
            isCrit = RollForCrit(attacker, defender);
            if (isCrit)
            {
                actionDetails.IsCrit = true;
                critMod = CalculateCritBonus(attacker, defender);
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
            Debug.Log($"hit?: {hit}");
            Debug.Log($"isCrit: {isCrit}");
            Debug.Log($"critMod: {critMod}");
            Debug.Log($"damage: {damage}");
        }
        return actionDetails.GetMessages();
    }

    public List<string> UseDefensiveAction(BattleSlot attacker, BattleSlot defender)
    {
        // Create the ActionDetails object
        ActionDetails actionDetails = new ActionDetails(attacker.Creature.Nickname, defender.Creature.Nickname);
        int accuracy = CalculateAccuracy(attacker.Creature, defender.Creature);
        bool hit = rollToHit(accuracy);
        bool glancingBlow = false;
        bool isCrit = false;
        float critMod = 1f;

        // Get the effectiveness category
        Effectiveness effectRating = TypeChart.GetEffectiveness(type, defender.Creature.Species.Type1, defender.Creature.Species.Type2);

        actionDetails.EffectRating = effectRating;

        // Get the effectiveness modifier based on the category
        float effectMod = TypeChart.EffectiveMod[effectRating];

        int damage = 0;
        if (glancingBlow)
        {
            actionDetails.IsGlancingBlow = true;
            damage = CalculateDamage(attacker.Creature, defender.Creature);
            damage = Mathf.CeilToInt(damage * attacker.Creature.GlancingDamageReduction);
        }
        else
        {
            isCrit = RollForCrit(attacker, defender);
            if (isCrit)
            {
                actionDetails.IsCrit = true;
                critMod = CalculateCritBonus(attacker, defender);
            }
            damage = CalculateDamage(attacker.Creature, defender.Creature, critMod);
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
            Debug.Log($"glancingBlow: {glancingBlow}");
            Debug.Log($"isCrit: {isCrit}");
            Debug.Log($"critMod: {critMod}");
            Debug.Log($"damage: {damage}");
        }
        return actionDetails.GetMessages();
    }

    public List<string> UseHealingAction(BattleSlot attacker, BattleSlot defender)
    {
        // Create the ActionDetails object
        ActionDetails actionDetails = new ActionDetails(attacker.Creature.Nickname, defender.Creature.Nickname);

        bool isCrit = false;
        float critMod = 1f;

        int healing = 0;

        isCrit = RollForCrit(attacker, defender);
        if (isCrit)
        {
            actionDetails.IsCrit = true;
            critMod = CalculateCritBonus(attacker, defender);
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

    public int CalculateCritChance(BattleSlot attacker, BattleSlot defender, float k = 0.5f)
    {
        int attackerSkill = attacker.Creature.Skill;
        int defenderSpeed = defender.Creature.Speed;
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

    private bool RollForCrit(BattleSlot attacker, BattleSlot defender)
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

    private float CalculateCritBonus(BattleSlot attacker, BattleSlot defender)
    {
        float critBonus = 1f;
        critBonus += attacker.Creature.CritBonus;
        critBonus -= defender.Creature.CritResistance;
        float clampedCrit = Mathf.Clamp(critBonus, 1f, 2f);
        if (DEBUG)
        {
            Debug.Log($"critBonus: {critBonus}");
            Debug.Log($"clampedCrit: {clampedCrit}");
        }
        return clampedCrit;
    }

    private int CalculateEnergyGain(BattleSlot attacker)
    {
        return (int)(attacker.Creature.MaxEnergy * (energyGain / 100f));
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
            attacker.AdjustEnergy(CalculateEnergyGain(attacker));
        }
    }
}