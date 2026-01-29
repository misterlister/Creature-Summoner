using System;
using System.Collections.Generic;
using System.Linq;
using static GameConstants;
using static TerrainConstants;

/// <summary>
/// Represents one 3x3 battle grid (player or enemy).
/// Manages tiles, terrain, and creature placement.
/// </summary>
public class BattleGrid
{
    public TeamSide TeamOwner { get; }
    public UnifiedBattlefield Battlefield { get; set; }
    private BattleTile[,] tiles = new BattleTile[BATTLE_ROWS, BATTLE_COLS];

    // Events
    public event Action<Creature, GridPosition> OnCreaturePlaced;
    public event Action<Creature, GridPosition> OnCreatureRemoved;
    public event Action<TerrainLayout> OnTerrainApplied;

    public BattleGrid(TeamSide owner, UnifiedBattlefield battlefield = null)
    {
        TeamOwner = owner;
        InitializeTiles();
        Battlefield = battlefield;
    }

    private void InitializeTiles()
    {
        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            for (int col = 0; col < BATTLE_COLS; col++)
            {
                var pos = new GridPosition(row, col);
                var tile = new BattleTile(pos, TeamOwner, this);
                tiles[row, col] = tile;

                // Subscribe to tile events for propagation
                tile.OnCreaturePlaced += c => OnCreaturePlaced?.Invoke(c, pos);
                tile.OnCreatureRemoved += c => OnCreatureRemoved?.Invoke(c, pos);
            }
        }
    }

    // Tile access
    public BattleTile GetTile(GridPosition pos)
    {
        if (!pos.IsValid()) return null;
        return tiles[pos.Row, pos.Col];
    }

    public BattleTile GetTile(int row, int col)
    {
        var pos = GridPosition.TryCreate(row, col);
        return pos.HasValue ? GetTile(pos.Value) : null;
    }

    public bool TryGetTile(GridPosition pos, out BattleTile tile)
    {
        tile = GetTile(pos);
        return tile != null;
    }

    // Terrain management
    public void ApplyTerrainLayout(TerrainLayout layout, Biome biome)
    {
        if (layout == null)
        {
            ClearAllTerrain();
            return;
        }

        var slots = layout.GetAllSlots();
        foreach (var slot in slots)
        {
            var tile = GetTile(slot.Position);
            if (tile != null)
            {
                TerrainType terrainInstance = slot.TerrainType switch
                {
                    TerrainTypeEnum.Regular => Terrains.Regular,
                    TerrainTypeEnum.LightCover => Terrains.LightCover,
                    TerrainTypeEnum.HeavyCover => Terrains.HeavyCover,
                    TerrainTypeEnum.LightRough => Terrains.LightRough,
                    TerrainTypeEnum.HeavyRough => Terrains.HeavyRough,
                    TerrainTypeEnum.Chasm => Terrains.Chasm,
                    _ => null
                };

                if (terrainInstance != null)
                {
                    var visuals = biome?.GetRandomVariant(terrainInstance.GetType());
                    tile.SetTerrain(terrainInstance, visuals);
                }
            }
        }

        OnTerrainApplied?.Invoke(layout);
    }

    public void ClearAllTerrain()
    {
        foreach (var tile in GetAllTiles())
        {
            tile.SetTerrain(null);
        }
    }

    // Creature placement
    public void PlaceCreature(Creature creature, GridPosition position)
    {
        if (creature == null)
            throw new ArgumentNullException(nameof(creature));

        var tile = GetTile(position);
        if (tile == null)
            throw new ArgumentException($"Invalid position {position}");

        tile.PlaceCreature(creature);
        creature.SetBattlePosition(tile);
    }

    public void RemoveCreature(GridPosition position)
    {
        var tile = GetTile(position);
        tile?.RemoveCreature();
    }

    public void RemoveCreature(Creature creature)
    {
        var tile = FindCreatureTile(creature);
        tile?.RemoveCreature();
    }

    // Queries
    public BattleTile FindCreatureTile(Creature creature)
    {
        return GetAllTiles().FirstOrDefault(t => t.OccupyingCreature == creature);
    }

    public List<BattleTile> GetAllTiles()
    {
        var result = new List<BattleTile>(BATTLE_ROWS * BATTLE_COLS);
        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            for (int col = 0; col < BATTLE_COLS; col++)
            {
                result.Add(tiles[row, col]);
            }
        }
        return result;
    }

    public List<BattleTile> GetOccupiedTiles()
    {
        return GetAllTiles().Where(t => t.IsOccupied).ToList();
    }

    public List<BattleTile> GetEmptyTiles()
    {
        return GetAllTiles().Where(t => t.IsEmpty).ToList();
    }

    public List<Creature> GetAllCreatures()
    {
        return GetOccupiedTiles()
            .Select(t => t.OccupyingCreature)
            .Where(c => c != null)
            .ToList();
    }

    public List<BattleTile> GetColumn(int col)
    {
        if (col < 0 || col >= BATTLE_COLS)
            return new List<BattleTile>();

        var result = new List<BattleTile>(BATTLE_ROWS);
        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            result.Add(tiles[row, col]);
        }
        return result;
    }

    public List<BattleTile> GetRow(int row)
    {
        if (row < 0 || row >= BATTLE_ROWS)
            return new List<BattleTile>();

        var result = new List<BattleTile>(BATTLE_COLS);
        for (int col = 0; col < BATTLE_COLS; col++)
        {
            result.Add(tiles[row, col]);
        }
        return result;
    }

    // Get tiles by column type
    public List<BattleTile> GetFrontline() => GetColumn(0);
    public List<BattleTile> GetMidline() => GetColumn(1);
    public List<BattleTile> GetBackline() => GetColumn(2);

    // Get adjacent tiles by position
    public List<BattleTile> GetAdjacentTiles(BattleTile tile)
    {
        if (tile == null) return new List<BattleTile>();

        if (Battlefield != null)
        {
            return Battlefield.GetAdjacentTiles(tile);
        }

        return GetAdjacentTiles(tile.Position);
    }

    public List<BattleTile> GetAdjacentTiles(GridPosition pos)
    {
        var result = new List<BattleTile>();
        if (!pos.IsValid()) return result;

        // 8-directional adjacency (cardinal + diagonal)
        (int dRow, int dCol)[] directions = {
            (-1, 0), (1, 0),  // Up, Down
            (0, -1), (0, 1),  // Left, Right
            (-1, -1), (-1, 1), // Up-Left, Up-Right
            (1, -1), (1, 1)   // Down-Left, Down-Right
        };

        foreach (var (dRow, dCol) in directions)
        {
            int r = pos.Row + dRow;
            int c = pos.Col + dCol;
            if (r >= 0 && r < BATTLE_ROWS && c >= 0 && c < BATTLE_COLS)
            {
                result.Add(tiles[r, c]);
            }
        }

        return result;
    }

    // Surface effects
    public void TickAllSurfaces()
    {
        foreach (var tile in GetAllTiles())
        {
            tile.TickSurface();
        }
    }

    // Movement validation
    public bool CanCreatureMoveTo(Creature creature, GridPosition targetPos)
    {
        var tile = GetTile(targetPos);
        return tile != null && tile.CanBeEnteredBy(creature);
    }

    public int GetMovementCost(Creature creature, GridPosition targetPos)
    {
        var tile = GetTile(targetPos);
        return tile?.GetMovementCost(creature) ?? int.MaxValue;
    }

    // Get valid movement positions for a creature
    public List<GridPosition> GetValidMovePositions(Creature creature, int maxEnergyCost)
    {
        var validPositions = new List<GridPosition>();

        foreach (var tile in GetEmptyTiles())
        {
            if (tile.CanBeEnteredBy(creature))
            {
                int cost = tile.GetMovementCost(creature);
                if (cost <= maxEnergyCost)
                {
                    validPositions.Add(tile.Position);
                }
            }
        }

        return validPositions;
    }

    // Clear all creatures (for battle cleanup)
    public void ClearAllCreatures()
    {
        foreach (var tile in GetOccupiedTiles())
        {
            tile.RemoveCreature();
        }
    }
}
