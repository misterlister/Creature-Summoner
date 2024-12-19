using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameConstants;

public class BattleField : MonoBehaviour
{
    //public WeatherEffect ActiveWeatherEffect { get; set; }
    [SerializeField] private GameObject battleSlotPrefab;
    [SerializeField] private Transform playerSlotsParent;
    [SerializeField] private Transform enemySlotsParent;

    private BattleSlot[,] PlayerGrid;
    private BattleSlot[,] EnemyGrid;
    public List<BattleSlot> FieldCreatures { get; set; } = new List<BattleSlot>();


    private void Start()
    {
        PlayerGrid = CreatePlayerBattleSlots();
        EnemyGrid = CreateEnemyBattleSlots();
    }

    private BattleSlot[,] CreatePlayerBattleSlots()
    {
        BattleSlot[,] slots = new BattleSlot[BATTLE_ROWS, BATTLE_COLS];
        for (int col = BATTLE_COLS - 1; col >= 0; col--)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                GameObject slotInstance = Instantiate(battleSlotPrefab, playerSlotsParent);
                BattleSlot battleSlot = slotInstance.GetComponent<BattleSlot>();
                battleSlot.Initialize(true);
                slots[row, col] = battleSlot;
            }
        }
        return slots;
    }

    private BattleSlot[,] CreateEnemyBattleSlots()
    {
        BattleSlot[,] slots = new BattleSlot[BATTLE_ROWS, BATTLE_COLS];
        for (int col = 0; col < BATTLE_COLS; col++)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                GameObject slotInstance = Instantiate(battleSlotPrefab, enemySlotsParent);
                BattleSlot battleSlot = slotInstance.GetComponent<BattleSlot>();
                battleSlot.Initialize(false);
                slots[row, col] = battleSlot;
            }
        }
        return slots;
    }

    public int EnemyCount()
    {
        int count = 0;
        for (int i = 0; i < BATTLE_ROWS; i++)
        {
            for (int j = 0; j < BATTLE_COLS; j++)
            {
                if (!EnemyGrid[i, j].IsEmpty)
                    count++;
            }
        }
        return count;
    }

    public bool AddCreature(Creature creature, int row, int col, bool isPlayerUnit)
    {
        if (row >= BATTLE_ROWS || col >= BATTLE_COLS) return false;

        var grid = isPlayerUnit ? PlayerGrid : EnemyGrid;
        if (grid[row, col].IsEmpty)
        {
            grid[row, col].Setup(creature);
        } 
        else
        {
            Debug.Log($"Error: row ${row} col ${col} player: {isPlayerUnit} can't initialize, as it's not empty!");
            return false;
        }

        InsertCreatureInOrder(grid[row, col], row, col);

        return true;
    }

    private void InsertCreatureInOrder(BattleSlot creature, int row, int col)
    {
        int index = GetInsertIndex(creature.IsPlayerSlot, row, col);
        if (index >= FieldCreatures.Count)
        {
            FieldCreatures.Add(creature);
        }
        else
        {
            FieldCreatures.Insert(index, creature);
        }
    }

    private int GetInsertIndex(bool isPlayerUnit, int row, int col)
    {
        int index = 0;

        // Player's back column comes first (col 1)
        for (int r = 0; r < BATTLE_ROWS; r++)
        {
            if (!PlayerGrid[r, 1].IsEmpty)
            {
                if (isPlayerUnit && col == 1 && row == r)
                {
                    return index;
                }
                index++;
            }
        }

        // Player's front column comes next (col 0)
        for (int r = 0; r < BATTLE_ROWS; r++)
        {
            if (!PlayerGrid[r, 0].IsEmpty)
            {
                if (isPlayerUnit && col == 0 && row == r)
                {
                    return index;
                }
                index++;
            }
        }

        // Enemy's front column comes next (col 0)
        for (int r = 0; r < BATTLE_ROWS; r++)
        {
            if (!EnemyGrid[r, 0].IsEmpty)
            {
                if (!isPlayerUnit && col == 0 && row == r)
                {
                    return index;
                }
                index++;
            }
        }

        // Enemy's back column comes last (col 1)
        for (int r = 0; r < BATTLE_ROWS; r++)
        {
            if (!EnemyGrid[r, 1].IsEmpty)
            {
                if (!isPlayerUnit && col == 1 && row == r)
                {
                    return index;
                }
                index++;
            }
        }
        return index;
    }

    public void RemoveCreature(BattleSlot creature)
    {
        FieldCreatures.Remove(creature);
        creature.ClearSlot();
    }

    public List<BattleSlot> GetAdjacentCreatures(BattleSlot creature)
    {
        var adjacent = new List<BattleSlot>();
        
        (int, int)? coords = GetPosition(creature);

        if (!coords.HasValue) return adjacent;

        int row = coords.Value.Item1;
        int col = coords.Value.Item2;

        var grid = creature.IsPlayerSlot ? PlayerGrid : EnemyGrid;

        if (row > 0) adjacent.Add(grid[row - 1, col]);
        if (row < BATTLE_ROWS - 1) adjacent.Add(grid[row + 1, col]);
        if (col > 0) adjacent.Add(grid[row, col - 1]);
        if (col < BATTLE_COLS - 1) adjacent.Add(grid[row, col + 1]);

        return adjacent.Where(c => !c.IsEmpty).ToList();
    }

    public (int, int)? GetPosition(BattleSlot creature)
    {
        if (creature.IsEmpty) return null;

        var grid = creature.IsPlayerSlot ? PlayerGrid : EnemyGrid;

        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            for (int col = 0; col < BATTLE_COLS; col++)
            {
                if (grid[row, col] == creature)
                {
                    return (row, col);
                }
            }
        }
        return null;
    }

    public BattleSlot GetCreature(bool isPlayerUnit, int row, int col)
    {
        if (row >= BATTLE_ROWS || row < 0) return null;
        if (col >= BATTLE_COLS || col < 0) return null;

        var grid = isPlayerUnit ? PlayerGrid : EnemyGrid;

        return grid[row, col];
    }

    public List<BattleSlot> GetTargets(bool isPlayerUnit, bool melee)
    {
        var grid = isPlayerUnit ? PlayerGrid : EnemyGrid;
        int maxCol = melee ? 1 : BATTLE_COLS;
        var targets = new List<BattleSlot>();

        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            for (int col = 0; col < maxCol; col++)
            {
                if (!grid[row, col].IsEmpty)
                {
                    targets.Add(grid[row, col]);
                }
            }
        }
        return targets;
    }
}
