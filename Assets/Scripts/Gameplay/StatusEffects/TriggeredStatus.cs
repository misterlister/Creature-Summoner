using UnityEngine;

namespace Game.Statuses
{
    public class TriggeredStatus :StatusEffect
    {
        public int Stacks { get; private set; }
        public TriggeredStatus(StatusType type, int stacks, bool isCrit, Creature source) 
            : base(type, StatusCategory.Triggered, false, source)
        {
            if (stacks < 1)
            {
                Debug.LogError($"Attempted to create {type} with {stacks} stacks. Clamping to 1.");
                stacks = 1;
            }

            Stacks = isCrit ? stacks + 1 : stacks;
            Stacks = Mathf.Min(Stacks, StatusConstants.MaxStacks);
        }

        public void AddStacks(int amount, bool isCrit)
        {
            int additionalStacks = isCrit ? amount + 1 : amount;
            Stacks = Mathf.Min(Stacks + additionalStacks, StatusConstants.MaxStacks);
        }

        public override void OnApply(Creature target)
        {
            // Implementation for when the status is applied
        }

        public override void OnRemove(Creature target)
        {
            // Implementation for when the status is removed
        }

        public override void OnTick(Creature target)
        {
            // Stun gets consumed at turn start
            if (Type == StatusType.Stunned && target.IsTurnActive())
            {
                ApplyStunEffect(target);
                Stacks = 0;
            }
        }

        private void ApplyStunEffect(Creature target)
        {
            /*
            switch (Stacks)
            {
                case 1:
                    target.MoveLast = true;
                    break;
                case 2:
                    target.CanMove = false;
                    target.CanUseEmpoweredActions = false;
                    break;
                case 3:
                    target.SkipTurn = true;
                    break;
            }
            */
        }

        public override bool ShouldExpire()
        {
            return Stacks <= 0;
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
            return new TriggeredStatus(Type, Stacks, false, Source);
        }
    }

}