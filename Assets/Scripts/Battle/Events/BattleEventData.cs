
public class BattleEventData
{
    public Creature SourceCreature { get; private set; }
    public BattleContext BattleContext { get; private set; }

    public BattleEventData(Creature sourceCreature, BattleContext battleContext)
    {
        SourceCreature = sourceCreature;
        BattleContext = battleContext;
    }
}
