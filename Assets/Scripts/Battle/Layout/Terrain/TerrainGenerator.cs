using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates procedural terrain layouts based on biomes and patterns.
/// </summary>
public static class TerrainGenerator
{
    /// <summary>
    /// Get all terrain type enums valid for a specific biome
    /// </summary>
    public static List<TerrainTypeEnum> GetTerrainTypesForBiome(Biome biome)
    {
        if (biome == null) return new List<TerrainTypeEnum>();

        var validTerrains = new List<TerrainTypeEnum>();
        var allTypes = System.Enum.GetValues(typeof(TerrainTypeEnum));

        foreach (TerrainTypeEnum terrainEnum in allTypes)
        {
            var terrainInstance = terrainEnum.GetTerrainInstance();
            if (terrainInstance != null && biome.IsTerrainValid(terrainInstance.GetType()))
            {
                validTerrains.Add(terrainEnum);
            }
        }

        return validTerrains;
    }

    /// <summary>
    /// Generate random terrain for a biome
    /// </summary>
    public static TerrainLayout GenerateForBiome(Biome biome, float density = 0.4f)
    {
        var terrainTypes = GetTerrainTypesForBiome(biome);
        if (terrainTypes.Count == 0)
        {
            Debug.LogWarning($"No terrain types found for biome {biome?.BiomeName}");
            return null;
        }

        return GenerateRandom(terrainTypes, density, biome);
    }

    /// <summary>
    /// Generate completely random terrain layout
    /// </summary>
    public static TerrainLayout GenerateRandom(List<TerrainTypeEnum> availableTerrains, float density = 0.4f, Biome biome = null)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        if (biome != null)
        {
            layout.LayoutBiome = biome;
        }

        for (int col = 0; col < GameConstants.BATTLE_COLS; col++)
        {
            for (int row = 0; row < GameConstants.BATTLE_ROWS; row++)
            {
                if (Random.value < density)
                {
                    var terrain = availableTerrains[Random.Range(0, availableTerrains.Count)];
                    layout.SetTerrain(new GridPosition(row, col), terrain);
                }
            }
        }

