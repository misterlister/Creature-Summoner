using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GameConstants;
using static BattleUIConstants;

/// <summary>
/// Manages both player and enemy grids as a unified 3×6 battlefield.
/// Provides global position access and cross-grid operations.
/// </summary>
public class UnifiedBattlefield : MonoBehaviour
{
    [Header("Grid Setup")]
    [SerializeField] private Transform playerGridParent;
    [SerializeField] private Transform enemyGridParent;
    [SerializeField] private BattleTileUI tilePrefab;

    [Header("Visual Settings")]
    private Biome currentBiome;
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
        bool isPlayer = team == TeamSide.Player;

        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            Vector2 tileSize = GetTileSize(row);

            for (int col = 0; col < GRID_COLS; col++)
            {
                BattleTile dataTile = grid.GetTile(row, col);

                BattleTileUI tileUI = Instantiate(tilePrefab, parent);
                RectTransform rt = tileUI.GetComponent<RectTransform>();

                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot = isPlayer ? new Vector2(1, 0) : Vector2.zero;

                //Debug.Log($"Placing tile at row {row}, col {col} for {(isPlayer ? "Player" : "Enemy")} grid. Calculated position: {GetTilePosition(row, col, tileSize, isPlayer)}");

                rt.anchoredPosition = GetTilePosition(row, col, tileSize, isPlayer);

                tileUI.SetTileSize(row);
                tileUI.Initialize(dataTile, isPlayer);

                allTileUIs[dataTile.BattlefieldPosition] = tileUI;
            }
        }
    }

    private Vector2 GetTileSize(int row)
    {
        float scale = row switch
        {
            1 => TILE_SCALE_ROW1,
            2 => TILE_SCALE_ROW2,
            _ => 1f
        };
        return new Vector2(TILE_WIDTH_ROW0 * scale, TILE_HEIGHT_ROW0 * scale);
    }

    private Vector2 GetTilePosition(int row, int col, Vector2 tileSize, bool isPlayer)
    {
        float y = 660f;
        for (int r = 0; r < row; r++)
            y -= GetTileSize(r).y;
        y -= tileSize.y;

        float x;
        if (isPlayer)
        {
            // Col 2 (frontline) flush against right edge, col 0 (backline) furthest left
            float rowWidth = GRID_COLS * tileSize.x;
            x = 800f - (GRID_COLS - 1 - col) * tileSize.x;
        }
        else
        {
            // Count rightward from left edge — col 0 (frontline) at x=0, col 2 (backline) most positive
            x = col * tileSize.x;
        }

        return new Vector2(x, y);
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
        return creature?.CurrentTile?.BattlefieldPosition ?? default;
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

        var center = tile.BattlefieldPosition;
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

    public void SwapCreatures(BattleTile tile1, BattleTile tile2)
    {
        if (tile1 == null || tile2 == null)
        {
            Debug.LogError($"Cannot swap: null tile(s)");
            return;
        }

        var creature1 = tile1.OccupyingCreature;
        var creature2 = tile2.OccupyingCreature;

        tile1.RemoveCreature();
        tile2.RemoveCreature();

        if (creature2 != null) tile1.PlaceCreature(creature2);
        if (creature1 != null) tile2.PlaceCreature(creature1);

        creature1?.SetBattlePosition(tile2);
        creature2?.SetBattlePosition(tile1);
    }

    #endregion

    #region Terrain Management

    public void ApplyTerrainLayout(TerrainLayout layout, Biome biome)
    {
        currentBiome = biome;
        SetBattleBackground();
        PlayerGrid.ApplyTerrainLayout(layout, currentBiome);
        EnemyGrid.ApplyTerrainLayout(layout, currentBiome);
    }

    #endregion

    #region Visual Feedback

    public void HighlightTiles(List<BattleTile> tiles, HighlightType highlightType)
    {
        foreach (var tile in tiles)
        {
            var tileUI = GetTileUI(tile.BattlefieldPosition);
            tileUI?.SetPersistentHighlight(highlightType);
        }
    }

    public void ClearAllHighlights()
    {
        foreach (var tileUI in allTileUIs.Values)
        {
            tileUI.ClearPersistentHighlight();
        }
    }

    public void SetActiveCreatureHighlight(Creature creature)
    {
        ClearAllHighlights();

        var tileUI = GetTileUI(creature.CurrentTile.BattlefieldPosition);
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