using System.Collections.Generic;
using static BattleSystemConstants;

/// <summary>
/// Static utility class for calculating Area of Effect targets.
/// </summary>
public static class AOETargetCalculator
{
    // Define AOE patterns as relative offsets from the primary target
    public static TargetList GetTargets(
    BattleTile primaryTile,
    AOE aoeType,
    BattleGrid grid,
    TeamSide teamSide)
    {
        if (!AOEPatterns.TryGetValue(aoeType, out var pattern))
            return new TargetList(primaryTile, new(), new());

        var center = primaryTile.BattlefieldPosition;
        int colDirection = teamSide == TeamSide.Player ? 1 : -1;

        return new TargetList(
            primaryTile,
            ResolveOffsets(pattern.SecondaryOffsets, center, colDirection, grid),
            ResolveOffsets(pattern.TertiaryOffsets, center, colDirection, grid)
        );
    }

    // Helper method to resolve relative offsets into actual BattleTiles
    private static List<BattleTile> ResolveOffsets(
        (int row, int col)[] offsets,
        BattlePosition center,
        int colDirection,
        BattleGrid grid)
    {
        var targets = new List<BattleTile>();

        foreach (var (row, col) in offsets)
        {
            var tile = grid.GetTile(center.Row + row, center.LocalCol + (col * colDirection));
            if (tile != null)
                targets.Add(tile);
        }

        return targets;
    }
}

public readonly struct TargetList
{
    public readonly BattleTile PrimaryTarget;
    public readonly List<BattleTile> SecondaryTargets;
    public readonly List<BattleTile> TertiaryTargets;
    public static TargetList Empty => new TargetList(null, new List<BattleTile>(), new List<BattleTile>());

    public TargetList(BattleTile primary, List<BattleTile> secondary, List<BattleTile> tertiary)
    {
        PrimaryTarget = primary;
        SecondaryTargets = secondary;
        TertiaryTargets = tertiary;
    }

    public List<BattleTile> AllTargets()
    {
        var all = new List<BattleTile> { PrimaryTarget };
        all.AddRange(SecondaryTargets);
        all.AddRange(TertiaryTargets);
        return all;
    }
}