using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages both player and enemy grids as a unified 3×6 battlefield.
/// Provides global position access and cross-grid operations.
/// </summary>
public class UnifiedBattlefield : MonoBehaviour
{
    [Header("Grid Setup")]
    [SerializeField] private Transform playerGridParent;
    [SerializeField] private Transform enemyGridParent;
    [SerializeField] private GameObject tilePrefab;

    [Header("Visual Settings")]
    [SerializeField] private Biome currentBiome;
    [SerializeField] private Image battleBackground;

    // Data layer - the source of truth
    public BattleGrid PlayerGrid { get; private set; }
    public BattleGrid EnemyGrid { get; private set; }

    // Presentation layer - visual representations
    private Dictionary<BattlePosition, BattleTileUI> allTileUIs = new Dictionary<BattlePosition, BattleTileUI>();

    // Targeting system
    public TargetingSystem TargetingSystem { get; private set; }

    // Events for battle-wide systems
    public event Action<Creature> OnCreatureDefeated;
    public event Action<Creature, BattlePosition> OnCreaturePlaced;
    public event Action<Creature, BattlePosition> OnCreatureRemoved;

    private void Awake()
    {
        InitializeGrids();
        SetupTileUI();
        TargetingSystem = new TargetingSystem(this);
        SetBattleBackground();
    }

    private void InitializeGrids()
    {
        PlayerGrid = new BattleGrid(TeamSide.Player, this);
        EnemyGrid = new BattleGrid(TeamSide.Enemy, this);

        // Subscribe to grid events
        PlayerGrid.OnCreaturePlaced += (c, gp) => OnCreaturePlaced?.Invoke(c, GetBattlePosition(gp, TeamSide.Player));
        PlayerGrid.OnCreatureRemoved += (c, gp) => OnCreatureRemoved?.Invoke(c, GetBattlePosition(gp, TeamSide.Player));
        EnemyGrid.OnCreaturePlaced += (c, gp) => OnCreaturePlaced?.Invoke(c, GetBattlePosition(gp, TeamSide.Enemy));
        EnemyGrid.OnCreatureRemoved += (c, gp) => OnCreatureRemoved?.Invoke(c, GetBattlePosition(gp, TeamSide.Enemy));
    }

    #region Terrain Management
    private void SetupTileUI()
    {
        SetupGridUI(PlayerGrid, playerGridParent, TeamSide.Player);
        SetupGridUI(EnemyGrid, enemyGridParent, TeamSide.Enemy);
    }

    private void SetupGridUI(BattleGrid grid, Transform parent, TeamSide team)
    {
        foreach (var tile in grid.GetAllTiles())
        {
            GameObject tileObj = Instantiate(tilePrefab, parent);
            BattleTileUI tileUI = tileObj.GetComponent<BattleTileUI>();

            bool isPlayerSide = team == TeamSide.Player;
            tileUI.Initialize(tile, isPlayerSide);

            BattlePosition battlePos = GetBattlePosition(tile.Position, team);
            allTileUIs[battlePos] = tileUI;

            // Wire up tile events to UI
            tile.OnCreaturePlaced += tileUI.OnCreaturePlaced;
            tile.OnCreatureRemoved += tileUI.OnCreatureRemoved;
            tile.OnTerrainChanged += tileUI.OnTerrainChanged;
            tile.OnSurfaceApplied += tileUI.OnSurfaceApplied;
            tile.OnSurfaceRemoved += tileUI.OnSurfaceRemoved;
        }
    }

    private void SetBattleBackground()
    {
        // Validate references
        if (battleBackground == null)
        {
            Debug.LogWarning("UnifiedBattlefield: Battle Background Image is not assigned in Inspector!");
            return;
        }

        if (currentBiome == null)
        {
            Debug.LogWarning("UnifiedBattlefield: Current Biome is not assigned!");
            return;
        }

        // Set background sprite from biome
        if (currentBiome.BackgroundSprite != null)
        {
            battleBackground.sprite = currentBiome.BackgroundSprite;
        }
        else
        {
            Debug.LogWarning($"UnifiedBattlefield: Biome '{currentBiome.BiomeName}' has no BackgroundSprite assigned!");
        }
    }

    #endregion

