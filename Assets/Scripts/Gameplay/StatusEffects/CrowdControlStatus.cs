using UnityEngine;
namespace Game.Statuses
{
    public class CrowdControlStatus : StatusEffect
    {
        public int Stacks { get; private set; }

        public CrowdControlStatus(StatusType type, int stacks, bool isCrit, Creature source)
            : base(type, StatusCategory.CrowdControl, false, source)
        {
            Stacks = isCrit ? stacks + 1 : stacks;
            Stacks = Mathf.Min(Stacks, StatusConstants.MaxStacks);
        }

        public void AddStacks(int amount, bool isCrit)
        {
            int toAdd = isCrit ? amount + 1 : amount;
            Stacks = Mathf.Min(Stacks + toAdd, StatusConstants.MaxStacks);
        }

        public override void OnTick(Creature target)
        {
            Stacks--;
        }

        public override bool ShouldExpire()
        {
            return Stacks <= 0;
        }

        public override void OnApply(Creature target)
        {
            /*
            switch (Type)
            {
                case StatusType.Blind:
                    target.DamageMultiplier *= 0.25f;
                    break;
                case StatusType.Confusion:
                    target.IsConfused = true;
                    break;
                case StatusType.Fear:
                    target.AddFearSource(Source);
                    break;
                case StatusType.Taunt:
                    target.SetTauntTarget(Source);
                    break;
            }
            */
        }

        public override void OnRemove(Creature target)
        {
            /*
            switch (Type)
            {
                case StatusType.Blind:
                    target.RecalculateDamageMultiplier();
                    break;
                case StatusType.Confusion:
                    target.IsConfused = false;
                    break;
                case StatusType.Fear:
                    target.RemoveFearSource(Source);
                    break;
                case StatusType.Taunt:
                    target.ClearTauntTarget(Source);
                    break;
            }
            */
        }

        public override string GetDisplayText()
        {
            return $"{Type} x{Stacks}";
        }

        public override int GetDisplayValue()
        {
            return Stacks;
        }

        public override StatusEffect Clone()
        {
            return new CrowdControlStatus(Type, Stacks, false, Source);
        }
    }
}