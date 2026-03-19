using System.Linq;
using UnityEngine;
using static BattleSystemConstants;

public class ActionExecutor
{
    private BattleContext context;

    public ActionExecutor(BattleContext context)
    {
        this.context = context;
    }

    public ActionResult Execute(ActionBase action, Creature attacker, TargetList targets)
    {
        // Create action event data
        var actionEventData = new ActionEventData(attacker, context)
        {
            Action = action,
            AOETargetCreatures = targets.AllTargets()
            .Where(t => t != null && t.IsOccupied)
            .Select(t => t.OccupyingCreature).ToList()
        };

        // Trigger before action event
        context.EventManager.TriggerBeforeAction(actionEventData);

        // Pay energy cost
        PayEnergyCost(action, attacker);

        // Execute based on role
        ActionResult result = action.Role switch
        {
            ActionRole.Offensive => ExecuteOffensive(action, attacker, targets),
            _ when action.Tags.Contains(ActionTag.Healing) => ExecuteHealing(action, attacker, targets),
            _ => ExecuteDefensive(action, attacker, targets)
        };

        // Generate energy for core actions
        GenerateEnergy(action, attacker);

        // Trigger after action event
        context.EventManager.TriggerAfterAction(actionEventData);

        return result;
    }

    private ActionResult ExecuteOffensive(ActionBase action, Creature attacker, TargetList targets)
    {
        var result = new ActionResult(action, attacker);

        if (targets.PrimaryTarget?.IsOccupied == true)
            ApplyOffensiveToTarget(action, attacker, targets.PrimaryTarget.OccupyingCreature, result, 1f);

        foreach (var tile in targets.SecondaryTargets)
            if (tile?.IsOccupied == true)
                ApplyOffensiveToTarget(action, attacker, tile.OccupyingCreature, result, SECONDARY_DAMAGE_MULTIPLIER);

        foreach (var tile in targets.TertiaryTargets)
            if (tile?.IsOccupied == true)
                ApplyOffensiveToTarget(action, attacker, tile.OccupyingCreature, result, TERTIARY_DAMAGE_MULTIPLIER);

        return result;
    }

    private void ApplyOffensiveToTarget(ActionBase action, Creature attacker, Creature defender, ActionResult result, float damageMultiplier = 1f)
    {
        var targetResult = new TargetResult(defender);

        int accuracy = CombatCalculator.CalculateAccuracy(action, attacker, defender);
        bool fullHit = CombatCalculator.RollToHit(accuracy, context.RNG);

        float critMod = 1f;
        HitType hitType;

        if (fullHit)
        {
            int critChance = CombatCalculator.CalculateCritChance(action, attacker, defender);
            bool isCrit = CombatCalculator.RollForCrit(critChance, context.RNG);
            if (isCrit)
            {
                hitType = HitType.Critical;
                critMod = CombatCalculator.CalculateCritBonus(attacker, defender);
            }
            else
            {
                hitType = HitType.Hit;
            }
        }
        else
        {
            hitType = HitType.Glance;
        }

        targetResult.HitType = hitType;

        Effectiveness effectRating = ElementalEffectivenessChart.GetEffectiveness(
            action.Element,
            attacker.IsSingleElement(),
            defender.Species.Element1,
            defender.Species.Element2);

        float effectMod = ElementalEffectivenessChart.EffectiveMod[effectRating];
        targetResult.EffectRating = effectRating;

        int damage = CombatCalculator.CalculateDamage(action, attacker, defender, critMod, context.RNG);

        if (hitType == HitType.Glance)
            damage = Mathf.CeilToInt(damage * attacker.GlancingDamageReduction);

        damage = Mathf.Max((int)(damage * effectMod * damageMultiplier), 1);

        if (action.IsRanged())
        {
            float coverBonus = CombatUtilities.GetTotalCoverBonus(defender, context.Battlefield);
            damage = Mathf.RoundToInt(damage * (1 - coverBonus));
        }

        var damageEvent = new DamageEventData(defender, context)
        {
            Attacker = attacker,
            ActionUsed = action,
            HitType = hitType,
            DamageAmount = damage,
            DamageMultiplier = 1f
        };

        context.EventManager.TriggerBeforeDamage(damageEvent);

        if (!damageEvent.PreventDamage)
        {
            int finalDamage = Mathf.RoundToInt(damageEvent.DamageAmount * damageEvent.DamageMultiplier);
            defender.TakeDamage(finalDamage);
            Debug.Log($"{attacker.Nickname} dealt {finalDamage} damage to {defender.Nickname} with {action.ActionName} (HitType: {hitType}, Effectiveness: {effectRating})");
            Debug.Log($"{defender.Nickname} has {defender.HP}/{defender.MaxHP} HP remaining.");
            targetResult.DamageDealt = finalDamage;
            targetResult.IsDefeated = defender.IsDefeated;
            context.EventManager.TriggerAfterDamage(damageEvent);

            if (defender.IsDefeated)
            {
                var defeatEvent = new CreatureDefeatEventData(defender, context)
                {
                    VictoriousCreature = attacker
                };
                context.EventManager.TriggerCreatureDefeated(defeatEvent);
            }
        }

        result.AddTargetResult(targetResult);
    }

