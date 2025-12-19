namespace Game.Statuses
{
    public static class StatusConstants
    {
        // Maximum stacks for stackable statuses
        public const int MaxStacks = 3;
        // Default turn duration for statuses
        public const int StatusDuration = 3;
        // Maximum duration for any status
        public const int MaxDuration = 5;
        // Reduction value for Exhaust and Withered
        public const float ResourceReductionVal = 0.5f;
        // Increase values for Stat Buffs
        public static readonly float[] StatBoonMultiplier =
        {
            1.0f,   // 0 stacks
            1.25f,  // 1 stack
            1.4f,   // 2 stacks
            1.5f    // 3 stacks
        };

        public static readonly float[] StatBaneMultiplier =
        {
            1.0f,       // 0 stacks
            1f / 1.25f, // 1 stack (0.8)
            1f / 1.4f,  // 2 stacks (~0.7142857)
            1f / 1.5f   // 3 stacks (~0.6666667)
        };

        public static readonly float[] ExposedDamageMultiplier =
        {
            1.0f,   // 0 stacks
            1.25f,  // 1 stack
            1.4f,   // 2 stacks
            1.5f    // 3 stacks
        };
    }
}