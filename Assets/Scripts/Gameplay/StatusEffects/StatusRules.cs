using System;
using System.Collections.Generic;

namespace Game.Statuses
{
    public static class StatusRules
    {
        private static readonly Dictionary<StatusType, StatusCategory> categoryByType =
            new()
            {
            // Over Time
            { StatusType.Chilled, StatusCategory.OverTime },
            { StatusType.Poisoned, StatusCategory.OverTime },
            { StatusType.Burning, StatusCategory.OverTime },
            { StatusType.Bleeding, StatusCategory.OverTime },

            // Persistent
            { StatusType.Rooted, StatusCategory.Persistent },
            { StatusType.Destabilized, StatusCategory.Persistent },
            { StatusType.Exhausted, StatusCategory.Persistent },
            { StatusType.Withered, StatusCategory.Persistent },

            // Crowd Control
            { StatusType.Blinded, StatusCategory.CrowdControl },
            { StatusType.Confused, StatusCategory.CrowdControl },
            { StatusType.Feared, StatusCategory.CrowdControl },
            { StatusType.Taunted, StatusCategory.CrowdControl },

            // Triggered
            { StatusType.Stunned, StatusCategory.Triggered },
            { StatusType.Exposed, StatusCategory.Triggered },

            // Stat Modifiers
            { StatusType.Empowered, StatusCategory.StatModifier },
            { StatusType.Enchanted, StatusCategory.StatModifier },
            { StatusType.Honed, StatusCategory.StatModifier },
            { StatusType.Hastened, StatusCategory.StatModifier },
            { StatusType.Fortified, StatusCategory.StatModifier },
            { StatusType.Warded, StatusCategory.StatModifier },

            { StatusType.Weakened, StatusCategory.StatModifier },
            { StatusType.Hexed, StatusCategory.StatModifier },
            { StatusType.Hindered, StatusCategory.StatModifier },
            { StatusType.Slowed, StatusCategory.StatModifier },
            { StatusType.Sundered, StatusCategory.StatModifier },
            { StatusType.Cursed, StatusCategory.StatModifier },
            };

        public static StatusCategory GetCategory(StatusType type)
        {
            if (!categoryByType.TryGetValue(type, out var category))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "No StatusCategory defined for this StatusType"
                );
            }

            return category;
        }

        private static readonly Dictionary<StatusType, StatType> statusToStat =
            new()
            {
                { StatusType.Empowered, StatType.Strength },
                { StatusType.Weakened, StatType.Strength },

                { StatusType.Enchanted, StatType.Magic },
                { StatusType.Hexed, StatType.Magic },

                { StatusType.Honed, StatType.Skill },
                { StatusType.Hindered, StatType.Skill },

                { StatusType.Hastened, StatType.Speed },
                { StatusType.Slowed, StatType.Speed },

                { StatusType.Fortified, StatType.Defense },
                { StatusType.Sundered, StatType.Defense },

                { StatusType.Warded, StatType.Resistance },
                { StatusType.Cursed, StatType.Resistance },
            };

        public static StatType GetAffectedStat(StatusType type)
        {
            if (!statusToStat.TryGetValue(type, out var stat))
            {
                throw new ArgumentException(
                    $"Status '{type}' is not a stat modifier.",
                    nameof(type)
                );
            }

            return stat;
        }
    }
}
