using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionExecutor
{
    private BattleContext context;

    public ActionExecutor(BattleContext context)
    {
        this.context = context;
    }

    public ActionResult Execute(ActionBase action, Creature attacker, List<BattleTile> targets)
    {
        // Create action event data
        var actionEventData = new ActionEventData(attacker, context)
        {
            Action = action,
            AOETargetCreatures = targets.Where(t => t.IsOccupied)
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

    private ActionResult ExecuteOffensive(ActionBase action, Creature attacker, List<BattleTile> targets)
    {
        var result = new ActionResult(action, attacker);

        foreach (var targetTile in targets)
        {
            if (!targetTile.IsOccupied) continue;

            var defender = targetTile.OccupyingCreature;
            var targetResult = new TargetResult(defender);

            // Calculate accuracy
            int accuracy = CombatCalculator.CalculateAccuracy(action, attacker, defender);
            bool fullHit = CombatCalculator.RollToHit(accuracy, context.RNG);

            // Determine hit type
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

            // Effectiveness
            Effectiveness effectRating = ElementalEffectivenessChart.GetEffectiveness(
                action.Element,
                attacker.IsSingleElement(),
                defender.Species.Element1,
                defender.Species.Element2
            );
            float effectMod = ElementalEffectivenessChart.EffectiveMod[effectRating];
            targetResult.EffectRating = effectRating;

            // Calculate damage
            int damage = CombatCalculator.CalculateDamage(action, attacker, defender, critMod, context.RNG);

            // Apply glancing reduction if needed
            if (hitType == HitType.Glance)
            {
                damage = Mathf.CeilToInt(damage * attacker.GlancingDamageReduction);
            }

            // Apply effectiveness
            damage = Mathf.Max((int)(damage * effectMod), 1);

            // Apply cover bonus (for ranged attacks)
            if (action.IsRanged())
            {
                float coverBonus = CombatUtilities.GetTotalCoverBonus(defender, context.Battlefield);
                damage = Mathf.RoundToInt(damage * (1 - coverBonus));
            }

            // Create damage event
            var damageEvent = new DamageEventData(defender, context)
            {
                Attacker = attacker,
                ActionUsed = action,
                HitType = hitType,
                DamageAmount = damage,
                DamageMultiplier = 1f
            };

            // Trigger before damage event (allows abilities to modify)
            context.EventManager.TriggerBeforeDamage(damageEvent);

            if (!damageEvent.PreventDamage)
            {
                int finalDamage = Mathf.RoundToInt(damageEvent.DamageAmount * damageEvent.DamageMultiplier);
                defender.TakeDamage(finalDamage);
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

        return result;
    }

    private ActionResult ExecuteHealing(ActionBase action, Creature attacker, List<BattleTile> targets)
    {
        var result = new ActionResult(action, attacker);

        foreach (var targetTile in targets)
        {
            if (!targetTile.IsOccupied) continue;

            var target = targetTile.OccupyingCreature;
            var targetResult = new TargetResult(target);

            // Calculate crit
            int critChance = CombatCalculator.CalculateCritChance(action, attacker, target);
            bool isCrit = CombatCalculator.RollForCrit(critChance, context.RNG);
            float critMod = 1f;

            if (isCrit)
            {
                targetResult.HitType = HitType.Critical;
                critMod = CombatCalculator.CalculateCritBonus(attacker, target);
            }

            // Calculate healing
            int healing = CombatCalculator.CalculateHealing(action, attacker, critMod, context.RNG);

            // Apply healing
            target.Heal(healing);
            targetResult.HealingDone = healing;

            result.AddTargetResult(targetResult);
        }

        return result;
    }

    private ActionResult ExecuteDefensive(ActionBase action, Creature attacker, List<BattleTile> targets)
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