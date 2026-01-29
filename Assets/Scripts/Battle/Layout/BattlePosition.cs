using System;
using static GameConstants;

/// <summary>
/// Represents a position on the unified 3×6 battlefield.
/// Row: 0-2 (shared across both grids)
/// GlobalCol: 0-5 (Player: 0-2, Enemy: 3-5)
/// Player frontline = col 2, Enemy frontline = col 3 (adjacent!)
/// </summary>
public struct BattlePosition : IEquatable<BattlePosition>
{
    public int Row { get; }
    public int GlobalCol { get; }

    public BattlePosition(int row, int globalCol)
    {
        Row = row;
        GlobalCol = globalCol;
    }

    // Validation
    public bool IsValid() => Row >= 0 && Row < BATTLE_ROWS && GlobalCol >= 0 && GlobalCol < (BATTLE_COLS * 2);

    public static BattlePosition? TryCreate(int row, int globalCol)
    {
        var pos = new BattlePosition(row, globalCol);
        return pos.IsValid() ? pos : null;
    }

    // Team and grid queries
    public TeamSide GetTeamSide() => GlobalCol <= 2 ? TeamSide.Player : TeamSide.Enemy;

    public int LocalCol => GlobalCol <= 2 ? GlobalCol : GlobalCol - 3;

    public GridPosition ToLocalGridPosition() => new GridPosition(Row, LocalCol);

    // Create from local grid position
    public static BattlePosition FromGridPosition(GridPosition gridPos, TeamSide team)
    {
        int globalCol = team == TeamSide.Player ? gridPos.Col : gridPos.Col + 3;
        return new BattlePosition(gridPos.Row, globalCol);
    }

    // Column (depth) queries - relative to team
    public bool IsFrontline()
    {
        return GetTeamSide() == TeamSide.Player ? LocalCol == 2 : LocalCol == 0;
    }

    public bool IsMidline() => LocalCol == 1;

    public bool IsBackline()
    {
        return GetTeamSide() == TeamSide.Player ? LocalCol == 0 : LocalCol == 2;
    }

    // Row (vertical) queries
    public bool IsTop() => Row == 0;
    public bool IsMiddle() => Row == 1;
    public bool IsBottom() => Row == 2;

    // Distance calculations
    public int ManhattanDistance(BattlePosition other)
    {
        return Math.Abs(Row - other.Row) + Math.Abs(GlobalCol - other.GlobalCol);
    }

    public int ChebyshevDistance(BattlePosition other)
    {
        return Math.Max(Math.Abs(Row - other.Row), Math.Abs(GlobalCol - other.GlobalCol));
    }

    public int ColumnDistance(BattlePosition other)
    {
        return Math.Abs(GlobalCol - other.GlobalCol);
    }

    public int RowDistance(BattlePosition other)
    {
        return Math.Abs(Row - other.Row);
    }

    // Adjacency - works across grids!
    public bool IsAdjacentTo(BattlePosition other)
    {
        int rowDist = Math.Abs(Row - other.Row);
        int colDist = Math.Abs(GlobalCol - other.GlobalCol);
        return rowDist <= 1 && colDist <= 1 && !(rowDist == 0 && colDist == 0);
    }

    public bool IsCardinallyAdjacentTo(BattlePosition other)
    {
        int rowDist = Math.Abs(Row - other.Row);
        int colDist = Math.Abs(GlobalCol - other.GlobalCol);
        return (rowDist == 1 && colDist == 0) || (rowDist == 0 && colDist == 1);
    }

    public bool IsDiagonallyAdjacentTo(BattlePosition other)
    {
        int rowDist = Math.Abs(Row - other.Row);
        int colDist = Math.Abs(GlobalCol - other.GlobalCol);
        return rowDist == 1 && colDist == 1;
    }

    // Same row or ±1 row (for ranged targeting rules)
    public bool IsInTargetableRow(BattlePosition other)
    {
        return Math.Abs(Row - other.Row) <= 1;
    }

    // Directional movement (within same grid only)
    public BattlePosition? GetAdjacentPosition(Direction direction)
    {
        var (rowOffset, colOffset) = direction switch
        {
            Direction.Up => (-1, 0),
            Direction.Down => (1, 0),
            Direction.Left => (0, -1),
            Direction.Right => (0, 1),
            _ => (0, 0)
        };

        return TryCreate(Row + rowOffset, GlobalCol + colOffset);
    }

    // Get position one column toward enemy
    public BattlePosition? GetForwardPosition()
    {
        int forwardCol = GetTeamSide() == TeamSide.Player ? GlobalCol + 1 : GlobalCol - 1;
        return TryCreate(Row, forwardCol);
    }

    // Get position one column toward backline
    public BattlePosition? GetBackwardPosition()
    {
        int backCol = GetTeamSide() == TeamSide.Player ? GlobalCol - 1 : GlobalCol + 1;
        return TryCreate(Row, backCol);
    }

    // String representation
    public string ToPositionDescription()
    {
        string rowName = Row switch
        {
            0 => "Top",
            1 => "Middle",
            2 => "Bottom",
            _ => "Unknown"
        };

        string colName = LocalCol switch
        {
            0 => GetTeamSide() == TeamSide.Player ? "Back Line" : "Front Line",
            1 => "Middle Line",
            2 => GetTeamSide() == TeamSide.Player ? "Front Line" : "Back Line",
            _ => "Unknown"
        };

        string team = GetTeamSide() == TeamSide.Player ? "Player" : "Enemy";

        return $"{team} - {rowName} {colName} (R{Row},GC{GlobalCol})";
    }

    public override string ToString() => $"(R{Row},GC{GlobalCol})";

    // Equality
    public bool Equals(BattlePosition other) => Row == other.Row && GlobalCol == other.GlobalCol;
    public override bool Equals(object obj) => obj is BattlePosition other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Row, GlobalCol);
    public static bool operator ==(BattlePosition left, BattlePosition right) => left.Equals(right);
    public static bool operator !=(BattlePosition left, BattlePosition right) => !left.Equals(right);
}