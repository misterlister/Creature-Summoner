using UnityEngine;

namespace Game.Statuses
{
    public abstract class StatusEffect
    {
        public StatusType Type { get; protected set; }
        public StatusCategory Category { get; protected set; }
        public bool IsBoon { get; protected set; }

        public string InstanceId { get; private set; }
        public Creature Source { get; protected set; }

        public StatusEffect(StatusType type, StatusCategory category, bool isBoon, Creature source)
        {
            Type = type;
            Category = category;
            IsBoon = isBoon;
            Source = source;
            InstanceId = System.Guid.NewGuid().ToString();
        }

        // Override in derived classes
        public abstract void OnApply(Creature target);
        public abstract void OnTick(Creature target); // Called each turn
        public abstract void OnRemove(Creature target);
        public abstract bool ShouldExpire(); // Check if should be removed
        public abstract StatusEffect Clone(); // For copying

        // UI/Display
        public abstract string GetDisplayText();
        public abstract int GetDisplayValue(); // Stacks, turns, etc.
    }
}