using System;
using System.Collections.Generic;
using System.Linq;
using static GameConstants;

/// <summary>
/// Handles all targeting calculations for the unified 3×6 battlefield.
/// 
/// Offensive (Melee/Reach/Distant): always targets enemies across the grid divide.
///   - Scans along the target's row from the grid boundary inward.
///   - Melee: first enemy only, zero pass-overs allowed.
///   - Reach: first or second enemy, up to 2 pass-overs.
///   - Distant: any enemy in row/adjacent row, ignores blockers.
/// 
/// Support (Touch/Team): always targets allies on the same (player) grid.
///   - Self: only self.
///   - Touch: cardinally adjacent allies only.
///   - Team: any ally anywhere on the grid.
/// 
/// Row adjacency rule:
///   - Top (row 0): can target rows 0 and 1 only.
///   - Middle (row 1): can target rows 0, 1, and 2.
///   - Bottom (row 2): can target rows 1 and 2 only.
/// </summary>
public class TargetingSystem
{
    private UnifiedBattlefield battlefield;

    public TargetingSystem(UnifiedBattlefield battlefield)
    {
        this.battlefield = battlefield;
    }

    /// <summary>
    /// Returns all valid target tiles for an action, filtered by the action's
    /// ValidTargets constraint.
    /// </summary>
    public List<BattleTile> GetValidTargetsForAction(Creature attacker, ActionBase action)
    {
        if (attacker == null || action == null)
            return new List<BattleTile>();

        bool groundTarget = action.ValidTargets == TargetType.Tile;

        return action.Range switch
        {
            ActionRange.Melee => GetOffensiveTargets(attacker, maxPassOvers: 0, groundTarget),
            ActionRange.Reach => GetOffensiveTargets(attacker, maxPassOvers: 2, groundTarget),
            ActionRange.Distant => GetDistantTargets(attacker, groundTarget),
            ActionRange.Touch => GetTouchTargets(attacker, action.ValidTargets),
            ActionRange.Team => GetTeamTargets(attacker, action.ValidTargets),
            ActionRange.Self => GetSelfTarget(attacker),
            _ => new List<BattleTile>()
        };
    }
    // -------------------------------------------------------------------------
    // Offensive targeting (cross-grid, enemies only)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Melee and Reach: for each targetable row, scan along that row from the
    /// grid boundary toward the enemy backline and collect enemies up to the
    /// pass-over limit.
    /// </summary>
    private List<BattleTile> GetOffensiveTargets(Creature attacker, int maxPassOvers, bool groundTarget)
    {
        var attackerPos = battlefield.GetBattlePosition(attacker);
        if (!attackerPos.IsValid()) return new List<BattleTile>();

        var results = new List<BattleTile>();

        foreach (int targetRow in GetTargetableRows(attackerPos.Row))
        {
            var targetsInRow = ScanEnemyRow(attackerPos, targetRow, maxPassOvers, groundTarget);
            results.AddRange(targetsInRow);
        }

        return results;
    }

    /// <summary>
    /// Scan one enemy row from the frontline column toward the backline.
    /// Returns occupied enemy tiles up to the pass-over limit.
    /// 
    /// "Pass-over" means a unit that is skipped to reach a further target.
    /// Melee (0 pass-overs): only the very first enemy in the row is valid.
    /// Reach (2 pass-overs): the 1st and 2nd enemies in the row are valid.
    /// </summary>
    private List<BattleTile> ScanEnemyRow(BattlePosition attackerPos, int targetRow, int maxPassOvers, bool groundTarget)
    {
        var results = new List<BattleTile>();
        TeamSide enemySide = attackerPos.GetTeamSide() == TeamSide.Player
            ? TeamSide.Enemy
            : TeamSide.Player;

        int[] colOrder = GetEnemyColumnScanOrder(enemySide);
        int targetsFound = 0;

        foreach (int globalCol in colOrder)
        {
            var pos = new BattlePosition(targetRow, globalCol);
            var tile = battlefield.GetTile(pos);
            if (tile == null) continue;

            if (!tile.IsOccupied)
            {
                // Empty tiles are valid ground targets only if no enemy has been
                // encountered yet — i.e. they lie between the attacker and the
                // first reachable enemy.
                if (groundTarget && targetsFound == 0)
                    results.Add(tile);
                continue;
            }

            // Occupied enemy tile — valid if within pass-over budget
            if (targetsFound <= maxPassOvers)
                results.Add(tile);

            targetsFound++;
            if (targetsFound > maxPassOvers) break;
        }

        return results;
    }

    /// <summary>
    /// Returns global column indices for the enemy grid in frontline-first order.
    /// </summary>
    private int[] GetEnemyColumnScanOrder(TeamSide enemySide)
    {
        // Enemy grid occupies global cols 3-5; their frontline is col 3.
        // Player grid occupies global cols 0-2; their frontline is col 2.
        return enemySide == TeamSide.Enemy
            ? new[] { 3, 4, 5 }   // Enemy frontline -> midline -> backline
            : new[] { 2, 1, 0 };  // Player frontline -> midline -> backline
    }

