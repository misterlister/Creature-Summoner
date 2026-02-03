using System;
using System.Collections.Generic;
using System.Linq;

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

    // Forwarded creature events (tile forwards these)
    public event Action<int, int> OnCreatureHPChanged;
    public event Action<int> OnCreatureDamaged;
    public event Action<int> OnCreatureHealed;

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

    /// <summary>
    /// Get adjacent creatures (non-null occupants)
    /// </summary>
    public List<Creature> GetAdjacentCreatures()
    {
        List<BattleTile> adjacentTiles = ParentGrid.GetAdjacentTiles(this);
        if (adjacentTiles == null) return new List<Creature>();
        return adjacentTiles
            .Where(t => t.OccupyingCreature != null)
            .Select(t => t.OccupyingCreature)
            .ToList();
    }

    /// <summary>
    /// Get adjacent enemies relative to the provided reference creature.
    /// If reference is null, uses the occupying creature of this tile.
    /// </summary>
    public List<Creature> GetAdjacentEnemies(Creature reference = null)
    {
        reference ??= OccupyingCreature;
        if (reference == null) return new List<Creature>();

        return GetAdjacentCreatures()
            .Where(c => reference.IsEnemy(c))
            .ToList();
    }

    /// <summary>
    /// Get adjacent allies relative to the provided reference creature.
    /// If reference is null, uses the occupying creature of this tile.
    /// </summary>
    public List<Creature> GetAdjacentAllies(Creature reference = null)
    {
        reference ??= OccupyingCreature;
        if (reference == null) return new List<Creature>();

        return GetAdjacentCreatures()
            .Where(c => !reference.IsEnemy(c) && c != reference)
            .ToList();
    }


    // Creature management
    public void PlaceCreature(Creature creature)
    {
        if (IsOccupied)
            throw new InvalidOperationException($"Tile {Position} already occupied by {OccupyingCreature?.Nickname}");

        if (Terrain != null && !Terrain.CanBeEnteredBy(creature))
            throw new InvalidOperationException($"Tile {Position} is blocked by {Terrain.TerrainName}");

        OccupyingCreature = creature;

        // Subscribe to creature events and forward them
        if (OccupyingCreature != null)
        {
            OccupyingCreature.OnHPChanged += HandleOccupantHPChanged;
            OccupyingCreature.OnTakeDamage += HandleOccupantDamaged;
            OccupyingCreature.OnHealed += HandleOccupantHealed;
        }

        OnCreaturePlaced?.Invoke(creature);
    }

    public void RemoveCreature()
    {
        if (IsEmpty) return;

        var creature = OccupyingCreature;

        // Unsubscribe forwarded handlers
        if (OccupyingCreature != null)
        {
            OccupyingCreature.OnHPChanged -= HandleOccupantHPChanged;
            OccupyingCreature.OnTakeDamage -= HandleOccupantDamaged;
            OccupyingCreature.OnHealed -= HandleOccupantHealed;
        }

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

    // Forwarders for creature events
    private void HandleOccupantHPChanged(int current, int max)
    {
        OnCreatureHPChanged?.Invoke(current, max);
    }

    private void HandleOccupantDamaged(int dmg)
    {
        OnCreatureDamaged?.Invoke(dmg);
    }

    private void HandleOccupantHealed(int amt)
    {
        OnCreatureHealed?.Invoke(amt);
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

}