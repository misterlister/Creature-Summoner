using System.Collections.Generic;
using UnityEngine;

namespace Game.Statuses
{
    public class StatModifierStatus : StatusEffect
    {
        public int Stacks { get; private set; }
        public int TurnsRemaining { get; private set; }
        public StatType Stat { get; private set; }


        public StatModifierStatus(StatusType type, int stacks, bool isCrit, Creature source)
            : base(type, StatusCategory.StatModifier, IsBuffType(type), source)
        {
            if (stacks < 1)
            {
                Debug.LogError($"Attempted to create {type} with {stacks} stacks. Clamping to 1.");
                stacks = 1;
            }

            Stacks = Mathf.Min(stacks, StatusConstants.MaxStacks);
            TurnsRemaining = isCrit ? StatusConstants.MaxDuration : StatusConstants.StatusDuration;
            Stat = StatusRules.GetAffectedStat(type);
        }

        private static bool IsBuffType(StatusType type)
        {
            return  type == StatusType.Empowered || type == StatusType.Enchanted ||
                    type == StatusType.Honed || type == StatusType.Hastened ||
                    type == StatusType.Fortified || type == StatusType.Warded;
        }

        public void AddStacks(int amount, bool isCrit)
        {
            Stacks = Mathf.Min(Stacks + amount, StatusConstants.MaxStacks);
            int newDuration = isCrit ? StatusConstants.MaxDuration : StatusConstants.StatusDuration;
            TurnsRemaining = Mathf.Max(TurnsRemaining, newDuration);
        }

        public float GetMultiplier()
        {
            if (Stacks <= 0 || Stacks > StatusConstants.MaxStacks)
            {
                return 1.0f;
            }

            if (IsBoon)
            {
                return StatusConstants.StatBoonMultiplier[Stacks];
            } 
            else
            {
                return StatusConstants.StatBaneMultiplier[Stacks];
            }
        }

        public override void OnTick(Creature target)
        {
            TurnsRemaining--;
        }

        public override bool ShouldExpire()
        {
            return TurnsRemaining <= 0;
        }

        public override void OnApply(Creature target)
        {
            target.Stats.MarkCurrentStatsDirty();
        }

        public override void OnRemove(Creature target)
        {
            target.Stats.MarkCurrentStatsDirty();
        }

        public override string GetDisplayText()
        {
            return $"{Type} x{Stacks} ({TurnsRemaining}t)";
        }

        public override int GetDisplayValue()
        {
            return Stacks;
        }

        public override StatusEffect Clone()
        {
            return new StatModifierStatus(Type, Stacks, false, Source)
            {
                TurnsRemaining = TurnsRemaining
            };
        }
    }
}