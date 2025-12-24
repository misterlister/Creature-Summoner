namespace Game.Battle.Modifiers
{
    public class CombatModifier
    {
        public CombatModifierType Type { get; }
        public int Value { get; }
        public CombatModifierMode Mode { get; }
        public string SourceName { get; }
        public object SourceObject { get; }

        public CombatModifier(CombatModifierType type, int value, CombatModifierMode mode,
                                string sourceName, object source = null)
        {
            Type = type;
            Value = value;
            Mode = mode;
            SourceName = sourceName;
            SourceObject = source;
        }
    }
}