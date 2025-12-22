namespace Game.Battle.Modifiers
{
    public class CombatModifier
    {
        public CombatModifierType CombatModifierType { get; }
        public int Value { get; }
        public string SourceName { get; }
        public object SourceObject { get; }

        public CombatModifier(CombatModifierType combatModifierType, int value, string sourceName, object source = null)
        {
            CombatModifierType = combatModifierType;
            Value = value;
            SourceName = sourceName;
            SourceObject = source;
        }
    }
}