    private ActionResult ExecuteHealing(ActionBase action, Creature healer, TargetList targets)
    {
        var result = new ActionResult(action, healer);

        if (targets.PrimaryTarget?.IsOccupied == true)
            ApplyHealingToTarget(action, healer, targets.PrimaryTarget.OccupyingCreature, result, 1f);

        foreach (var tile in targets.SecondaryTargets)
            if (tile?.IsOccupied == true)
                ApplyHealingToTarget(action, healer, tile.OccupyingCreature, result, SECONDARY_DAMAGE_MULTIPLIER);

        foreach (var tile in targets.TertiaryTargets)
            if (tile?.IsOccupied == true)
                ApplyHealingToTarget(action, healer, tile.OccupyingCreature, result, TERTIARY_DAMAGE_MULTIPLIER);

        return result;
    }

    private void ApplyHealingToTarget(ActionBase action, Creature healer, Creature target, ActionResult result, float healingMultiplier = 1f)
    {
        var targetResult = new TargetResult(target);

        // Calculate crit
        int critChance = CombatCalculator.CalculateCritChance(action, healer, target);
        bool isCrit = CombatCalculator.RollForCrit(critChance, context.RNG);
        float critMod = 1f;

        if (isCrit)
        {
            targetResult.HitType = HitType.Critical;
            critMod = CombatCalculator.CalculateCritBonus(healer, target);
        }

        // Calculate healing
        int healing = Mathf.Max((int)(CombatCalculator.CalculateHealing(action, healer, critMod, context.RNG) * healingMultiplier));

        // Apply healing
        target.Heal(healing);
        targetResult.HealingDone = healing;

        result.AddTargetResult(targetResult);
    }

    private ActionResult ExecuteDefensive(ActionBase action, Creature attacker, TargetList targets)
    {
        var result = new ActionResult(action, attacker);
        // TODO: Implement defensive action logic (buffs, debuffs, etc.)
        return result;
    }

    private void PayEnergyCost(ActionBase action, Creature attacker)
    {
        if (action.SlotType != ActionSlotType.Empowered) return;
        if (action.EnergyValue > 0)
        {
            attacker.RemoveEnergy(action.EnergyValue);
        }
    }

    private void GenerateEnergy(ActionBase action, Creature attacker)
    {
        if (action.SlotType != ActionSlotType.Core) return;
        int energyGain = CombatCalculator.CalculateEnergyGain(action, attacker);
        if (energyGain > 0)
        {
            attacker.AddEnergy(energyGain);
        }
    }
}