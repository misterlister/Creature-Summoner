using UnityEngine;
using static GameConstants;
public static class CombatCalculator
{
    public static int CalculateAccuracy(ActionBase action, Creature attacker, Creature defender)
    {
        float ratio = ((attacker.Skill - (float)defender.Speed) / defender.Speed);
        float hitChance = action.Accuracy * (1 + ACCURACY_ADJUSTMENT_FACTOR * ratio);
        return (int)Mathf.Clamp(hitChance, MIN_HIT, MAX_HIT);
    }

    public static int CalculateStatusAccuracy(ActionBase action, Creature attacker, Creature defender)
    {
        int defenseStat = action.Source == ActionSource.Magical ? defender.Defense : defender.Resistance;
        int statusResist = defenseStat + defender.Energy;
        float ratio = ((attacker.Skill - (float)statusResist) / statusResist);
        float statusChance = action.Accuracy * (1 + STATUS_RESIST_ADJUSTMENT_FACTOR * ratio);
        return (int)Mathf.Clamp(statusChance, MIN_HIT, MAX_HIT);
    }

    public static int CalculateDamage(ActionBase action, Creature attacker, Creature defender, float critMod = 1.0f, System.Random rng = null)
    {
        float attack = action.Source == ActionSource.Physical ? attacker.Strength : attacker.Magic;
        float defense = action.Source == ActionSource.Physical ? defender.Defense : defender.Resistance;

        if (critMod > 1.0f)
        {
            defense = Mathf.Max(attack, defense * 0.8f);
        }

        float variance = GetDamageVariance(attacker, defender, rng);
        float damage = ((((attacker.Level / 3 + 1) * action.Power * attack / defense) / 50 + 1) * critMod * variance);

        return Mathf.Max((int)damage, 1);
    }

    public static int CalculateHealing(ActionBase action, Creature healer, float critMod = 1.0f, System.Random rng = null)
    {
        float creature_power = action.Source == ActionSource.Physical
            ? healer.Compare_stat_to_average(StatType.Strength)
            : healer.Compare_stat_to_average(StatType.Magic);

        float variance = GetHealVariance(healer, rng);
        float healing = ((((healer.Level / 3 + 1) * action.Power * creature_power) / 50 + 1) * critMod * variance);

        return Mathf.Max((int)healing, 1);
    }

    public static int CalculateCritChance(ActionBase action, Creature attacker, Creature defender)
    {
        int defenderSpeed = Mathf.Max(defender.Speed, 1);
        float adjustedCritChance = action.BaseCrit * (1 + 0.5f * ((attacker.Skill - (float)defenderSpeed) / defenderSpeed));
        return Mathf.FloorToInt(Mathf.Clamp(adjustedCritChance, action.BaseCrit / 2f, 100f));
    }

    public static float CalculateCritBonus(Creature attacker, Creature defender)
    {
        float critBonus = 1f + attacker.CritDamageBonus - defender.CritDamageResistance;
        return Mathf.Clamp(critBonus, 1f, 2f);
    }

    public static int CalculateEnergyGain(ActionBase action, Creature attacker)
    {
        if (action.SlotType != ActionSlotType.Core) return 0;
        return (int)(attacker.MaxEnergy * (action.EnergyValue / 100f));
    }

    private static float GetDamageVariance(Creature attacker, Creature defender, System.Random rng)
    {
        float ratio = (float)attacker.Skill / defender.Speed;
        int varianceRange = (int)(BASE_VARIANCE + ((ratio - 1) * VARIANCE_ADJUSTMENT));
        int clampedVarianceRange = Mathf.Clamp(varianceRange, MIN_VARIANCE, MAX_VARIANCE);

        int variance = rng != null
            ? rng.Next(clampedVarianceRange, ROLL_CEILING)
            : Random.Range(clampedVarianceRange, ROLL_CEILING);

        return variance / 100f;
    }

    private static float GetHealVariance(Creature healer, System.Random rng)
    {
        float ratio = healer.Compare_stat_to_average(StatType.Skill);
        int varianceRange = (int)(BASE_VARIANCE + ((ratio - 1) * VARIANCE_ADJUSTMENT));
        int clampedVarianceRange = Mathf.Clamp(varianceRange, MIN_VARIANCE, MAX_VARIANCE);

        int variance = rng != null
            ? rng.Next(clampedVarianceRange, ROLL_CEILING)
            : Random.Range(clampedVarianceRange, ROLL_CEILING);

        return variance / 100f;
    }

    public static bool RollToHit(int accuracy, System.Random rng = null)
    {
        int roll = rng != null
            ? rng.Next(1, ROLL_CEILING)
            : Random.Range(1, ROLL_CEILING);
        return roll <= accuracy;
    }

    public static bool RollForCrit(int critChance, System.Random rng = null)
    {
        int roll = rng != null
            ? rng.Next(1, ROLL_CEILING)
            : Random.Range(1, ROLL_CEILING);
        return roll <= critChance;
    }
}