    /// <summary>
    /// Distant: any enemy in a targetable row, no blocker restriction.
    /// </summary>
    private List<BattleTile> GetDistantTargets(Creature attacker, bool groundTarget)
    {
        var attackerPos = battlefield.GetBattlePosition(attacker);
        if (!attackerPos.IsValid()) return new List<BattleTile>();

        var results = new List<BattleTile>();
        TeamSide enemySide = attackerPos.GetTeamSide() == TeamSide.Player
            ? TeamSide.Enemy
            : TeamSide.Player;

        var enemyGrid = battlefield.GetGrid(enemySide);

        foreach (var tile in enemyGrid.GetAllTiles())
        {
            var tilePos = tile.BattlefieldPosition;

            if (IsTargetableRow(attackerPos.Row, tilePos.Row))
            {
                if (groundTarget || tile.IsOccupied)
                {
                    results.Add(tile);
                }
            }
        }

        return results;
    }

    // -------------------------------------------------------------------------
    // Support targeting (same-grid, allies only)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Touch: cardinally adjacent ally tiles (up/down/left/right, no diagonals).
    /// Cannot skip intervening units.
    /// </summary>
    private List<BattleTile> GetTouchTargets(Creature attacker, TargetType validTargets)
    {
        var attackerPos = battlefield.GetBattlePosition(attacker);
        if (!attackerPos.IsValid()) return new List<BattleTile>();

        var results = new List<BattleTile>();

        // Cardinal directions
        (int dRow, int dCol)[] cardinals = { (-1, 0), (1, 0), (0, -1), (0, 1) };

        foreach (var (dRow, dCol) in cardinals)
        {
            int r = attackerPos.Row + dRow;
            int c = attackerPos.GlobalCol + dCol;

            var neighborPos = BattlePosition.TryCreate(r, c);
            if (!neighborPos.HasValue) continue;

            // Must stay on the same team's grid
            if (neighborPos.Value.GetTeamSide() != attackerPos.GetTeamSide()) continue;

            var tile = battlefield.GetTile(neighborPos.Value);
            if (tile == null) continue;

            // Touch targets occupied ally tiles
            if (tile.IsOccupied)
            {
                results.Add(tile);
            }
            else
            { 
                if (validTargets == TargetType.Tile)
                {
                    // If ground-targeting is allowed, empty adjacent tiles are also valid.
                    results.Add(tile);
                }
            }
        }

        if (validTargets == TargetType.Self || validTargets == TargetType.AllyOrSelf)
        {
            // If self-targeting is allowed, include the attacker's own tile as well.
            var selfTile = battlefield.GetTile(attackerPos);
            if (selfTile != null)
            {
                results.Add(selfTile);
            }
        }

        return results;
    }

    /// <summary>
    /// Team: any occupied ally tile on the attacker's grid, including self
    /// if the action's ValidTargets includes self.
    /// </summary>
    private List<BattleTile> GetTeamTargets(Creature attacker, TargetType validTargets)
    {
        var attackerPos = battlefield.GetBattlePosition(attacker);
        if (!attackerPos.IsValid()) return new List<BattleTile>();

        bool selfAllowed = validTargets == TargetType.AllyOrSelf || validTargets == TargetType.Self;
        bool groundTarget = validTargets == TargetType.Tile;
        var allyGrid = battlefield.GetGrid(attackerPos.GetTeamSide());

        return allyGrid.GetAllTiles()
            .Where(t =>
            {
                if (t.IsOccupied)
                    return t.OccupyingCreature != attacker || selfAllowed;
                else
                    return groundTarget;
            })
            .ToList();
    }

    private List<BattleTile> GetSelfTarget(Creature attacker)
    {
        var pos = battlefield.GetBattlePosition(attacker);
        var tile = battlefield.GetTile(pos);
        return tile != null ? new List<BattleTile> { tile } : new List<BattleTile>();
    }

    // -------------------------------------------------------------------------
    // Row adjacency helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the rows that an attacker in the given row can target.
    /// Top (0)    -> rows 0, 1
    /// Middle (1) -> rows 0, 1, 2
    /// Bottom (2) -> rows 1, 2
    /// </summary>
    private IEnumerable<int> GetTargetableRows(int attackerRow)
    {
        return attackerRow switch
        {
            0 => new[] { 0, 1 },
            1 => new[] { 0, 1, 2 },
            2 => new[] { 1, 2 },
            _ => Array.Empty<int>()
        };
    }

    private bool IsTargetableRow(int attackerRow, int targetRow)
    {
        return GetTargetableRows(attackerRow).Contains(targetRow);
    }
}