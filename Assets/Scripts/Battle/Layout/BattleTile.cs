using System;
using UnityEngine;

/// <summary>
/// Represents a single tile on a battle grid.
/// Handles terrain, surfaces, and creature occupation.
/// Pure data - no UI logic.
/// </summary>
public class BattleTile
{
    public GridPosition Position { get; }
    public TeamSide TeamOwner { get; }
    public BattleGrid ParentGrid { get; set; }

    // Terrain (permanent for battle)
    public TerrainType Terrain { get; private set; }
    public TerrainVisuals CurrentVisuals { get; private set; }

    // Surface (temporary, can change)
    public SurfaceEffect ActiveSurface { get; private set; }

    // Occupation
    public Creature OccupyingCreature { get; private set; }
    public bool IsOccupied => OccupyingCreature != null;
    public bool IsEmpty => OccupyingCreature == null;

    // Events
    public event Action<Creature> OnCreaturePlaced;
    public event Action<Creature> OnCreatureRemoved;
    public event Action<SurfaceEffect> OnSurfaceApplied;
    public event Action OnSurfaceRemoved;
    public event Action<TerrainType, TerrainType, TerrainVisuals> OnTerrainChanged;


    public BattleTile(GridPosition position, TeamSide owner, BattleGrid parentGrid)
    {
        Position = position;
        TeamOwner = owner;
        ParentGrid = parentGrid;
    }

    // Terrain management
    public void SetTerrain(TerrainType terrain, TerrainVisuals visuals = null)
    {
        var oldTerrain = Terrain;
        Terrain = terrain;
        CurrentVisuals = visuals;
        OnTerrainChanged?.Invoke(oldTerrain, terrain, visuals);
    }


    /// <summary>
    /// Destroy terrain if destructible, replacing it with its destroyed form
    /// </summary>
    public bool DestroyTerrain(Biome currentBiome)
    {
        if (Terrain == null || !Terrain.IsDestructible)
            return false;

        var destroyedTerrain = Terrain.GetDestroyedTerrain();
        if (destroyedTerrain == null)
            return false;

        // Get new visuals from biome
        var newVisuals = currentBiome?.GetRandomVariant(destroyedTerrain.GetType());
        SetTerrain(destroyedTerrain, newVisuals);

        return true;
    }

    // Creature management
    public void PlaceCreature(Creature creature)
    {
        if (IsOccupied)
            throw new InvalidOperationException($"Tile {Position} already occupied by {OccupyingCreature.Nickname}");

        if (Terrain != null && !Terrain.CanBeEnteredBy(creature))
            throw new InvalidOperationException($"Tile {Position} is blocked by {Terrain.TerrainName}");

        OccupyingCreature = creature;
        OnCreaturePlaced?.Invoke(creature);
    }

    public void RemoveCreature()
    {
        if (IsEmpty) return;

        var creature = OccupyingCreature;
        OccupyingCreature = null;
        OnCreatureRemoved?.Invoke(creature);
    }

    // Surface management
    public void ApplySurface(SurfaceEffect surface)
    {
        if (surface == null) return;

        // Remove old surface
        if (ActiveSurface != null)
        {
            OnSurfaceRemoved?.Invoke();
        }

        // Apply new surface
        ActiveSurface = surface;
        OnSurfaceApplied?.Invoke(surface);
    }

    public void RemoveSurface()
    {
        if (ActiveSurface == null) return;

        ActiveSurface = null;
        OnSurfaceRemoved?.Invoke();
    }

    public void TickSurface()
    {
        if (ActiveSurface == null) return;

        ActiveSurface.TickDuration();

        if (ActiveSurface.IsExpired)
        {
            RemoveSurface();
        }
    }

    // Trigger surface effect on creature
    public void TriggerSurfaceEffect(Creature creature, SurfaceTriggerTiming timing, BattleContext context)
    {
        if (ActiveSurface == null) return;
        if (!ActiveSurface.ShouldTrigger(timing)) return;

        ActiveSurface.Apply(creature, context);
    }

    // Movement validation
    public bool CanBeEnteredBy(Creature creature)
    {
        if (IsOccupied) return false;
        if (Terrain == null) return true;
        return Terrain.CanBeEnteredBy(creature);
    }

    public bool CanBeForcedInto(Creature creature)
    {
        if (IsOccupied) return false;
        if (Terrain == null) return true;
        return Terrain.CanBeForcedInto(creature);
    }

    public int GetMovementCost(Creature creature)
    {
        if (Terrain == null) return 0;

        return Terrain.GetMovementCost(creature);
    }

    // Get effective defense modifier for creature on this tile
    // NOTE: Only applies to RANGED attacks
    public float GetRangedDefenseBonus(Creature creature)
    {
        if (Terrain == null) return 0f;

        float bonus = Terrain.GetRangedDefenseAdjustment(creature);

        // Check if the space ahead contains heavy cover
        /*
        if (!creature.IsElement(CreatureElement.Air) && ParentGrid != null)
        {
            GridPosition aheadPos = Position.GetAdjacentPosition(
                creature.TeamSide == TeamSide.Player ? Direction.Right : Direction.Left);
            if (ParentGrid.TryGetTile(aheadPos, out BattleTile aheadTile))
            {
                if (aheadTile?.Terrain is HeavyCoverTerrain)
                {
                    bonus += TerrainConstants.HEAVY_COVER_DEFENSE_BONUS;
                }
            }
        }
        */

        return bonus;
    }

    // Get terrain damage for this creature this turn
    public int GetTerrainDamage(Creature creature)
    {
        if (Terrain == null) return 0;

        return Terrain.GetDamageForCreature(creature);
    }

    public override string ToString() => $"Tile {Position} ({TeamOwner})";
}