        return layout;
    }

    /// <summary>
    /// Generate symmetric terrain (mirrored top-to-bottom)
    /// </summary>
    public static TerrainLayout GenerateSymmetric(List<TerrainTypeEnum> availableTerrains, float density = 0.4f, Biome biome = null)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        if (biome != null)
        {
            layout.LayoutBiome = biome;
        }

        for (int col = 0; col < GameConstants.BATTLE_COLS; col++)
        {
            // Only generate for rows 0 and 1
            for (int row = 0; row < 2; row++)
            {
                if (Random.value < density)
                {
                    var terrain = availableTerrains[Random.Range(0, availableTerrains.Count)];
                    layout.SetTerrain(new GridPosition(row, col), terrain);

                    // Mirror to bottom
                    if (row == 0)
                    {
                        layout.SetTerrain(new GridPosition(2, col), terrain);
                    }
                }
            }
        }

        return layout;
    }

    /// <summary>
    /// Generate terrain with more density in frontline
    /// </summary>
    public static TerrainLayout GenerateFrontlineHeavy(List<TerrainTypeEnum> availableTerrains, Biome biome = null)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        if (biome != null)
        {
            layout.LayoutBiome = biome;
        }

        for (int col = 0; col < GameConstants.BATTLE_COLS; col++)
        {
            // Density decreases as we move back
            float density = col switch
            {
                0 => 0.7f,  // Front
                1 => 0.4f,  // Mid
                2 => 0.2f,  // Back
                _ => 0.3f
            };

            for (int row = 0; row < GameConstants.BATTLE_ROWS; row++)
            {
                if (Random.value < density)
                {
                    var terrain = availableTerrains[Random.Range(0, availableTerrains.Count)];
                    layout.SetTerrain(new GridPosition(row, col), terrain);
                }
            }
        }

        return layout;
    }

    /// <summary>
    /// Generate terrain with more density in backline
    /// </summary>
    public static TerrainLayout GenerateBacklineHeavy(List<TerrainTypeEnum> availableTerrains, Biome biome = null)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        if (biome != null)
        {
            layout.LayoutBiome = biome;
        }

        for (int col = 0; col < GameConstants.BATTLE_COLS; col++)
        {
            // Density increases as we move back
            float density = col switch
            {
                0 => 0.2f,  // Front
                1 => 0.4f,  // Mid
                2 => 0.7f,  // Back
                _ => 0.3f
            };

            for (int row = 0; row < GameConstants.BATTLE_ROWS; row++)
            {
                if (Random.value < density)
                {
                    var terrain = availableTerrains[Random.Range(0, availableTerrains.Count)];
                    layout.SetTerrain(new GridPosition(row, col), terrain);
                }
            }
        }

        return layout;
    }

    /// <summary>
    /// Generate clustered terrain (groups of same terrain type)
    /// </summary>
    public static TerrainLayout GenerateClustered(List<TerrainTypeEnum> availableTerrains, float density = 0.4f, Biome biome = null)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        if (biome != null)
        {
            layout.LayoutBiome = biome;
        }

        var used = new bool[GameConstants.BATTLE_ROWS, GameConstants.BATTLE_COLS];

        int targetTiles = Mathf.RoundToInt(9 * density);
        int placedTiles = 0;

        while (placedTiles < targetTiles)
        {
            // Pick random starting position
            int startRow = Random.Range(0, GameConstants.BATTLE_ROWS);
            int startCol = Random.Range(0, GameConstants.BATTLE_COLS);

            if (used[startRow, startCol]) continue;

            // Pick terrain for this cluster
            var terrain = availableTerrains[Random.Range(0, availableTerrains.Count)];

            // Create cluster (1-3 tiles)
            int clusterSize = Random.Range(1, 4);
            var cluster = new List<GridPosition> { new GridPosition(startRow, startCol) };

            // Try to expand cluster
            for (int i = 1; i < clusterSize && cluster.Count < clusterSize; i++)
            {
                var lastPos = cluster[cluster.Count - 1];
                var neighbors = GetAdjacentPositions(lastPos);

                foreach (var neighbor in neighbors)
                {
                    if (!used[neighbor.Row, neighbor.Col])
                    {
                        cluster.Add(neighbor);
                        break;
                    }
                }
            }

            // Place cluster
            foreach (var pos in cluster)
            {
                layout.SetTerrain(pos, terrain);
                used[pos.Row, pos.Col] = true;
                placedTiles++;
            }
        }

        return layout;
    }

    private static List<GridPosition> GetAdjacentPositions(GridPosition pos)
    {
        var adjacent = new List<GridPosition>();

        var offsets = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };

        foreach (var (rowOffset, colOffset) in offsets)
        {
            var newPos = new GridPosition(pos.Row + rowOffset, pos.Col + colOffset);
            if (newPos.IsValid())
            {
                adjacent.Add(newPos);
            }
        }

        return adjacent;
    }

    /// <summary>
    /// Generate based on pattern type
    /// </summary>
    public static TerrainLayout GenerateByPattern(
        TerrainGenerationPattern pattern,
        List<TerrainTypeEnum> availableTerrains,
        float density = 0.4f,
        Biome biome = null)
    {
        return pattern switch
        {
            TerrainGenerationPattern.Random => GenerateRandom(availableTerrains, density, biome),
            TerrainGenerationPattern.Symmetric => GenerateSymmetric(availableTerrains, density, biome),
            TerrainGenerationPattern.FrontlineHeavy => GenerateFrontlineHeavy(availableTerrains, biome),
            TerrainGenerationPattern.BacklineHeavy => GenerateBacklineHeavy(availableTerrains, biome),
            TerrainGenerationPattern.Clustered => GenerateClustered(availableTerrains, density, biome),
            TerrainGenerationPattern.Scattered => GenerateRandom(availableTerrains, density * 0.5f, biome),
            _ => GenerateRandom(availableTerrains, density, biome)
        };
    }
}