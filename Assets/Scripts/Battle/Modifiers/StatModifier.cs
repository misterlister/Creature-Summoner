namespace Game.Battle.Modifiers
{ 
    public class StatModifier
    {
        public StatType StatType { get; }
        public int Value { get; }
        public ModifierMode Mode { get; }
        public string SourceName { get; }
        public object SourceObject { get; }

        public StatModifier(StatType statType, int value, ModifierMode mode,
                            string sourceName, object source = null)
        {
            StatType = statType;
            Value = value;
            Mode = mode;
            SourceName = sourceName;
            SourceObject = source;
        }
    }
}