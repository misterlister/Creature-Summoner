using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Battle.Modifiers;

namespace Game.Creatures.Managers
{
    public class StatManager
    {
        private Creature owner;

        // Base Stats - only recalculate when leveling or changing class
        private Dictionary<StatType, int> baseStats;
        private bool baseStatsDirty = true;

        // Current Stats - recalculate when buffs/debuffs change
        private Dictionary<StatType, int> currentStats;
        private bool currentStatsDirty = true;

        public event Action OnStatsChanged;

        public StatManager(Creature owner)
        {
            this.owner = owner;
            baseStats = new Dictionary<StatType, int>();
            currentStats = new Dictionary<StatType, int>();
        }

        public int GetBaseStat(StatType statType)
        {
            if (baseStatsDirty)
            {
                RecalculateBaseStats();
            }
            return baseStats.GetValueOrDefault(statType, 0);
        }

        public int GetCurrentStat(StatType statType)
        {
            if (currentStatsDirty)
            {
                RecalculateCurrentStats();
            }
            return currentStats.GetValueOrDefault(statType, 0);
        }

        public int GetStatModifier(StatType statType)
        {
            return GetCurrentStat(statType) - GetBaseStat(statType);
        }

        public void MarkBaseStatsDirty()
        {
            baseStatsDirty = true;
            currentStatsDirty = true;
            OnStatsChanged?.Invoke();
        }

        public void MarkCurrentStatsDirty()
        {
            currentStatsDirty = true;
            OnStatsChanged?.Invoke();
        }

        private void RecalculateBaseStats()
        {
            baseStats.Clear();

            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                float speciesBase = GetSpeciesBaseStat(statType);
                float scaled = ApplyLevelScaling(speciesBase, statType, owner.Level);
                int classBonus = GetClassModifier(statType);

                int finalStat = Mathf.RoundToInt(Mathf.Max(GameConstants.MIN_STAT_VALUE, scaled + classBonus));
                baseStats[statType] = finalStat;
            }

            baseStatsDirty = false;
        }

        private void RecalculateCurrentStats(BattleContext context = null)
        {
            currentStats.Clear();

            foreach (var kvp in baseStats)
            {
                currentStats[kvp.Key] = kvp.Value;
            }

            if (context != null)
            {
                var statModifiers = new List<StatModifier>();

                owner.GetAllStatModifiers(context, statModifiers);

                foreach (StatType statType in Enum.GetValues(typeof(StatType)))
                {
                    int baseStat = baseStats.GetValueOrDefault(statType, 0);
                    int modifiedStat = ApplyStatModifiers(baseStat, statType, statModifiers);
                    currentStats[statType] = modifiedStat;
                }
            }

            currentStatsDirty = false;
        }

        private int ApplyStatModifiers(int baseStat, StatType statType, List<StatModifier> modifiers)
        {
            // Max HP and Energy are not modified by temporary modifiers
            if (statType == StatType.HP || statType == StatType.Energy)
            {
                return baseStat;
            }

            int totalFlat = 0;
            float totalPercentBase = 0f;
            float totalPercentTotal = 1f;

            // Collect modifiers for this stat type
            foreach (var mod in modifiers)
            {
                if (mod.StatType != statType) continue;

                switch (mod.Mode)
                {
                    case ModifierMode.Flat:
                        totalFlat += mod.Value;
                        break;
                    case ModifierMode.PercentBase:
                        totalPercentBase += mod.Value / 100f;
                        break;
                    case ModifierMode.PercentTotal:
                        totalPercentTotal *= (1f + mod.Value / 100f);
                        break;
                }
            }

            // Apply in order: Base -> Flat -> PercentBase -> PercentTotal
            float result = baseStat;
            result += totalFlat;
            result *= (1f + totalPercentBase);
            result *= totalPercentTotal;

            return Mathf.RoundToInt(Mathf.Max(GameConstants.MIN_STAT_VALUE, result));
        }

        private float GetSpeciesBaseStat(StatType statType)
        {
            return statType switch
            {
                StatType.HP => owner.Species.HP,
                StatType.Energy => owner.Species.Energy,
                StatType.Strength => owner.Species.Strength,
                StatType.Magic => owner.Species.Magic,
                StatType.Skill => owner.Species.Skill,
                StatType.Speed => owner.Species.Speed,
                StatType.Defense => owner.Species.Defense,
                StatType.Resistance => owner.Species.Resistance,
                _ => 0f,
            };
        }

        private float ApplyLevelScaling(float baseStat, StatType statType, int level)
        {
            if (statType == StatType.HP || statType == StatType.Energy)
            {
                return Mathf.FloorToInt(((baseStat * 4) * level) / 100f) + 10 + level;
            }
            return Mathf.FloorToInt(((baseStat * 4) * level) / 100f) + 5;
        }

        private int GetClassModifier(StatType statType)
        {
            if (owner.CurrentClass == null) return 0;
            return owner.CurrentClass.GetStatModifier(statType);
        }
    }
}