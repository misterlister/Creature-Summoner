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

    private BattleSlot[,] BattleGrid;

    public List<BattleSlot> FieldCreatures { get; set; } = new List<BattleSlot>();


    private void Start()
    {
        BattleGrid = new BattleSlot[BATTLE_ROWS, BATTLE_COLS];
        SetupPlayerBattleSlots();
        SetupEnemyBattleSlots();
    }

    private void SetupPlayerBattleSlots()
    {
        
        for (int col = 0; col < ENEMY_COL; col++)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                GameObject slotInstance = Instantiate(battleSlotPrefab, playerSlotsParent);
                BattleSlot battleSlot = slotInstance.GetComponent<BattleSlot>();
                battleSlot.Initialize(playerSlot: true, row, col);
                BattleGrid[row, col] = battleSlot;
            }
        }
    }

    private void SetupEnemyBattleSlots()
    {
        for (int col = ENEMY_COL; col < BATTLE_COLS; col++)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                GameObject slotInstance = Instantiate(battleSlotPrefab, enemySlotsParent);
                BattleSlot battleSlot = slotInstance.GetComponent<BattleSlot>();
                battleSlot.Initialize(playerSlot: false, row, col);
                BattleGrid[row, col] = battleSlot;
            }
        }
    }

    public int EnemyCount()
    {
        int count = 0;
        for (int i = 0; i < BATTLE_ROWS; i++)
        {
            for (int j = ENEMY_COL; j < BATTLE_COLS; j++)
            {
                if (!BattleGrid[i, j].IsEmpty)
                    count++;
            }
        }
        return count;
    }

    public bool AddCreature(Creature creature, int row, int col, bool isPlayerUnit)
    {
        if (row >= BATTLE_ROWS || col >= BATTLE_COLS) return false;

        if (creature == null) return false;

        if (isPlayerUnit && col >= ENEMY_COL)
        {
            Debug.Log($"Error: row ${row} col ${col}. Can't insert player unit into enemy's side!");
            return false;
        }

        if (!isPlayerUnit && col < ENEMY_COL)
        {
            Debug.Log($"Error: row ${row} col ${col}. Can't insert enemy unit into player's side!");
            return false;
        }

        if (BattleGrid[row, col].IsEmpty)
        {
            BattleGrid[row, col].Setup(creature);
        } 
        else
        {
            Debug.Log($"Error: row ${row} col ${col} player: {isPlayerUnit} can't insert into non-empty row!");
            return false;
        }

        InsertCreatureInOrder(BattleGrid[row, col], row, col);

        return true;
    }

    private void InsertCreatureInOrder(BattleSlot creature, int row, int col)
    {
        int index = GetInsertIndex(row, col);
        if (index >= FieldCreatures.Count)
        {
            FieldCreatures.Add(creature);
        }
        else
        {
            FieldCreatures.Insert(index, creature);
        }
    }

    private int GetInsertIndex(int row, int col)
    {
        // Calculate the base index based on the column
        int baseIndex = col * BATTLE_ROWS;

        // Add the row offset within the column
        return baseIndex + row;
    }

    public void RemoveCreature(BattleSlot slot)
    {
        FieldCreatures.Remove(slot);
        slot.ClearSlot();
    }
    public BattleSlot GetCreature(int row, int col)
    {
        if (row >= BATTLE_ROWS || row < 0) return null;
        if (col >= BATTLE_COLS || col < 0) return null;

        return BattleGrid[row, col];
    }

    public List<BattleSlot> GetMeleeTargets(BattleSlot creature)
    {
        var targets = new List<BattleSlot>();

        int row = creature.Row;
        int col = creature.Col;

        // Add direct neighbors (up, down, left, right)
        if (row > 0) targets.Add(BattleGrid[row - 1, col]); // Up
        if (row < BATTLE_ROWS - 1) targets.Add(BattleGrid[row + 1, col]); // Down
        if (col > 0) targets.Add(BattleGrid[row, col - 1]); // Left
        if (col < BATTLE_COLS - 1) targets.Add(BattleGrid[row, col + 1]); // Right

        // Add diagonal neighbors
        if (row > 0 && col > 0) targets.Add(BattleGrid[row - 1, col - 1]); // Up-Left
        if (row > 0 && col < BATTLE_COLS - 1) targets.Add(BattleGrid[row - 1, col + 1]); // Up-Right
        if (row < BATTLE_ROWS - 1 && col > 0) targets.Add(BattleGrid[row + 1, col - 1]); // Down-Left
        if (row < BATTLE_ROWS - 1 && col < BATTLE_COLS - 1) targets.Add(BattleGrid[row + 1, col + 1]); // Down-Right

        // Add targets 2 spaces away in straight lines if the space in between is empty
        if (row > 1 && BattleGrid[row - 1, col].IsEmpty) targets.Add(BattleGrid[row - 2, col]); // Two Up
        if (row < BATTLE_ROWS - 2 && BattleGrid[row + 1, col].IsEmpty) targets.Add(BattleGrid[row + 2, col]); // Two Down
        if (col > 1 && BattleGrid[row, col - 1].IsEmpty) targets.Add(BattleGrid[row, col - 2]); // Two Left
        if (col < BATTLE_COLS - 2 && BattleGrid[row, col + 1].IsEmpty) targets.Add(BattleGrid[row, col + 2]); // Two Right

        // Add "behind diagonal" targets if the diagonal space is empty
        if (row > 0 && col > 0 && BattleGrid[row - 1, col - 1].IsEmpty && col > 1)
            targets.Add(BattleGrid[row - 1, col - 2]); // Behind Up-Left
        if (row > 0 && col < BATTLE_COLS - 1 && BattleGrid[row - 1, col + 1].IsEmpty && col < BATTLE_COLS - 2)
            targets.Add(BattleGrid[row - 1, col + 2]); // Behind Up-Right
        if (row < BATTLE_ROWS - 1 && col > 0 && BattleGrid[row + 1, col - 1].IsEmpty && col > 1)
            targets.Add(BattleGrid[row + 1, col - 2]); // Behind Down-Left
        if (row < BATTLE_ROWS - 1 && col < BATTLE_COLS - 1 && BattleGrid[row + 1, col + 1].IsEmpty && col < BATTLE_COLS - 2)
            targets.Add(BattleGrid[row + 1, col + 2]); // Behind Down-Right

        // Filter out empty targets and return the list
        return targets.Where(t => !t.IsEmpty).ToList();
    }

    public List<BattleSlot> GetRangedTargets(BattleSlot creature)
    {
        int row = creature.Row;
        int col = creature.Col;

        // Start with all grid spaces
        var targets = BattleGrid.Cast<BattleSlot>().ToList();

        // If the creature is in a corner, remove the opposing corner
        if (IsCorner(row, col))
        {
            int opposingRow = row == 0 ? BATTLE_ROWS - 1 : 0;
            int opposingCol = col == 0 ? BATTLE_COLS - 1 : 0;
            targets.Remove(BattleGrid[opposingRow, opposingCol]);
        }

        // Filter out empty targets and return the list
        return targets.Where(t => !t.IsEmpty).ToList();
    }

    // Helper to check if the current position is a corner
    private bool IsCorner(int row, int col)
    {
        return (row == 0 || row == BATTLE_ROWS - 1) &&
               (col == 0 || col == BATTLE_COLS - 1);
    }
}
