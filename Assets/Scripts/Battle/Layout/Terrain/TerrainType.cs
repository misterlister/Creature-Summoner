using UnityEngine;
using static TerrainConstants;

/// <summary>
/// Base class for all terrain types.
/// Each specific terrain is a concrete implementation with fixed mechanics.
/// Visual representation is handled by BiomeTerrainSet.
/// </summary>
public abstract class TerrainType
{
    // Identity (fixed per terrain type)
    public abstract string TerrainName { get; }
    public abstract string Description { get; }

    // Mechanical properties (fixed per terrain type)
    public abstract bool CanBeEntered { get; }
    public abstract float MovementCostPercent { get; }
    public abstract float RangedDefenseAdjustment { get; }
    public abstract float HazardDamagePerTurn { get; }
    public abstract bool InstantDefeatOnEntry { get; }

    // Destruction behavior
    public abstract bool IsDestructible { get; }
    public abstract TerrainType GetDestroyedTerrain();

    // Helper methods
    public int GetDamageForCreature(Creature creature)
    {
        return Mathf.CeilToInt(creature.MaxHP * HazardDamagePerTurn);
    }

    public virtual int GetMovementCost(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Air))
        {
            return 0;
        }

        if (MovementCostPercent <= 0f) return 0;

        return Mathf.CeilToInt(creature.MaxEnergy * MovementCostPercent);
    }

    public virtual float GetRangedDefenseAdjustment(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Air))
        {
            return 0;
        }

        if (creature.IsElement(CreatureElement.Earth))
        {
            return RangedDefenseAdjustment * EARTH_DEFENSE_BONUS_MULTIPLIER;
        }

        return RangedDefenseAdjustment;
    }

    public virtual bool DoesCreatureTakeDamage(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Air))
        {
            return false;
        }

        return HazardDamagePerTurn != 0f;
    }
    public virtual bool CanBeEnteredBy(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Air))
        {
            return true;
        }

        return CanBeEntered;
    }

    public virtual bool CanBeForcedInto(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Air))
        {
            return true;
        }

        return CanBeEntered;
    }

    public virtual bool IsInstantDefeatForCreature(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Air))
        {
            return false;
        }

        return InstantDefeatOnEntry;
    }
}

// ============================================================================
// SINGLETON INSTANCES - Access via Terrains.X
// ============================================================================

public static class Terrains
{
    public static readonly RegularTerrain Regular = new RegularTerrain();
    public static readonly LightCoverTerrain LightCover = new LightCoverTerrain();
    public static readonly HeavyCoverTerrain HeavyCover = new HeavyCoverTerrain();
    public static readonly LightRoughTerrain LightRough = new LightRoughTerrain();
    public static readonly HeavyRoughTerrain HeavyRough = new HeavyRoughTerrain();
    public static readonly WaterTerrain Water = new WaterTerrain();
    public static readonly LavaTerrain Lava = new LavaTerrain();
    public static readonly ChasmTerrain Chasm = new ChasmTerrain();

    public static TerrainType[] All =
    {
        Regular, LightCover, HeavyCover, LightRough,
        HeavyRough, Water, Lava, Chasm
    };
}

// ============================================================================
// CONCRETE TERRAIN TYPES
// ============================================================================

public class RegularTerrain : TerrainType
{
    public override string TerrainName => "Regular Ground";
    public override string Description => "Normal terrain with no special properties.";

    public override bool CanBeEntered => true;
    public override float MovementCostPercent => 0f;
    public override float RangedDefenseAdjustment => 0f;
    public override float HazardDamagePerTurn => 0f;
    public override bool InstantDefeatOnEntry => false;

    public override bool IsDestructible => false;
    public override TerrainType GetDestroyedTerrain() => null;
}

public class LightCoverTerrain : TerrainType
{
    public override string TerrainName => "Light Cover";
    public override string Description => "Provides minor protection from ranged attacks. Can be destroyed.";

    public override bool CanBeEntered => true;
    public override float MovementCostPercent => 0f;
    public override float RangedDefenseAdjustment => LIGHT_COVER_DEFENSE_BONUS;
    public override float HazardDamagePerTurn => 0f;
    public override bool InstantDefeatOnEntry => false;

    public override bool IsDestructible => true;
    public override TerrainType GetDestroyedTerrain() => Terrains.LightRough;
}

public class HeavyCoverTerrain : TerrainType
{
    public override string TerrainName => "Heavy Cover";
    public override string Description => "Impassable barrier that provides strong protection. Can be destroyed.";

    public override bool CanBeEntered => false;
    public override float MovementCostPercent => 0f;
    public override float RangedDefenseAdjustment => HEAVY_COVER_DEFENSE_BONUS;
    public override float HazardDamagePerTurn => 0f;
    public override bool InstantDefeatOnEntry => false;

