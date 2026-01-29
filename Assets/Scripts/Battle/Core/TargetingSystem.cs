using System;
using System.Collections.Generic;
using System.Linq;
using static GameConstants;

/// <summary>
/// Handles all targeting calculations for the unified battlefield.
/// Implements same-grid and across-grid targeting rules.
/// </summary>
public class TargetingSystem
{
    private UnifiedBattlefield battlefield;

    public TargetingSystem(UnifiedBattlefield battlefield)
    {
        this.battlefield = battlefield;
    }

    /// <summary>
    /// Get all valid targets for an action, checking both same-grid and across-grid rules.
    /// Returns union of both sets (a target valid under either rule is included).
    /// </summary>
    public List<BattleTile> GetAllValidTargets(Creature attacker, ActionRange range)
    {
        var attackerPos = battlefield.GetBattlePosition(attacker);
        if (!attackerPos.IsValid())
            return new List<BattleTile>();

        var sameGridTargets = GetSameGridTargets(attackerPos, range);
        var acrossGridTargets = GetAcrossGridTargets(attackerPos, range);

        // Union - add all same-grid targets, then add across-grid targets that aren't already included
        var allTargets = new List<BattleTile>(sameGridTargets);
        foreach (var target in acrossGridTargets)
        {
            if (!allTargets.Contains(target))
            {
                allTargets.Add(target);
            }
        }

        return allTargets;
    }

    #region Same-Grid Targeting

    /// <summary>
    /// Get valid targets within the same grid as the attacker.
    /// Range is space-based, units do not block.
    /// </summary>
    private List<BattleTile> GetSameGridTargets(BattlePosition attacker, ActionRange range)
    {
        var targets = new List<BattleTile>();
        var grid = battlefield.GetGrid(attacker.GetTeamSide());

        foreach (var tile in grid.GetAllTiles())
        {
            var targetPos = battlefield.GetBattlePosition(tile);

            if (IsValidSameGridTarget(attacker, targetPos, range))
            {
                targets.Add(tile);
            }
        }

        return targets;
    }

    private bool IsValidSameGridTarget(BattlePosition attacker, BattlePosition target, ActionRange range)
    {
        // Must be same team (self counts as same team)
        if (attacker.GetTeamSide() != target.GetTeamSide())
            return false;

        return range switch
        {
            ActionRange.Self => attacker == target, // Only self
            ActionRange.Melee => attacker == target || IsMeleeRangeSameGrid(attacker, target),
            ActionRange.Short => attacker == target || IsShortRangeSameGrid(attacker, target),
            ActionRange.Long => true, // All allies including self
            _ => false
        };
    }

    private bool IsMeleeRangeSameGrid(BattlePosition from, BattlePosition to)
    {
        // Adjacent including diagonals
        return from.IsAdjacentTo(to);
    }

    private bool IsShortRangeSameGrid(BattlePosition from, BattlePosition to)
    {
        // Within 2 spaces, but NOT two spaces diagonally
        int distance = from.ManhattanDistance(to);
        if (distance > 2)
            return false;

        // Check if it's exactly 2 spaces diagonally (which is not allowed)
        if (Math.Abs(from.Row - to.Row) == 2 && Math.Abs(from.GlobalCol - to.GlobalCol) == 2)
            return false;

        return true;
    }

    #endregion

    #region Across-Grid Targeting

    /// <summary>
    /// Get valid targets in the opposite grid.
    /// Range is blocker-based, units in front block targets behind them.
    /// Targets must be in same row or within 1 row.
    /// </summary>
    private List<BattleTile> GetAcrossGridTargets(BattlePosition attacker, ActionRange range)
    {
        var targets = new List<BattleTile>();
        var oppositeGrid = battlefield.GetGrid(
            attacker.GetTeamSide() == TeamSide.Player ? TeamSide.Enemy : TeamSide.Player
        );

        foreach (var tile in oppositeGrid.GetAllTiles())
        {
            var targetPos = battlefield.GetBattlePosition(tile);

            if (IsValidAcrossGridTarget(attacker, targetPos, range))
            {
                targets.Add(tile);
            }
        }

        return targets;
    }

    private bool IsValidAcrossGridTarget(BattlePosition attacker, BattlePosition target, ActionRange range)
    {
        // Self range doesn't work across grids
        if (range == ActionRange.Self)
            return false;

        // Must be in same row or within 1 row
        if (!attacker.IsInTargetableRow(target))
            return false;

        // Count all blockers between attacker and target (including friendlies!)
        int blockers = CountBlockersInPath(attacker, target);

        return range switch
        {
            ActionRange.Melee => IsValidMeleeAcrossGrid(attacker, target, blockers),
            ActionRange.Short => IsValidShortAcrossGrid(attacker, target, blockers),
            ActionRange.Long => IsValidLongAcrossGrid(attacker, target, blockers),
            _ => false
        };
    }

