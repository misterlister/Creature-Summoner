namespace Game.Statuses
{
    public class OverTimeStatus : StatusEffect
    {
        public int[] Schedule { get; private set; }

        public OverTimeStatus(StatusType type, int amount, bool isCrit, Creature source)
            : base(type, StatusCategory.OverTime, type == StatusType.Energized || type == StatusType.Regenerating, source)
        {
            Schedule = new int[StatusConstants.MaxDuration];

            int duration = isCrit ? StatusConstants.MaxDuration : StatusConstants.StatusDuration;

            for (int i = 0; i < Schedule.Length; i++)
            {
                if (i < duration)
                {
                    Schedule[i] = amount;
                }
                else
                {
                    Schedule[i] = 0;
                }
            }
        }

        public void AddToSchedule(int amount, bool isCrit)
        {
            int duration = isCrit ? StatusConstants.MaxDuration : StatusConstants.StatusDuration;

            for (int i = 0; i < Schedule.Length && i < duration; i++)
            {
                if (Schedule[i] > 0)
                {
                    Schedule[i] += amount;
                }
            }
        }

        public override void OnTick(Creature target)
        {
            int value = Schedule[0];

            // Double damage if burning creature moved or bleeding creature was hit
            if ((Type == StatusType.Burning && !target.MovedThisTurn)
                || Type == StatusType.Bleeding && target.WasHitThisTurn)
            {
                value *= 2;
            }

            switch (Type)
            {
                case StatusType.Poisoned:
                case StatusType.Bleeding:
                case StatusType.Burning:
                    target.TakeDamage(value);
                    break;
                case StatusType.Energized:
                    target.RestoreEnergy(value);
                    break;
                case StatusType.Regenerating:
                    target.RestoreHealth(value);
                    break;
            }

            // Shift schedule
            for (int i = 0; i < Schedule.Length - 1; i++)
            {
                Schedule[i] = Schedule[i + 1];
            }
            Schedule[Schedule.Length - 1] = 0;
        }

        public override bool ShouldExpire()
        {
            foreach (int tick in Schedule)
            {
                if (tick > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public override StatusEffect Clone()
        {
            OverTimeStatus clone = new OverTimeStatus(Type, 0, false, Source);
            Schedule.CopyTo(clone.Schedule, 0);
            return clone;
        }

        public override int GetDisplayValue()
        {
            return Schedule[0];
        }

        public override string GetDisplayText()
        {
            string valString = "";
            foreach (int tick in Schedule)
            {
                if (tick > 0)
                {
                    valString += tick.ToString() + "/";
                }
            }
            return $"{Type.ToString()}: {valString}";
        }

        public override void OnApply(Creature target)
        {
            // Nothing for now
        }

        public override void OnRemove(Creature target)
        {
            // Nothing for now
        }
    }
}