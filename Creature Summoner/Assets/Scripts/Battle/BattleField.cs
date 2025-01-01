using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static GameConstants;
using static UnityEngine.GraphicsBuffer;

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
        if (creature != null && creature.Species == null)
        {
            creature = null;
        }
        if (row >= BATTLE_ROWS || col >= BATTLE_COLS) return false;

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
        if (creature != null)
        {
            InsertCreatureInOrder(BattleGrid[row, col], row, col);
        }

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

    public void GetMeleeTargets(BattleSlot creature, HighlightAuraState highlightAura)
    {
        int row = creature.Row;
        int col = creature.Col;

        // Define the relative offsets for the 8 neighboring cells (4 cardinal + 4 diagonal)
        (int, int)[] directions = {
            (-1, 0), (1, 0),  // Up, Down
            (0, -1), (0, 1),  // Left, Right
            (-1, -1), (-1, 1), // Up-Left, Up-Right
            (1, -1), (1, 1)   // Down-Left, Down-Right
        };

        // Check all neighbors (direct and diagonal)
        foreach (var (dRow, dCol) in directions)
        {
            int newRow = row + dRow;
            int newCol = col + dCol;

            // Skip invalid positions (out of bounds) and empty spaces directly
            if (newRow < 0 || newRow >= BATTLE_ROWS || newCol < 0 || newCol >= BATTLE_COLS || BattleGrid[newRow, newCol].IsEmpty)
                continue;

            // Update aura for valid, non-empty neighbors
            BattleGrid[newRow, newCol].UpdateHighlightAura(highlightAura);
        }

        // Check "two spaces" away in straight lines (up, down, left, right)
        CheckTwoSpaces(row, col, highlightAura);

        // Check "behind diagonal" targets if space in between is empty
        CheckBehindDiagonals(row, col, highlightAura);
    }

    private void CheckTwoSpaces(int row, int col, HighlightAuraState highlightAura)
    {
        // Two spaces away (only if the intermediate space is empty and the target is non-empty)
        if (row > 1 && BattleGrid[row - 1, col].IsEmpty && !BattleGrid[row - 2, col].IsEmpty) // Two Up
            BattleGrid[row - 2, col].UpdateHighlightAura(highlightAura);

        if (row < BATTLE_ROWS - 2 && BattleGrid[row + 1, col].IsEmpty && !BattleGrid[row + 2, col].IsEmpty) // Two Down
            BattleGrid[row + 2, col].UpdateHighlightAura(highlightAura);

        if (col > 1 && BattleGrid[row, col - 1].IsEmpty && !BattleGrid[row, col - 2].IsEmpty) // Two Left
            BattleGrid[row, col - 2].UpdateHighlightAura(highlightAura);

        if (col < BATTLE_COLS - 2 && BattleGrid[row, col + 1].IsEmpty && !BattleGrid[row, col + 2].IsEmpty) // Two Right
            BattleGrid[row, col + 2].UpdateHighlightAura(highlightAura);
    }


    private void CheckBehindDiagonals(int row, int col, HighlightAuraState highlightAura)
    {
        // Behind diagonal targets (check if space in between is empty and target is non-empty)
        if (row > 1 && col > 1 && BattleGrid[row - 1, col - 1].IsEmpty && !BattleGrid[row - 2, col - 2].IsEmpty) // Behind Up-Left
            BattleGrid[row - 2, col - 2].UpdateHighlightAura(highlightAura);

        if (row > 1 && col < BATTLE_COLS - 2 && BattleGrid[row - 1, col + 1].IsEmpty && !BattleGrid[row - 2, col + 2].IsEmpty) // Behind Up-Right
            BattleGrid[row - 2, col + 2].UpdateHighlightAura(highlightAura);

        if (row < BATTLE_ROWS - 2 && col > 1 && BattleGrid[row + 1, col - 1].IsEmpty && !BattleGrid[row + 2, col - 2].IsEmpty) // Behind Down-Left
            BattleGrid[row + 2, col - 2].UpdateHighlightAura(highlightAura);

        if (row < BATTLE_ROWS - 2 && col < BATTLE_COLS - 2 && BattleGrid[row + 1, col + 1].IsEmpty && !BattleGrid[row + 2, col + 2].IsEmpty) // Behind Down-Right
            BattleGrid[row + 2, col + 2].UpdateHighlightAura(highlightAura);
    }



    public void GetRangedTargets(BattleSlot creature, HighlightAuraState highlightAura)
    {
        int row = creature.Row;
        int col = creature.Col;

        // Calculate the opposite diagonal coordinates once
        var oppositeDiagonal = GetOppositeDiagonal(row, col);

        // Start with all grid spaces
        foreach (var target in BattleGrid)
        {
            // Skip the current creature, empty spaces
            if (target == creature || target.IsEmpty)
            {
                continue;
            }

            // Only skip the opposite diagonal if it exists
            if (oppositeDiagonal != null && target == oppositeDiagonal)
            {
                continue;
            }

            // Update aura for valid, non-empty target positions
            target.UpdateHighlightAura(highlightAura);
        }
    }

    // Helper to get the creature on the opposite diagonal (if any)
    private BattleSlot GetOppositeDiagonal(int row, int col)
    {
        if (row == 0 && col == 0)
            return BattleGrid[BATTLE_ROWS - 1, BATTLE_COLS - 1]; // Top-left to bottom-right
        else if (row == 0 && col == BATTLE_COLS - 1)
            return BattleGrid[BATTLE_ROWS - 1, 0]; // Top-right to bottom-left
        else if (row == BATTLE_ROWS - 1 && col == 0)
            return BattleGrid[0, BATTLE_COLS - 1]; // Bottom-left to top-right
        else if (row == BATTLE_ROWS - 1 && col == BATTLE_COLS - 1)
            return BattleGrid[0, 0]; // Bottom-right to top-left

        return null; // No opposite diagonal for non-corner positions
    }

    public void ResetTargetHighlights(BattleSlot activeCreature = null)
    {
        foreach (var creature in FieldCreatures) {
            if (creature == activeCreature)
            {
                creature.UpdateHighlightAura(HighlightAuraState.Active);
            }
            else
            {
                creature.UpdateHighlightAura(HighlightAuraState.None);
            }
        }
    }

    public List<BattleSlot> GetAOETargets(BattleSlot target, ActionBase action, int yChoice, bool isPlayer)
    {
        List<BattleSlot> result = new List<BattleSlot>();
        if (target == null || action == null)
        {
            return result;
        }
        HighlightAuraState highlightAura = action.Offensive ? HighlightAuraState.Negative : HighlightAuraState.Positive;
        int targetRow = target.Row;
        int targetCol = target.Col;

        result.Add(target);
        target.UpdateHighlightAura(highlightAura);

        switch (action.AreaOfEffect)
        {
            case AOE.Single:
                break;
            case AOE.Line:
                HandleLineAOE(result, targetRow, targetCol, highlightAura, isPlayer);
                break;
            case AOE.SmallArc:
                HandleArcAOE(result, targetRow, targetCol, highlightAura, width: 2, yChoice);
                break;
            case AOE.WideArc:
                HandleArcAOE(result, targetRow, targetCol, highlightAura, width: 3, yChoice);
                break;
            case AOE.SmallCone:
                HandleConeAOE(result, targetRow, targetCol, highlightAura, width: 2, yChoice, isPlayer);
                break;
            case AOE.LargeCone:
                HandleConeAOE(result, targetRow, targetCol, highlightAura, width: 3, yChoice, isPlayer);
                break;
            case AOE.Square:
                HandleRectAOE(result, targetRow, targetCol, highlightAura, width: 2, yChoice, isPlayer);
                break;
            case AOE.Field:
                HandleRectAOE(result, targetRow, targetCol, highlightAura, width: 3, yChoice, isPlayer);
                break;
            case AOE.Burst:
                HandleBurstAOE(result, targetRow, targetCol, highlightAura);
                break;
            default:
                break;
        }
        return result;
    }

    private void HandleLineAOE(List<BattleSlot> result, int row, int col, HighlightAuraState highlightAura, bool isPlayer)
    {
        int newCol = isPlayer ? col + 1 : col - 1;
        if (newCol >= 0 && newCol < BATTLE_COLS)
        {
            BattleSlot slot = BattleGrid[row, newCol];
            result.Add(slot);
            slot.UpdateHighlightAura(highlightAura);
        }
    }

    private void HandleArcAOE(List<BattleSlot> result, int row, int col, HighlightAuraState highlightAura, int width, int yChoice)
    {
        // Select the row above (if within bounds)
        if (width == 3 || yChoice == 0)
        {
            if (row - 1 >= 0) // Ensure row above is within bounds
            {
                BattleSlot slot = BattleGrid[row - 1, col];
                result.Add(slot);
                slot.UpdateHighlightAura(highlightAura);
            }
        }
        if (width == 3 || yChoice == 1)
        {
            // Select the row below (if within bounds)
            if (row + 1 < BATTLE_ROWS) // Ensure row below is within bounds
            {
                BattleSlot slot = BattleGrid[row + 1, col];
                result.Add(slot);
                slot.UpdateHighlightAura(highlightAura);
            }
        }
    }

    private void HandleConeAOE(List<BattleSlot> result, int row, int col, HighlightAuraState highlightAura, int width, int yChoice, bool isPlayer)
    {
        // Calculate the column offset based on whether it's the player or the enemy
        int colOffset = isPlayer ? 1 : -1;
        // Ensure the column is within bounds
        if (col + colOffset >= 0 && col + colOffset < BATTLE_COLS) 
        {
            // Select the space behind the target (same row, 1 column over)
            BattleSlot slotBehind = BattleGrid[row, col + colOffset];
            result.Add(slotBehind);
            slotBehind.UpdateHighlightAura(highlightAura);

            // Select the space above (if within bounds)
            if (row - 1 >= 0) // Ensure row below is within bounds
            {
                if (width == 3 || yChoice == 0)
                {
                    BattleSlot slotAbove = BattleGrid[row - 1, col + colOffset];
                    result.Add(slotAbove);
                    slotAbove.UpdateHighlightAura(highlightAura);
                }
            }
            // Select the space below (if within bounds)
            if (row + 1 < BATTLE_ROWS) // Ensure row below is within bounds
            {
                if (width == 3 || yChoice == 1)
                {
                    BattleSlot slotBelow = BattleGrid[row + 1, col + colOffset];
                    result.Add(slotBelow);
                    slotBelow.UpdateHighlightAura(highlightAura);
                }
            }
        }
    }

    private void HandleRectAOE(List<BattleSlot> result, int row, int col, HighlightAuraState highlightAura, int width, int yChoice, bool isPlayer)
    {
        // Calculate the column offset based on whether it's the player or the enemy
        int colOffset = isPlayer ? 1 : -1;
        if (row - 1 >= 0) // Ensure row above is within bounds
        {
            if (width == 3 || yChoice == 0)
            {
                // Select the space above the target (same column, 1 row above)
                BattleSlot slotAbove = BattleGrid[row - 1, col];
                result.Add(slotAbove);
                slotAbove.UpdateHighlightAura(highlightAura);
            }
        }

        if (row + 1 < BATTLE_ROWS) // Ensure row below is within bounds
        {
            if (width == 3 || yChoice == 1)
            {
                // Select the space below the target (same column, 1 row below)
                BattleSlot slotBelow = BattleGrid[row + 1, col];
                result.Add(slotBelow);
                slotBelow.UpdateHighlightAura(highlightAura);
            }
        }
        // Ensure the column behind is within bounds
        if (col + colOffset >= 0 && col + colOffset < BATTLE_COLS)
        {
            // Select the space behind the target (same row, 1 column over)
            BattleSlot slotBehind = BattleGrid[row, col + colOffset];
            result.Add(slotBehind);
            slotBehind.UpdateHighlightAura(highlightAura);

            // Select the space above (if within bounds)
            if (row - 1 >= 0) // Ensure row below is within bounds
            {
                if (width == 3 || yChoice == 0)
                {
                    BattleSlot slotAbove = BattleGrid[row - 1, col + colOffset];
                    result.Add(slotAbove);
                    slotAbove.UpdateHighlightAura(highlightAura);
                }
            }

            // Select the space below (if within bounds)
            if (row + 1 < BATTLE_ROWS) // Ensure row below is within bounds
            {
                if (width == 3 || yChoice == 1)
                {
                    BattleSlot slotBelow = BattleGrid[row + 1, col + colOffset];
                    result.Add(slotBelow);
                    slotBelow.UpdateHighlightAura(highlightAura);
                }
            }

        }
    }

    private void HandleBurstAOE(List<BattleSlot> result, int row, int col, HighlightAuraState highlightAura)
    {
        // Ensure row below is within bounds
        if (row - 1 >= 0)
        {
            // Select the space above the target (same column, 1 row above)
            BattleSlot slotAbove = BattleGrid[row - 1, col];
            result.Add(slotAbove);
            slotAbove.UpdateHighlightAura(highlightAura);
        }
        // Ensure row below is within bounds
        if (row + 1 < BATTLE_ROWS) 
        {
            // Select the space below the target (same column, 1 row below)
            BattleSlot slotBelow = BattleGrid[row + 1, col];
            result.Add(slotBelow);
            slotBelow.UpdateHighlightAura(highlightAura);
        }
        // Ensure the column behind is within bounds
        if (col - 1 >= 0)
        {
            // Select the space behind the target (same row, 1 column back)
            BattleSlot slotBehind = BattleGrid[row, col - 1];
            result.Add(slotBehind);
            slotBehind.UpdateHighlightAura(highlightAura);
        }
        // Ensure the column in front is within bounds
        if (col + 1 < BATTLE_COLS)
        {
            // Select the space in front of the target (same row, 1 column in front)
            BattleSlot slotAhead = BattleGrid[row, col + 1];
            result.Add(slotAhead);
            slotAhead.UpdateHighlightAura(highlightAura);
        }
    }
}
