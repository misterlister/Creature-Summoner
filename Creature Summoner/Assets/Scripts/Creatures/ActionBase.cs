using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAction", menuName = "Talents/Create new Action")]
public class ActionBase : Talent
{
    const float HIT_MODIFIER = 0.5f;
    const float MAX_HIT = 1.00f;
    const float MIN_HIT = 0.10f;
    const int MIN_DAMAGE = 1;
    const int BASE_VARIANCE = 85;
    const int VARIANCE_ADJUSTMENT = 10;
    const int MIN_VARIANCE = BASE_VARIANCE - VARIANCE_ADJUSTMENT;
    const int MAX_VARIANCE = BASE_VARIANCE + VARIANCE_ADJUSTMENT;
    const int ROLL_CEILING = 101;

    [SerializeField] CreatureType type;
    [SerializeField] ActionCategory category;
    [SerializeField] ActionSource source;
    [SerializeField] int power = 40;
    [SerializeField] int accuracy = 90;
    [SerializeField] bool ranged;
    [SerializeField] bool offensive = true;
    [SerializeField] bool preparation = false;
    [SerializeField] int numTargets = 1;
    [SerializeField] int baseCrit = 5;
    [SerializeField] List<string> tags;
    [SerializeField] int energy;

    public CreatureType Type => type;
    public ActionCategory Category => category;
    public ActionSource Source => source;
    public int Power => power;
    public int Accuracy => accuracy;
    public bool Ranged => ranged;
    public bool Offensive => offensive;
    public bool Preparation => preparation;
    public int NumTargets => numTargets;
    public int BaseCrit => baseCrit;
    public List<string> Tags => tags;
    public int Energy => energy;

    public int CalculateAccuracy(Creature attacker, Creature defender)
    {
        float hitChance = accuracy * (1 + HIT_MODIFIER * ((attacker.Skill - defender.Speed) / defender.Speed));
        float clampedHitChance = Mathf.Clamp(hitChance, MIN_HIT, MAX_HIT);
        return (int)(clampedHitChance * 100);
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

        float variance = get_variance(attacker, defender);

        float damage = ((((attacker.Level / 3 + 1) * power * attack / defense) / 50 + 1) * critMod * variance);

        return Mathf.Max((int)damage, 1);
    }

    private float get_variance(Creature attacker, Creature defender)
    {
        float ratio = (float)attacker.Skill / defender.Speed;
        int varianceRange = (int)(BASE_VARIANCE + ((ratio - 1) * VARIANCE_ADJUSTMENT));
        int clampedVarianceRange = Mathf.Clamp(varianceRange, MIN_VARIANCE, MAX_VARIANCE);
        int variance = Random.Range(clampedVarianceRange, ROLL_CEILING);
        return variance / 100f;
    }

    private bool roll_to_hit(int accuracy)
    {
        int roll = Random.Range(1, ROLL_CEILING);
        if (roll <= accuracy)
        {
            return true;
        }
        return false;
    }

    private bool check_for_glancing_blow(int glanceChance)
    {
        int roll = Random.Range(1, ROLL_CEILING);
        if (roll <= glanceChance)
        {
            return true;
        }
        return false;
    }

    public void UseAction(BattleCreature attacker, BattleCreature defender)
    {
        int accuracy = CalculateAccuracy(attacker.CreatureInstance, defender.CreatureInstance);
        bool glancing_blow = false;
        // Check if the attack hit
        if (roll_to_hit(accuracy) == false)
        {
            // If it did not hit, check if it was a glancing blow
            glancing_blow = check_for_glancing_blow(defender.CreatureInstance.ChanceToBeGlanced);
            // If it was not glancing, it fully missed, so we return
            if (glancing_blow == false) { return; }
        }
        int damage = CalculateDamage(attacker.CreatureInstance, defender.CreatureInstance);
        if (glancing_blow)
        {
            damage = (int)Mathf.Ceil(damage * attacker.CreatureInstance.GlancingDamageReduction);
        }
        defender.RemoveHP(damage);
    }
}