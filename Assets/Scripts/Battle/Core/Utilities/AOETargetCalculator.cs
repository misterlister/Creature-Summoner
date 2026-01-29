using System;
using System.Collections.Generic;
using static GameConstants;

/// <summary>
/// Static utility class for calculating Area of Effect targets.
/// Works with BattlePosition for unified battlefield targeting.
/// </summary>
public static class AOETargetCalculator
{
    public static List<BattleTile> GetTargets(
        BattlePosition center,
        AOE aoeType,
        UnifiedBattlefield battlefield,
        bool isPlayerAttacker,
        int yChoice = 0)
    {
        return aoeType switch
        {
            AOE.Single => new List<BattleTile>(),
            AOE.SmallLine => GetLineTargets(center, 2, battlefield, isPlayerAttacker),
            AOE.LargeLine => GetLineTargets(center, 3, battlefield, isPlayerAttacker),
            AOE.FullLine => GetLineTargets(center, 6, battlefield, isPlayerAttacker), // Can extend across both grids
            AOE.SmallArc => GetArcTargets(center, 2, battlefield, yChoice),
            AOE.WideArc => GetArcTargets(center, 3, battlefield, yChoice),
            AOE.FullArc => GetArcTargets(center, BATTLE_ROWS, battlefield, yChoice),
            AOE.SmallCone => GetConeTargets(center, 2, 1, battlefield, isPlayerAttacker, yChoice),
            AOE.MediumCone => GetConeTargets(center, 3, 1, battlefield, isPlayerAttacker, yChoice),
            AOE.LargeCone => GetConeTargets(center, 5, 2, battlefield, isPlayerAttacker, yChoice),
            AOE.SmallBurst => GetBurstTargets(center, 1, battlefield),
            AOE.LargeBurst => GetBurstTargets(center, 2, battlefield),
            _ => new List<BattleTile>()
        };
    }

    private static List<BattleTile> GetLineTargets(
        BattlePosition center,
        int length,
        UnifiedBattlefield battlefield,
        bool isPlayerAttacker)
    {
        var targets = new List<BattleTile>();
        int colDirection = isPlayerAttacker ? 1 : -1;

        for (int i = 1; i < length; i++)
        {
            int newGlobalCol = center.GlobalCol + (colDirection * i);
            var pos = BattlePosition.TryCreate(center.Row, newGlobalCol);

            if (pos.HasValue)
            {
                var tile = battlefield.GetTile(pos.Value);
                if (tile != null)
                {
                    targets.Add(tile);
                }
            }
        }

        return targets;
    }

    private static List<BattleTile> GetArcTargets(
        BattlePosition center,
        int width,
        UnifiedBattlefield battlefield,
        int yChoice)
    {
        var targets = new List<BattleTile>();
        int halfWidth = width / 2;

        // Adjust starting offset for even widths based on yChoice
        int startOffset = (width % 2 == 0 && yChoice == 1)
            ? -halfWidth + 1
            : -halfWidth;

        for (int i = 0; i < width; i++)
        {
            int offset = startOffset + i;
            if (offset == 0) continue; // Skip center row

            int targetRow = center.Row + offset;
            var pos = BattlePosition.TryCreate(targetRow, center.GlobalCol);

            if (pos.HasValue)
            {
                var tile = battlefield.GetTile(pos.Value);
                if (tile != null)
                {
                    targets.Add(tile);
                }
            }
        }

        return targets;
    }

    private static List<BattleTile> GetConeTargets(
        BattlePosition center,
        int maxWidth,
        int depth,
        UnifiedBattlefield battlefield,
        bool isPlayerAttacker,
        int yChoice)
    {
        var targets = new List<BattleTile>();
        int colDirection = isPlayerAttacker ? 1 : -1;

        for (int d = 1; d <= depth; d++)
        {
            int targetGlobalCol = center.GlobalCol + (colDirection * d);

            // Check if column is valid
            if (targetGlobalCol < 0 || targetGlobalCol >= 6)
                continue;

            // Calculate width at this depth
            int widthAtDepth = depth == 0 ? 1 : 1 + (d * (maxWidth - 1)) / depth;

            // Get row positions at this depth
            var rowPositions = GetConeRowPositions(center.Row, widthAtDepth, yChoice);

            foreach (int row in rowPositions)
            {
                var pos = BattlePosition.TryCreate(row, targetGlobalCol);
                if (pos.HasValue)
                {
                    var tile = battlefield.GetTile(pos.Value);
                    if (tile != null)
                    {
                        targets.Add(tile);
                    }
                }
            }
        }

        return targets;
    }

    private static List<int> GetConeRowPositions(int centerRow, int width, int yChoice)
    {
        var positions = new List<int>();

        if (width % 2 == 0)
        {
            // Even width: use yChoice to offset
            int halfWidth = width / 2;
            int startOffset = (yChoice == 0) ? -halfWidth : -halfWidth + 1;

            for (int i = 0; i < width; i++)
            {
                int row = centerRow + startOffset + i;
                if (row >= 0 && row < BATTLE_ROWS)
                {
                    positions.Add(row);
                }
            }
        }
        else
        {
            // Odd width: centered around centerRow
            int halfWidth = width / 2;
            for (int offset = -halfWidth; offset <= halfWidth; offset++)
            {
                int row = centerRow + offset;
                if (row >= 0 && row < BATTLE_ROWS)
                {
                    positions.Add(row);
                }
            }
        }

        return positions;
    }

    private static List<BattleTile> GetBurstTargets(
        BattlePosition center,
        int radius,
        UnifiedBattlefield battlefield)
    {
        var targets = new List<BattleTile>();

        for (int dRow = -radius; dRow <= radius; dRow++)
        {
            for (int dCol = -radius; dCol <= radius; dCol++)
            {
                // Skip center
                if (dRow == 0 && dCol == 0) continue;

                // Use Manhattan distance for diamond pattern
                if (Math.Abs(dRow) + Math.Abs(dCol) <= radius)
                {
                    int newRow = center.Row + dRow;
                    int newGlobalCol = center.GlobalCol + dCol;

                    var pos = BattlePosition.TryCreate(newRow, newGlobalCol);
                    if (pos.HasValue)
                    {
                        var tile = battlefield.GetTile(pos.Value);
                        if (tile != null)
                        {
                            targets.Add(tile);
                        }
                    }
                }
            }
        }

        return targets;
    }
}