    private bool IsValidMeleeAcrossGrid(BattlePosition attacker, BattlePosition target, int blockers)
    {
        // Cannot pass over any units
        if (blockers > 0)
            return false;

        // Must be the closest unit in that row (or adjacent rows if within 1 row)
        if (!IsClosestInRow(attacker, target))
            return false;

        // Check column reach limits based on attacker's position
        int maxReachableCol = GetMeleeMaxReachableColumn(attacker);

        return target.GlobalCol <= maxReachableCol;
    }

    private int GetMeleeMaxReachableColumn(BattlePosition attacker)
    {
        // Player perspective: col 0 = back, col 1 = mid, col 2 = front
        // Enemy perspective: col 0 = front, col 1 = mid, col 2 = back

        if (attacker.GetTeamSide() == TeamSide.Player)
        {
            return attacker.LocalCol switch
            {
                0 => 3, // Back -> enemy front (col 3)
                1 => 4, // Mid -> enemy mid (col 4)
                2 => 5, // Front -> enemy back (col 5)
                _ => 3
            };
        }
        else // Enemy
        {
            return attacker.LocalCol switch
            {
                0 => 2, // Front (enemy col 0 = global 3) -> player front (col 2)
                1 => 1, // Mid -> player mid (col 1)
                2 => 0, // Back -> player back (col 0)
                _ => 2
            };
        }
    }

    private bool IsValidShortAcrossGrid(BattlePosition attacker, BattlePosition target, int blockers)
    {
        // Can pass over 1-2 units
        if (blockers > 2)
            return false;

        // Cannot target opposing backline from own backline (max 4 column distance)
        int colDistance = attacker.ColumnDistance(target);
        return colDistance <= 4;
    }

    private bool IsValidLongAcrossGrid(BattlePosition attacker, BattlePosition target, int blockers)
    {
        // Can pass over 3-4 units
        return blockers <= 4;
    }

    private int CountBlockersInPath(BattlePosition from, BattlePosition to)
    {
        // Count blockers in the TARGET's row, from next column after attacker to just before target
        // This applies for same row or within 1 row targeting

        int minCol = Math.Min(from.GlobalCol, to.GlobalCol);
        int maxCol = Math.Max(from.GlobalCol, to.GlobalCol);

        int blockerCount = 0;

        // Count from the column after attacker, up to (but not including) target's column
        // Always count along the target's row
        for (int col = minCol + 1; col < maxCol; col++)
        {
            var checkPos = new BattlePosition(to.Row, col);
            var tile = battlefield.GetTile(checkPos);

            if (tile != null && tile.IsOccupied)
            {
                blockerCount++;
            }
        }

        return blockerCount;
    }

    private bool IsClosestInRow(BattlePosition attacker, BattlePosition target)
    {
        // Get all enemies in the TARGET's row (not attacker's row)
        var enemiesInRow = battlefield.GetCreaturesInRow(target.Row)
            .Where(c => c.TeamSide != attacker.GetTeamSide())
            .ToList();

        if (enemiesInRow.Count == 0)
            return false;

        // Target must be the closest or tied for closest in that row
        int targetDistance = attacker.ColumnDistance(target);
        return enemiesInRow.All(e =>
        {
            var enemyPos = battlefield.GetBattlePosition(e);
            return attacker.ColumnDistance(enemyPos) >= targetDistance;
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get valid targets for a specific action, filtered by target type.
    /// </summary>
    public List<BattleTile> GetValidTargetsForAction(Creature attacker, ActionBase action)
    {
        if (attacker == null || action == null)
            return new List<BattleTile>();

        // Get all positionally valid targets
        var potentialTargets = GetAllValidTargets(attacker, action.Range);

        // Filter by target type (enemy/ally/self/empty)
        return FilterByTargetType(potentialTargets, attacker, action.ValidTargets);
    }

    private List<BattleTile> FilterByTargetType(
        List<BattleTile> tiles,
        Creature attacker,
        TargetType targetType)
    {
        var filtered = new List<BattleTile>();

        foreach (var tile in tiles)
        {
            if (IsValidTargetType(tile, attacker, targetType))
            {
                filtered.Add(tile);
            }
        }

        return filtered;
    }

    private bool IsValidTargetType(BattleTile tile, Creature attacker, TargetType targetType)
    {
        switch (targetType)
        {
            case TargetType.Enemy:
                return tile.IsOccupied && tile.OccupyingCreature.TeamSide != attacker.TeamSide;

            case TargetType.Ally:
                return tile.IsOccupied && tile.OccupyingCreature.TeamSide == attacker.TeamSide && tile.OccupyingCreature != attacker;

            case TargetType.Self:
                return tile.IsOccupied && tile.OccupyingCreature == attacker;

            case TargetType.AllyIncludingSelf:
                return tile.IsOccupied && tile.OccupyingCreature.TeamSide == attacker.TeamSide;

            case TargetType.EmptySpace:
                return tile.IsEmpty;

            case TargetType.OccupiedSpace:
                return tile.IsOccupied;

            case TargetType.Any:
                return true;

            default:
                return false;
        }
    }

    #endregion
}