    #region Unified Tile Access

    /// <summary>
    /// Get tile at global battle position
    /// </summary>
    public BattleTile GetTile(BattlePosition pos)
    {
        if (!pos.IsValid()) return null;

        var grid = GetGrid(pos.GetTeamSide());
        return grid.GetTile(pos.ToLocalGridPosition());
    }

    public bool TryGetTile(BattlePosition pos, out BattleTile tile)
    {
        tile = GetTile(pos);
        return tile != null;
    }

    public BattleTileUI GetTileUI(BattlePosition pos)
    {
        return allTileUIs.TryGetValue(pos, out var tileUI) ? tileUI : null;
    }

    #endregion

    #region Position Conversions

    /// <summary>
    /// Convert local GridPosition to global BattlePosition
    /// </summary>
    public BattlePosition GetBattlePosition(GridPosition gridPos, TeamSide team)
    {
        return BattlePosition.FromGridPosition(gridPos, team);
    }

    /// <summary>
    /// Get battle position for a creature
    /// </summary>
    public BattlePosition GetBattlePosition(Creature creature)
    {
        if (creature?.CurrentTile == null)
            return default;

        return GetBattlePosition(creature.CurrentTile.Position, creature.TeamSide);
    }

    /// <summary>
    /// Get battle position for a tile
    /// </summary>
    public BattlePosition GetBattlePosition(BattleTile tile)
    {
        if (tile == null) return default;
        return GetBattlePosition(tile.Position, tile.TeamOwner);
    }

    #endregion

    #region Adjacency

    /// <summary>
    /// Get adjacent tiles across the full 3x6 battlefield for the provided tile.
    /// </summary>
    public List<BattleTile> GetAdjacentTiles(BattleTile tile)
    {
        var result = new List<BattleTile>();
        if (tile == null) return result;

        var center = GetBattlePosition(tile);
        if (!center.IsValid()) return result;

        // 8-directional offsets
        (int dRow, int dCol)[] directions = {
            (-1, 0), (1, 0),
            (0, -1), (0, 1),
            (-1, -1), (-1, 1),
            (1, -1), (1, 1)
        };

        foreach (var (dRow, dCol) in directions)
        {
            int r = center.Row + dRow;
            int c = center.GlobalCol + dCol;

            if (r < 0 || r >= GameConstants.BATTLE_ROWS || c < 0 || c >= GameConstants.BATTLE_COLS)
                continue;

            var neighborPos = new BattlePosition(r, c);
            var neighbor = GetTile(neighborPos);
            if (neighbor != null) result.Add(neighbor);
        }

        return result;
    }

    #endregion

    #region Path and Blocker Queries

    /// <summary>
    /// Get all tiles between two positions (exclusive)
    /// </summary>
    public List<BattleTile> GetTilesBetween(BattlePosition from, BattlePosition to, bool sameRowOnly = true)
    {
        var tiles = new List<BattleTile>();

        if (sameRowOnly && from.Row != to.Row)
            return tiles;

        int minCol = Math.Min(from.GlobalCol, to.GlobalCol);
        int maxCol = Math.Max(from.GlobalCol, to.GlobalCol);

        for (int col = minCol + 1; col < maxCol; col++)
        {
            var pos = new BattlePosition(from.Row, col);
            var tile = GetTile(pos);
            if (tile != null)
            {
                tiles.Add(tile);
            }
        }

        return tiles;
    }

    /// <summary>
    /// Count occupied tiles (blockers) between two positions.
    /// Counts tiles in the same row, excluding the start and end positions.
    /// </summary>
    public int CountBlockersBetween(BattlePosition from, BattlePosition to)
    {
        // Only count if in same row
        if (from.Row != to.Row)
            return 0;

        var betweenTiles = GetTilesBetween(from, to, sameRowOnly: true);
        return betweenTiles.Count(t => t.IsOccupied);
    }

    /// <summary>
    /// Check if there's heavy cover between attacker and target
    /// </summary>
    public bool IsHeavyCoverBetween(BattlePosition from, BattlePosition to)
    {
        if (from.Row != to.Row)
            return false;

        var betweenTiles = GetTilesBetween(from, to);
        return betweenTiles.Any(t => t.Terrain is HeavyCoverTerrain);
    }

