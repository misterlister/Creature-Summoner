namespace Game.Battle.Modifiers
{
    public class BattleRuleModifier
    {
        public BattleRuleType Type { get; }
        public string SourceName { get; }
        public object SourceObject { get; }

        public BattleRuleModifier(BattleRuleType type, string sourceName, object source = null)
        {
            Type = type;
            SourceName = sourceName;
            SourceObject = source;
        }
    }
}