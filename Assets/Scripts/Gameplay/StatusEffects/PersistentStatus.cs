using UnityEngine;

namespace Game.Statuses
{
    public class PersistentStatus : StatusEffect
    {
        public int TurnsRemaining { get; private set; }

        public PersistentStatus(StatusType type, bool isCrit, Creature source)
            : base(type, StatusCategory.Persistent, false, source)
        {
            TurnsRemaining = isCrit ? StatusConstants.MaxDuration : StatusConstants.StatusDuration;
        }

        public void RefreshDuration(bool isCrit)
        {
            int duration = isCrit ? StatusConstants.MaxDuration : StatusConstants.StatusDuration;
            TurnsRemaining = Mathf.Max(TurnsRemaining, duration);
        }

        public override void OnTick(Creature target)
        {
            TurnsRemaining--;
            if (TurnsRemaining <= 0)
            {
                //target.RemoveStatus(this);
            }
        }

        public override bool ShouldExpire()
        {
            return TurnsRemaining <= 0;
        }

        public override string GetDisplayText()
        {
            return $"{Type} ({TurnsRemaining} turns left)";
        }

        public override int GetDisplayValue()
        {
            return TurnsRemaining;
        }

        public override void OnApply(Creature target)
        {
            // Implementation for when the status is applied
        }

        public override void OnRemove(Creature target)
        {
            // Implementation for when the status is removed
        }

        public override StatusEffect Clone()
        {
            return new PersistentStatus(Type, false, Source)
            {
                TurnsRemaining = TurnsRemaining
            };
        }
    }
}
