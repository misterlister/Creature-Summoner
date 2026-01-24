
[System.Serializable]
public class PositionedCreature
{
    public CreatureConfig Config;
    public GridPosition Position; // (row, col) within team grid

    public PositionedCreature(CreatureConfig config, int row, int col)
    {
        Config = config;
        Position = new GridPosition(row, col);
    }
}