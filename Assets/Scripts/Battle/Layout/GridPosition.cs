using System;
using static GameConstants;

/// <summary>
/// Represents a position on a 3x3 battle grid.
/// Row: 0-2 (Top/Middle/Bottom)
/// Col: 0-2 (Front/Middle/Back line)
/// </summary>
public struct GridPosition : IEquatable<GridPosition>
{
    public int Row { get; }
    public int Col { get; }

    public GridPosition(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public bool IsValid() => Row >= 0 && Row < BATTLE_ROWS && Col >= 0 && Col < BATTLE_COLS;

    public static GridPosition? TryCreate(int row, int col)
    {
        var pos = new GridPosition(row, col);
        return pos.IsValid() ? pos : null;
    }

    // Column (depth) queries
    public bool IsFrontline() => Col == 0;
    public bool IsMidline() => Col == 1;
    public bool IsBackline() => Col == 2;

    // Row (vertical) queries
    public bool IsTop() => Row == 0;
    public bool IsMiddle() => Row == 1;
    public bool IsBottom() => Row == 2;

    // Directional movement
    public GridPosition? GetAdjacentPosition(Direction direction)
    {
        var (rowOffset, colOffset) = direction switch
        {
            Direction.Up => (-1, 0),
            Direction.Down => (1, 0),
            Direction.Left => (0, -1),
            Direction.Right => (0, 1),
            _ => (0, 0)
        };

        return TryCreate(Row + rowOffset, Col + colOffset);
    }

    // Distance calculations
    public int ManhattanDistance(GridPosition other)
    {
        return Math.Abs(Row - other.Row) + Math.Abs(Col - other.Col);
    }

    public int ChebyshevDistance(GridPosition other)
    {
        return Math.Max(Math.Abs(Row - other.Row), Math.Abs(Col - other.Col));
    }

    // Adjacency
    public bool IsAdjacentTo(GridPosition other)
    {
        return ManhattanDistance(other) == 1;
    }

    public bool IsDiagonalTo(GridPosition other)
    {
        return Math.Abs(Row - other.Row) == 1 && Math.Abs(Col - other.Col) == 1;
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

        string colName = Col switch
        {
            0 => "Front Line",
            1 => "Middle Line",
            2 => "Back Line",
            _ => "Unknown"
        };

        return $"{rowName} {colName} ({Row},{Col})";
    }

    public override string ToString() => $"({Row},{Col})";

    // Equality
    public bool Equals(GridPosition other) => Row == other.Row && Col == other.Col;
    public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Row, Col);

    public static bool operator ==(GridPosition left, GridPosition right) => left.Equals(right);
    public static bool operator !=(GridPosition left, GridPosition right) => !left.Equals(right);
}