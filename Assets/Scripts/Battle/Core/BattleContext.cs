using System.Collections.Generic;
using System.Linq;

public class BattleContext
{
    // Core references
    public UnifiedBattlefield Battlefield { get; set; }
    public BattleManager BattleManager { get; set; }
    public BattleEventManager EventManager { get; set; }

    // Environmental
    public Biome CurrentBiome { get; set; }

    // IMPLEMENT LATER
    // public WeatherEffect ActiveWeather { get; set; }

    // Turn tracking
    public int TurnNumber { get; set; }
    public Creature CurrentActingCreature { get; set; }

    // RNG (for determinism/replays)
    public System.Random RNG { get; set; }

    // Global modifiers
    public float GlobalDamageMultiplier { get; set; } = 1f;
    public float GlobalHealingMultiplier { get; set; } = 1f;

    public BattleContext(UnifiedBattlefield battlefield, BattleManager manager)
    {
        Battlefield = battlefield;
        BattleManager = manager;
        EventManager = new BattleEventManager();
        RNG = new System.Random();
    }

    // Helper queries
    public List<Creature> GetAllCreatures() => Battlefield.GetAllCreatures();

    public List<Creature> GetAlliesOf(Creature creature)
    {
        return Battlefield.GetGrid(creature.TeamSide).GetAllCreatures()
            .Where(c => c != creature).ToList();
    }

    public List<Creature> GetEnemiesOf(Creature creature)
    {
        var oppositeTeam = creature.TeamSide == TeamSide.Player ? TeamSide.Enemy : TeamSide.Player;
        return Battlefield.GetGrid(oppositeTeam).GetAllCreatures();
    }

    public List<Creature> GetAlliesOfElement(Creature creature, CreatureElement element)
    {
        return GetAlliesOf(creature).Where(c => c.IsElement(element)).ToList();
    }
}