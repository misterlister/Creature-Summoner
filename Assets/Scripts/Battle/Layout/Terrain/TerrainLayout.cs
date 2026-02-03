using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

/// <summary>
/// Defines a 6x6 terrain layout for a player/enemy pair of battle grids.
/// Stores terrain types (mechanics) - visuals differ based on Biome.
/// </summary>
[CreateAssetMenu(menuName = "Battle/Create Terrain Layout")]
public class TerrainLayout : ScriptableObject
{
    [System.Serializable]
    public class GridColumn
    {
        [Tooltip("Top position in this column")]
        public TerrainTypeEnum Top = TerrainTypeEnum.Regular;

        [Tooltip("Middle position in this column")]
        public TerrainTypeEnum Middle = TerrainTypeEnum.Regular;

        [Tooltip("Bottom position in this column")]
        public TerrainTypeEnum Bottom = TerrainTypeEnum.Regular;

        public TerrainTypeEnum GetRow(int row)
        {
            return row switch
            {
                0 => Top,
                1 => Middle,
                2 => Bottom,
                _ => TerrainTypeEnum.Regular
            };
        }

        public void SetRow(int row, TerrainTypeEnum terrain)
        {
            switch (row)
            {
                case 0: Top = terrain; break;
                case 1: Middle = terrain; break;
                case 2: Bottom = terrain; break;
            }
        }
    }
    [Header("Battlefield Columns (0–5, left to right)")]
    [SerializeField]
    private List<GridColumn> columns = new();

    private void OnEnable()
    {
        EnsureColumnCount();
    }

    private void OnValidate()
    {
        EnsureColumnCount();
    }

    private void EnsureColumnCount()
    {
        while (columns.Count < BATTLE_COLS)
            columns.Add(new GridColumn());

        while (columns.Count > BATTLE_COLS)
            columns.RemoveAt(columns.Count - 1);
    }

    public TerrainTypeEnum GetTerrainType(int row, int col)
    {
        if (row < 0 || row >= BATTLE_ROWS || col < 0 || col >= BATTLE_COLS)
            return TerrainTypeEnum.Regular;

        return columns[col].GetRow(row);
    }

    public void SetTerrain(int row, int col, TerrainTypeEnum terrain)
    {
        if (row < 0 || row >= BATTLE_ROWS || col < 0 || col >= BATTLE_COLS)
            return;

        columns[col].SetRow(row, terrain);
    }

    public void Clear()
    {
        for (int col = 0; col < BATTLE_COLS; col++)
            for (int row = 0; row < BATTLE_ROWS; row++)
                SetTerrain(row, col, TerrainTypeEnum.Regular);
    }
}

/// <summary>
/// Extension methods for TerrainTypeEnum
/// </summary>
public static class TerrainTypeExtensions
{
    public static TerrainType GetTerrainInstance(this TerrainTypeEnum terrainType)
    {
        return terrainType switch
        {
            TerrainTypeEnum.Regular => Terrains.Regular,
            TerrainTypeEnum.LightCover => Terrains.LightCover,
            TerrainTypeEnum.HeavyCover => Terrains.HeavyCover,
            TerrainTypeEnum.LightRough => Terrains.LightRough,
            TerrainTypeEnum.HeavyRough => Terrains.HeavyRough,
            TerrainTypeEnum.Chasm => Terrains.Chasm,
            _ => null
        };
    }
}