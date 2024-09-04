using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleField
{
    //public WeatherEffect ActiveWeatherEffect { get; set; }
    public List<BattleCreature> FieldCreatures { get; set; }
    private BattleCreature[,] PlayerGrid;
    private BattleCreature[,] EnemyGrid;

    int rows;
    int cols;

    public BattleField(int row_num, int col_num)
    {
        rows = row_num;
        cols = col_num;
        FieldCreatures = new List<BattleCreature>();
        PlayerGrid = new BattleCreature[rows, cols];
        EnemyGrid = new BattleCreature[rows, cols];
    }

    public bool AddCreature(BattleCreature creature, int row, int col)
    {
        if (row >= rows || col >= cols) return false;

        var grid = creature.IsPlayerUnit ? PlayerGrid : EnemyGrid;
        if (grid[row, col] == null)
        {
            grid[row, col] = creature;
        } 
        else
        {
            return false;
        }
        FieldCreatures.Add(creature);
        creature.Setup();
        return true;
    }

    public bool RemoveCreature(BattleCreature creature)
    {
        var grid = creature.IsPlayerUnit? PlayerGrid : EnemyGrid;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid[row, col] == creature)
                {
                    grid[row, col] = null;
                    FieldCreatures.Remove(creature);
                    creature.Reset();
                    return true;
                }
            }
        }
        return false;
    }

    public List<BattleCreature> GetAdjacentCreatures(BattleCreature creature)
    {
        var adjacent = new List<BattleCreature>();
        
        (int, int)? coords = GetPosition(creature);

        if (!coords.HasValue) return adjacent;

        int row = coords.Value.Item1;
        int col = coords.Value.Item2;

        var grid = creature.IsPlayerUnit ? PlayerGrid : EnemyGrid;

        if (row > 0) adjacent.Add(grid[row - 1, col]);
        if (row < rows - 1) adjacent.Add(grid[row + 1, col]);
        if (col > 0) adjacent.Add(grid[row, col - 1]);
        if (col < cols - 1) adjacent.Add(grid[row, col + 1]);

        return adjacent.Where(c => c != null).ToList();
    }

    public (int, int)? GetPosition(BattleCreature creature)
    {
        if (creature == null) return null;

        var grid = creature.IsPlayerUnit ? PlayerGrid : EnemyGrid;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid[row, col] == creature)
                {
                    return (row, col);
                }
            }
        }
        return null;
    }

    public BattleCreature GetCreature(bool isPlayerUnit, int row, int col)
    {
        var grid = isPlayerUnit ? PlayerGrid : EnemyGrid;

        return grid[row, col];
    }

    public List<BattleCreature> GetTargets(bool isPlayerUnit, bool melee)
    {
        var grid = isPlayerUnit ? PlayerGrid : EnemyGrid;
        int maxCol = melee ? 1 : cols;
        var targets = new List<BattleCreature>();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < maxCol; col++)
            {
                if (grid[row, col] != null)
                {
                    targets.Add(grid[row, col]);
                }
            }
        }
        return targets;
    }
}