    public override bool IsDestructible => true;
    public override TerrainType GetDestroyedTerrain() => Terrains.HeavyRough;
}

public class LightRoughTerrain : TerrainType
{
    public override string TerrainName => "Light Rough Terrain";
    public override string Description => "Difficult terrain that requires extra energy to traverse.";

    public override bool CanBeEntered => true;
    public override float MovementCostPercent => LIGHT_ROUGH_TERRAIN_MOVEMENT_COST;
    public override float RangedDefenseAdjustment => 0f;
    public override float HazardDamagePerTurn => 0f;
    public override bool InstantDefeatOnEntry => false;
    public override bool IsDestructible => false;
    public override TerrainType GetDestroyedTerrain() => null;
}

public class HeavyRoughTerrain : TerrainType
{
    public override string TerrainName => "Heavy Rough Terrain";
    public override string Description => "Very difficult terrain that requires significant energy to traverse.";

    public override bool CanBeEntered => true;
    public override float MovementCostPercent => HEAVY_ROUGH_TERRAIN_MOVEMENT_COST;
    public override float RangedDefenseAdjustment => 0f;
    public override float HazardDamagePerTurn => 0f;
    public override bool InstantDefeatOnEntry => false;

    public override bool IsDestructible => false;
    public override TerrainType GetDestroyedTerrain() => null;
}

public class WaterTerrain : TerrainType
{
    public override string TerrainName => "Water";
    public override string Description => "Deep water that slows movement. Fire and Earth creatures take damage.";

    public override bool CanBeEntered => true;
    public override float MovementCostPercent => HEAVY_ROUGH_TERRAIN_MOVEMENT_COST;
    public override float RangedDefenseAdjustment => HELPLESS_TERRAIN_DEFENSE_PENALTY;
    public override float HazardDamagePerTurn => HAZARDOUS_TERRAIN_DAMAGE_PER_TURN;
    public override bool InstantDefeatOnEntry => false;

    public override bool IsDestructible => false;
    public override TerrainType GetDestroyedTerrain() => null;

    // Water creatures move freely
    public override int GetMovementCost(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Water))
        {
            return 0;
        }
        return base.GetMovementCost(creature);
    }

    // Water creatures don't get the defense penalty
    public override float GetRangedDefenseAdjustment(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Water))
        {
            return 0f;
        }
        return base.GetRangedDefenseAdjustment(creature);
    }

    // Fire and Earth creatures take damage
    public override bool DoesCreatureTakeDamage(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Fire) || creature.IsElement(CreatureElement.Earth))
        {
            return true;
        }

        return false;
    }
}

public class LavaTerrain : TerrainType
{
    public override string TerrainName => "Lava";
    public override string Description => "Molten lava that burns creatures each turn. Fire creatures are immune.";

    public override bool CanBeEntered => true;
    public override float MovementCostPercent => HEAVY_ROUGH_TERRAIN_MOVEMENT_COST;
    public override float RangedDefenseAdjustment => HELPLESS_TERRAIN_DEFENSE_PENALTY;
    public override float HazardDamagePerTurn => HAZARDOUS_TERRAIN_DAMAGE_PER_TURN;
    public override bool InstantDefeatOnEntry => false;

    public override bool IsDestructible => false;
    public override TerrainType GetDestroyedTerrain() => null;

    // Fire creatures move freely
    public override int GetMovementCost(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Fire))
        {
            return 0;
        }
        return base.GetMovementCost(creature);
    }

    // Water and Earth creatures who are not also Fire-Element get a defense penalty
    public override float GetRangedDefenseAdjustment(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Fire))
        {
            return 0f;
        }

        if (creature.IsElement(CreatureElement.Water) || creature.IsElement(CreatureElement.Earth))
        {
            return base.GetRangedDefenseAdjustment(creature);
        }

        return 0f;
    }

    // Fire creatures don't take damage
    public override bool DoesCreatureTakeDamage(Creature creature)
    {
        if (creature.IsElement(CreatureElement.Fire))
        {
            return false;
        }
        return base.DoesCreatureTakeDamage(creature);
    }
}

public class ChasmTerrain : TerrainType
{
    public override string TerrainName => "Chasm";
    public override string Description => "Bottomless pit. Creatures forced into this space are instantly defeated.";

    public override bool CanBeEntered => false;
    public override float MovementCostPercent => 0f;
    public override float RangedDefenseAdjustment => 0f;
    public override float HazardDamagePerTurn => 0f;
    public override bool InstantDefeatOnEntry => true;

    public override bool IsDestructible => false;
    public override TerrainType GetDestroyedTerrain() => null;

    // Though impassable, creatures can be forced into chasms
    public override bool CanBeForcedInto(Creature creature)
    {
         return true;
    }
}