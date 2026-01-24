using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

/// <summary>
/// Defines a 3x3 terrain layout for a battle grid.
/// Stores terrain types (mechanics) - visuals come from Biome.
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

    [Header("Biome")]
    [Tooltip("What biome is this layout for?")]
    public Biome LayoutBiome;

    [Header("Grid Layout (Left to Right on Battlefield)")]
    [Tooltip("Front Line (Col 0) - Closest to enemy")]
    [SerializeField] private GridColumn frontLine = new();

    [Tooltip("Middle Line (Col 1)")]
    [SerializeField] private GridColumn middleLine = new();

    [Tooltip("Back Line (Col 2) - Furthest from enemy")]
    [SerializeField] private GridColumn backLine = new();

    // Get terrain enum at specific position
    public TerrainTypeEnum GetTerrainType(GridPosition pos)
    {
        if (!pos.IsValid()) return TerrainTypeEnum.Regular;

        return pos.Col switch
        {
            0 => frontLine.GetRow(pos.Row),
            1 => middleLine.GetRow(pos.Row),
            2 => backLine.GetRow(pos.Row),
            _ => TerrainTypeEnum.Regular
        };
    }

    public TerrainTypeEnum GetTerrainType(int row, int col)
    {
        var pos = GridPosition.TryCreate(row, col);
        return pos.HasValue ? GetTerrainType(pos.Value) : TerrainTypeEnum.Regular;
    }

    // Set terrain at specific position
    public void SetTerrain(GridPosition pos, TerrainTypeEnum terrain)
    {
        if (!pos.IsValid())
        {
            Debug.LogError($"Cannot set terrain at invalid position {pos}");
            return;
        }

        switch (pos.Col)
        {
            case 0: frontLine.SetRow(pos.Row, terrain); break;
            case 1: middleLine.SetRow(pos.Row, terrain); break;
            case 2: backLine.SetRow(pos.Row, terrain); break;
        }
    }

    // Get all terrain slots as a flat list
    public List<TerrainSlot> GetAllSlots()
    {
        var slots = new List<TerrainSlot>();

        for (int col = 0; col < BATTLE_COLS; col++)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                var pos = new GridPosition(row, col);
                var terrainType = GetTerrainType(pos);

                slots.Add(new TerrainSlot
                {
                    Position = pos,
                    TerrainType = terrainType
                });
            }
        }

        return slots;
    }

    // Check if layout is all regular terrain
    public bool IsAllRegular()
    {
        for (int col = 0; col < BATTLE_COLS; col++)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                if (GetTerrainType(row, col) != TerrainTypeEnum.Regular)
                    return false;
            }
        }
        return true;
    }

    // Clear all terrain (reset to regular)
    public void Clear()
    {
        for (int col = 0; col < BATTLE_COLS; col++)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                SetTerrain(new GridPosition(row, col), TerrainTypeEnum.Regular);
            }
        }
    }

    private void OnValidate()
    {
        if (LayoutBiome == null) return;

        // Check all terrain types are valid for the biome
        for (int col = 0; col < BATTLE_COLS; col++)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                var terrainType = GetTerrainType(row, col);
                var terrainInstance = terrainType.GetTerrainInstance();

                if (terrainInstance != null && !LayoutBiome.IsTerrainValid(terrainInstance.GetType()))
                {
                    Debug.LogWarning(
                        $"Terrain '{terrainType}' at ({row},{col}) is not valid for biome '{LayoutBiome.BiomeName}'",
                        this);
                }
            }
        }
    }

    [System.Serializable]
    public class TerrainSlot
    {
        public GridPosition Position;
        public TerrainTypeEnum TerrainType;
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
            TerrainTypeEnum.Water => Terrains.Water,
            TerrainTypeEnum.Lava => Terrains.Lava,
            TerrainTypeEnum.Chasm => Terrains.Chasm,
            _ => null
        };
    }

    public static TerrainTypeEnum FromTerrainType(TerrainType terrain)
    {
        if (terrain == null) return TerrainTypeEnum.Regular;

        return terrain switch
        {
            RegularTerrain => TerrainTypeEnum.Regular,
            LightCoverTerrain => TerrainTypeEnum.LightCover,
            HeavyCoverTerrain => TerrainTypeEnum.HeavyCover,
            LightRoughTerrain => TerrainTypeEnum.LightRough,
            HeavyRoughTerrain => TerrainTypeEnum.HeavyRough,
            WaterTerrain => TerrainTypeEnum.Water,
            LavaTerrain => TerrainTypeEnum.Lava,
            ChasmTerrain => TerrainTypeEnum.Chasm,
            _ => TerrainTypeEnum.Regular
        };
    }
}