using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
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

    public List<Creature> FieldCreatures { get; set; } = new List<Creature>();


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
            FieldCreatures.Add(creature);
        }

        return true;
    }

    public void RemoveCreature(BattleSlot slot)
    {
        FieldCreatures.Remove(slot.Creature);
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
        int row = creature.Row;
        int col = creature.Col;

        List<BattleSlot> targets = new List<BattleSlot>();

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

            // Skip invalid positions (out of bounds) and empty spaces
            if (newRow < 0 || newRow >= BATTLE_ROWS || newCol < 0 || newCol >= BATTLE_COLS || BattleGrid[newRow, newCol].IsEmpty)
                continue;

            // Update aura for valid, non-empty neighbors
            targets.Add(BattleGrid[newRow, newCol]);
        }

        // Check "two spaces" away in straight lines (up, down, left, right)
        targets.AddRange(AddDistantMeleeTargets(row, col));

        // Check "behind diagonal" targets if space in between is empty
        targets.AddRange(AddDiagonalMeleeTargets(row, col));

        return targets;
    }

    private List<BattleSlot> AddDistantMeleeTargets(int row, int col)
    {
        List<BattleSlot> targets = new List<BattleSlot>();

        // Two spaces away (only if the intermediate space is empty and the target is non-empty)
        if (row > 1 && BattleGrid[row - 1, col].IsEmpty && !BattleGrid[row - 2, col].IsEmpty) // Two Up
            targets.Add(BattleGrid[row - 2, col]);

        if (row < BATTLE_ROWS - 2 && BattleGrid[row + 1, col].IsEmpty && !BattleGrid[row + 2, col].IsEmpty) // Two Down
            targets.Add(BattleGrid[row + 2, col]);

        if (col > 1 && BattleGrid[row, col - 1].IsEmpty && !BattleGrid[row, col - 2].IsEmpty) // Two Left
            targets.Add(BattleGrid[row, col - 2]);

        if (col < BATTLE_COLS - 2 && BattleGrid[row, col + 1].IsEmpty && !BattleGrid[row, col + 2].IsEmpty) // Two Right
            targets.Add(BattleGrid[row, col + 2]);
        return targets;
    }


    private List<BattleSlot> AddDiagonalMeleeTargets(int row, int col)
    {
        List<BattleSlot> targets = new List<BattleSlot>();

        // Behind diagonal targets (check if space in between is empty and target is non-empty)
        if (row > 1 && col > 1 && BattleGrid[row - 1, col - 1].IsEmpty && !BattleGrid[row - 2, col - 2].IsEmpty) // Behind Up-Left
            targets.Add(BattleGrid[row - 2, col - 2]);

        if (row > 1 && col < BATTLE_COLS - 2 && BattleGrid[row - 1, col + 1].IsEmpty && !BattleGrid[row - 2, col + 2].IsEmpty) // Behind Up-Right
            targets.Add(BattleGrid[row - 2, col + 2]);

        if (row < BATTLE_ROWS - 2 && col > 1 && BattleGrid[row + 1, col - 1].IsEmpty && !BattleGrid[row + 2, col - 2].IsEmpty) // Behind Down-Left
            targets.Add(BattleGrid[row + 2, col - 2]);

        if (row < BATTLE_ROWS - 2 && col < BATTLE_COLS - 2 && BattleGrid[row + 1, col + 1].IsEmpty && !BattleGrid[row + 2, col + 2].IsEmpty) // Behind Down-Right
            targets.Add(BattleGrid[row + 2, col + 2]);
        return targets;
    }



    public List<BattleSlot> GetRangedTargets(BattleSlot creature)
    {
        int row = creature.Row;
        int col = creature.Col;

        List<BattleSlot> targets = new List<BattleSlot>();

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
            targets.Add(target);
        }
        return targets;
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
        for (int row = 0; row < BATTLE_ROWS; row++) // Loop through rows
        {
            for (int col = 0; col < BATTLE_COLS; col++) // Loop through columns
            {
                BattleSlot slot = BattleGrid[row, col];
                if (slot == null)
                {
                    Debug.Log($"Error: BattleSlot Row: {row} col {col} is null");
                }

                if (activeCreature != null && slot == activeCreature)
                {
                    slot.UpdateHighlightAura(HighlightAuraState.Active);
                }
                else
                {
                    slot.UpdateHighlightAura(HighlightAuraState.None);
                }
            }
        }
    }

    public List<BattleSlot> GetMoveTargets(BattleSlot currentSlot, int distance = 1)
    {
        List<BattleSlot> targets = new List<BattleSlot>();
        if (currentSlot == null)
        {
            return targets;
        }
        int currentRow = currentSlot.Row;
        int currentCol = currentSlot.Col;

        int startCol = (currentSlot.IsPlayerSlot)? 0 : ENEMY_COL;
        int lastCol = (currentSlot.IsPlayerSlot) ? ENEMY_COL : BATTLE_COLS;

        for (int col = startCol; col < lastCol; col++)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                // Skip the current position
                if (currentRow == row && currentCol == col)
                {
                    continue;
                }

                // Check distance between current and target positions
                int distanceToTarget = Mathf.Abs(currentRow - row) + Mathf.Abs(currentCol - col);
                if (distanceToTarget <= distance)
                {
                    BattleSlot slot = BattleGrid[row, col];
                    targets.Add(slot);
                }
            }
        }
        
        return targets;
    }

    public List<BattleSlot> GetAOETargets(BattleSlot target, ActionBase action, int yChoice, bool isPlayer)
    {
        List<BattleSlot> targets = new List<BattleSlot>();
        if (target == null || action == null)
        {
            return targets;
        }
        
        int targetRow = target.Row;
        int targetCol = target.Col;

        targets.Add(target);

        switch (action.AreaOfEffect)
        {
            case AOE.Single:
                break;
            case AOE.SmallLine:
                targets.AddRange(AddLineAOETargets(targetRow, targetCol, length: 2, isPlayer));
                break;
            case AOE.LargeLine:
                targets.AddRange(AddLineAOETargets(targetRow, targetCol, length: 3, isPlayer));
                break;
            case AOE.FullLine:
                targets.AddRange(AddLineAOETargets(targetRow, targetCol, length: BATTLE_COLS, isPlayer));
                break;
            case AOE.SmallArc:
                targets.AddRange(AddArcAOETargets(targetRow, targetCol, width: 2, yChoice));
                break;
            case AOE.WideArc:
                targets.AddRange(AddArcAOETargets(targetRow, targetCol, width: 3, yChoice));
                break;
            case AOE.FullArc:
                targets.AddRange(AddArcAOETargets(targetRow, targetCol, width: BATTLE_ROWS, yChoice));
                break;
            case AOE.SmallCone:
                targets.AddRange(AddConeAOETargets(targetRow, targetCol, width: 2, depth: 1, yChoice, isPlayer));
                break;
            case AOE.MediumCone:
                targets.AddRange(AddConeAOETargets(targetRow, targetCol, width: 3, depth: 1, yChoice, isPlayer));
                break;
            case AOE.LargeCone:
                targets.AddRange(AddConeAOETargets(targetRow, targetCol, width: 5, depth: 2, yChoice, isPlayer));
                break;
            case AOE.SmallBurst:
                targets.AddRange(AddBurstAOETargets(targetRow, targetCol, size: 1));
                break;
            case AOE.LargeBurst:
                targets.AddRange(AddBurstAOETargets(targetRow, targetCol, size: 2));
                break;
            default:
                break;
        }
        return targets;
    }


    private List<BattleSlot> AddLineAOETargets(int row, int col, int length, bool isPlayer)
    {
        List<BattleSlot> targets = new List<BattleSlot>();
        for (int i = 1; i < length; i++)
        {
            int newCol = isPlayer ? col + i : col - i;
            if (newCol >= 0 && newCol < BATTLE_COLS)
            {
                BattleSlot slot = BattleGrid[row, newCol];
                targets.Add(slot);
            }
        }
        return targets;
    }

    private List<BattleSlot> AddArcAOETargets(int row, int col, int width, int yChoice)
    {
        List<BattleSlot> targets = new List<BattleSlot>();

        int halfWidth = width / 2;
        int startOffset = (width % 2 == 0 && yChoice == 1) ? -halfWidth + 1 : -halfWidth;

        for (int i = 0; i < width; i++)
        {
            int offset = startOffset + i;
            if (offset == 0) continue; // Skip target row

            int targetRow = row + offset;
            if (targetRow >= 0 && targetRow < BATTLE_ROWS)
                targets.Add(BattleGrid[targetRow, col]);
        }

        return targets;
    }

    private List<BattleSlot> AddConeAOETargets(int row, int col, int width, int depth, int yChoice, bool isPlayer)
    {
        List<BattleSlot> targets = new List<BattleSlot>();
        int colDirection = isPlayer ? 1 : -1;

        // Start at 1 to skip the original target position
        for (int d = 1; d <= depth; d++)
        {
            int targetCol = col + (colDirection * d);
            if (targetCol < 0 || targetCol >= BATTLE_COLS)
                continue;

            // Calculate width at this depth
            int widthAtDepth;
            if (depth == 0)
            {
                widthAtDepth = 1;
            }
            else
            {
                widthAtDepth = 1 + (d * (width - 1)) / depth;
            }

            // Calculate row alignment based on width
            if (widthAtDepth % 2 == 0)
            {
                // Even width: use yChoice to offset
                int halfWidth = widthAtDepth / 2;
                int startOffset = (yChoice == 0) ? -halfWidth : -halfWidth + 1;

                for (int i = 0; i < widthAtDepth; i++)
                {
                    int targetRow = row + startOffset + i;
                    if (targetRow >= 0 && targetRow < BATTLE_ROWS)
                        targets.Add(BattleGrid[targetRow, targetCol]);
                }
            }
            else
            {
                // Odd width: centered around target row
                int halfWidth = widthAtDepth / 2;
                for (int offset = -halfWidth; offset <= halfWidth; offset++)
                {
                    int targetRow = row + offset;
                    if (targetRow >= 0 && targetRow < BATTLE_ROWS)
                        targets.Add(BattleGrid[targetRow, targetCol]);
                }
            }
        }

        return targets;
    }

    private List<BattleSlot> AddBurstAOETargets(int row, int col, int size)
    {
        List<BattleSlot> targets = new List<BattleSlot>();

        // Iterate through all positions within the burst radius
        for (int dRow = -size; dRow <= size; dRow++)
        {
            for (int dCol = -size; dCol <= size; dCol++)
            {
                // Skip the original target position
                if (dRow == 0 && dCol == 0) continue;

                // Manhattan distance check to make diamond pattern
                if (Math.Abs(dRow) + Math.Abs(dCol) <= size)
                {
                    int newRow = row + dRow;
                    int newCol = col + dCol;

                    if (newRow >= 0 && newRow < BATTLE_ROWS &&
                        newCol >= 0 && newCol < BATTLE_COLS)
                    {
                        targets.Add(BattleGrid[newRow, newCol]);
                    }
                }
            }
        }

        return targets;
    }

    public void SwapSlots(BattleSlot slot1, BattleSlot slot2)
    {
        Creature creature1 = slot1.Creature;
        Creature creature2 = slot2.Creature;
        slot1.ClearSlot();
        slot2.ClearSlot();
        slot1.Setup(creature2);
        slot2.Setup(creature1);
    }
}