    /// <summary>
    /// Get all creatures in a specific row across both grids
    /// </summary>
    public List<Creature> GetCreaturesInRow(int row)
    {
        var creatures = new List<Creature>();

        for (int col = 0; col < 6; col++)
        {
            var pos = new BattlePosition(row, col);
            var tile = GetTile(pos);
            if (tile != null && tile.IsOccupied)
            {
                creatures.Add(tile.OccupyingCreature);
            }
        }

        return creatures;
    }

    #endregion

    #region Creature Management

    public bool PlaceCreature(Creature creature, BattlePosition position)
    {
        if (creature == null || !position.IsValid())
            return false;

        var grid = GetGrid(position.GetTeamSide());
        var localPos = position.ToLocalGridPosition();

        try
        {
            grid.PlaceCreature(creature, localPos);
            creature.TeamSide = position.GetTeamSide();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error placing creature at {position}: {e.Message}");
            return false;
        }
    }

    public void RemoveCreature(Creature creature)
    {
        if (creature?.CurrentTile == null) return;

        var grid = GetGrid(creature.TeamSide);
        grid.RemoveCreature(creature);
    }

    public void RemoveCreature(BattlePosition position)
    {
        var grid = GetGrid(position.GetTeamSide());
        grid.RemoveCreature(position.ToLocalGridPosition());
    }

    public BattleTile FindCreatureTile(Creature creature)
    {
        if (creature == null) return null;

        var grid = GetGrid(creature.TeamSide);
        return grid.FindCreatureTile(creature);
    }

    public List<Creature> GetAllCreatures()
    {
        var creatures = new List<Creature>();
        creatures.AddRange(PlayerGrid.GetAllCreatures());
        creatures.AddRange(EnemyGrid.GetAllCreatures());
        return creatures;
    }

    public void SwapCreatures(Creature creature1, Creature creature2)
    {
        var tile1 = FindCreatureTile(creature1);
        var tile2 = FindCreatureTile(creature2);

        if (tile1 == null || tile2 == null)
        {
            Debug.LogError("Cannot swap: one or both creatures not found");
            return;
        }

        // Remove both
        tile1.RemoveCreature();
        tile2.RemoveCreature();

        // Place in swapped positions
        tile1.PlaceCreature(creature2);
        tile2.PlaceCreature(creature1);

        creature1.SetBattlePosition(tile2);
        creature2.SetBattlePosition(tile1);
    }

    #endregion

    #region Terrain Management

    public void ApplyTerrainLayout(TerrainLayout layout)
    {
        PlayerGrid.ApplyTerrainLayout(layout, currentBiome);
        EnemyGrid.ApplyTerrainLayout(layout, currentBiome);
    }

    public void SetBiome(Biome biome)
    {
        currentBiome = biome;
    }

    #endregion

    #region Visual Feedback

    public void HighlightTiles(List<BattleTile> tiles, HighlightType highlightType)
    {
        foreach (var tile in tiles)
        {
            var battlePos = GetBattlePosition(tile);
            var tileUI = GetTileUI(battlePos);
            tileUI?.SetHighlight(highlightType);
        }
    }

    public void ClearAllHighlights()
    {
        foreach (var tileUI in allTileUIs.Values)
        {
            tileUI.ClearHighlight();
        }
    }

    public void SetActiveCreatureHighlight(Creature creature)
    {
        ClearAllHighlights();

        var battlePos = GetBattlePosition(creature);
        var tileUI = GetTileUI(battlePos);
        tileUI?.SetHighlight(HighlightType.ActiveCreature);
    }

    #endregion

    #region Utility

    public BattleGrid GetGrid(TeamSide team)
    {
        return team == TeamSide.Player ? PlayerGrid : EnemyGrid;
    }

    public int GetEnemyCount()
    {
        return EnemyGrid.GetAllCreatures().Count;
    }

    public int GetPlayerCount()
    {
        return PlayerGrid.GetAllCreatures().Count;
    }

    #endregion
}

public enum HighlightType
{
    None,
    ActiveCreature,
    ValidTarget,
    InvalidTarget,
    MoveTarget,
    PositiveTarget,
    NegativeTarget
}