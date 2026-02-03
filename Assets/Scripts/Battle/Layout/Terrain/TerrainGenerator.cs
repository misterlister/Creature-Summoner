using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates procedural terrain layouts.
/// </summary>
public static class TerrainGenerator
{
    private const float INCREASED_MODIFIER = 1.5f; // multiplier for increased focused terrain generation
    private const float DECREASED_MODIFIER = 0.8f; // multiplier for decreased focused terrain generation
    private const int LARGE_INCREASE_VALUE = 10; // large increase in weight for focused terrain generation
    private const int SMALL_INCREASE_VALUE = 5; // small increase in weight for focused terrain generation
    private const int DECREASE_VALUE = 5; // decrease in weight for non-focused terrain generation

    // --- PUBLIC GENERATION METHODS ---

    public static TerrainLayout GenerateByPattern(
        TerrainGenerationPattern pattern,
        float density = 0.4f,
        bool enableChasms = true)
    {
        switch (pattern)
        {
            case TerrainGenerationPattern.Default:
                return GenerateRandom(density, enableChasms);
            case TerrainGenerationPattern.Mirrored:
                return GenerateMirrored(density, enableChasms);
            case TerrainGenerationPattern.FrontlineFocus:
                return GenerateLineFocus(density, enableChasms, frontline: true);
            case TerrainGenerationPattern.BacklineFocus:
                return GenerateLineFocus(density, enableChasms, frontline: false);
            case TerrainGenerationPattern.CoverFocus:
                return GenerateFocus(density, enableChasms, TerrainFocusCategory.Cover);
            case TerrainGenerationPattern.RoughFocus:
                return GenerateFocus(density, enableChasms, TerrainFocusCategory.Rough);
            case TerrainGenerationPattern.LightFocus:
                return GenerateFocus(density, enableChasms, TerrainFocusCategory.Light);
            case TerrainGenerationPattern.HeavyFocus:
                return GenerateFocus(density, enableChasms, TerrainFocusCategory.Heavy);
            case TerrainGenerationPattern.Clustered:
                return GenerateClustered(density, enableChasms);
            case TerrainGenerationPattern.Constricted:
                return GenerateConstricted(density, enableChasms);
            default:
                return GenerateRandom(density, enableChasms);
        }
    }

    // --- RANDOM GENERATION ---

    private static TerrainLayout GenerateRandom(float density, bool enableChasms)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        var weighted = BuildDefaultWeightedList(enableChasms);

        for (int col = 0; col < GameConstants.BATTLE_COLS; col++)
        {
            GenerateRandomColumn(layout, col, density, weighted);
        }

        return layout;
    }

    private static TerrainLayout GenerateMirrored(float density, bool enableChasms)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        var weighted = BuildDefaultWeightedList(enableChasms);

        // Generate left side (player side)
        for (int col = 0; col <= 2; col++)
        {
            GenerateRandomColumn(layout, col, density, weighted);
        }

        // Mirror to enemy side (flip y-axis)
        for (int col = 0; col <= 2; col++)
        {
            int mirroredCol = 5 - col; // mirror player 0->5, 1->4, 2->3
            for (int row = 0; row < GameConstants.BATTLE_ROWS; row++)
            {
                var mirroredRow = GameConstants.BATTLE_ROWS - 1 - row;
                layout.SetTerrain(mirroredRow, mirroredCol, layout.GetTerrainType(row, col));
            }
        }

        return layout;
    }

    private static TerrainLayout GenerateLineFocus(float density, bool enableChasms, bool frontline)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        var weighted = BuildDefaultWeightedList(enableChasms);

        for (int col = 0; col < GameConstants.BATTLE_COLS; col++)
        {
            // Apply density multiplier based on column position
            float colDensity = density;
            if (frontline)
            {
                // Increase density towards frontline (col 2/3), decrease towards backline (col 0/5)
                colDensity *= col switch
                {
                    5 => DECREASED_MODIFIER,
                    4 => 1f,
                    3 => INCREASED_MODIFIER,
                    2 => INCREASED_MODIFIER,
                    1 => 1f,
                    0 => DECREASED_MODIFIER,
                    _ => 1f
                };
            }
            else
            {
                // Increase density towards backline (col 0/5), decrease towards frontline (col 2/3)
                colDensity *= col switch
                {
                    0 => INCREASED_MODIFIER,
                    1 => 1f,
                    2 => DECREASED_MODIFIER,
                    3 => DECREASED_MODIFIER,
                    4 => 1f,
                    5 => INCREASED_MODIFIER,
                    _ => 1f
                };
            }

            GenerateRandomColumn(layout, col, colDensity, weighted);
        }

        return layout;
    }

    private static TerrainLayout GenerateFocus(float density, bool enableChasms, TerrainFocusCategory category)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        var weighted = BuildFocusWeightedList(category, enableChasms);

        for (int col = 0; col < GameConstants.BATTLE_COLS; col++)
        {
            GenerateRandomColumn(layout, col, density, weighted);
        }

        return layout;
    }

    private static TerrainLayout GenerateClustered(float density, bool enableChasms)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        var weighted = BuildDefaultWeightedList(enableChasms);

        var used = new bool[GameConstants.BATTLE_ROWS, GameConstants.BATTLE_COLS];
        int targetTiles = Mathf.RoundToInt(GameConstants.BATTLE_ROWS * GameConstants.BATTLE_COLS * density);
        int placedTiles = 0;

        while (placedTiles < targetTiles)
        {
            int row = Random.Range(0, GameConstants.BATTLE_ROWS);
            int col = Random.Range(0, GameConstants.BATTLE_COLS);

            if (used[row, col]) continue;

            var terrain = weighted[Random.Range(0, weighted.Count)];
            layout.SetTerrain(row, col, terrain);
            used[row, col] = true;
            placedTiles++;

            // Attempt small clusters around this tile
            foreach (var pos in GetAdjacentPositions(row, col))
            {
                if (placedTiles >= targetTiles) break;
                if (!used[pos.row, pos.col] && Random.value < 0.5f)
                {
                    layout.SetTerrain(pos.row, pos.col, terrain);
                    used[pos.row, pos.col] = true;
                    placedTiles++;
                }
            }
        }

        return layout;
    }

    private static TerrainLayout GenerateConstricted(float density, bool enableChasms)
    {
        var layout = ScriptableObject.CreateInstance<TerrainLayout>();
        var weighted = BuildDefaultWeightedList(enableChasms);

        for (int col = 0; col < GameConstants.BATTLE_COLS; col++)
        {
            GenerateRandomColumn(layout, col, density, weighted, allowConstrictedBlocking: true);
        }

        return layout;
    }

    // --- COLUMN HELPER ---

    private static void GenerateRandomColumn(
        TerrainLayout layout,
        int col,
        float density,
        List<TerrainTypeEnum> weightedTerrains,
        bool allowChasms = true,
        bool allowConstrictedBlocking = false)
    {
        TerrainTypeEnum? lastBlocking = null;
        int blockingCountThisRow = 0;

        for (int row = 0; row < GameConstants.BATTLE_ROWS; row++)
        {
            if (Random.value >= density)
            {
                layout.SetTerrain(row, col, TerrainTypeEnum.Regular);
                continue;
            }

            TerrainTypeEnum chosen;
            int attempt = 0;

            do
            {
                chosen = weightedTerrains[Random.Range(0, weightedTerrains.Count)];

                // chasm toggle
                if (!allowChasms && chosen == TerrainTypeEnum.Chasm)
                    chosen = TerrainTypeEnum.Regular;

                attempt++;
                if (attempt > 10) break; // prevent infinite loop
            }
            while (IsBlocking(chosen) && !CanPlaceBlockingTile(row, lastBlocking, blockingCountThisRow, allowConstrictedBlocking));

            // update tracking
            if (IsBlocking(chosen))
            {
                lastBlocking = chosen;
                blockingCountThisRow++;
            }
            else
            {
                lastBlocking = null;
            }

            layout.SetTerrain(row, col, chosen);
        }
    }

    private static bool IsBlocking(TerrainTypeEnum t) => t == TerrainTypeEnum.HeavyCover || t == TerrainTypeEnum.Chasm;

    private static bool CanPlaceBlockingTile(int row, TerrainTypeEnum? lastBlocking, int blockingCountThisRow, bool allowConstricted)
    {
        if (allowConstricted)
        {
            return blockingCountThisRow < 2;
        }
        else
        {
            return lastBlocking == null;
        }
    }

    // --- WEIGHTED LISTS ---

    private static List<TerrainTypeEnum> BuildDefaultWeightedList(bool enableChasms)
    {
        // Default: Regular most common, light cover/rough slightly more frequent than heavy
        var weights = new Dictionary<TerrainTypeEnum, int>
        {
            { TerrainTypeEnum.Regular, 40 },
            { TerrainTypeEnum.LightCover, 15 },
            { TerrainTypeEnum.HeavyCover, 10 },
            { TerrainTypeEnum.LightRough, 15 },
            { TerrainTypeEnum.HeavyRough, 10 },
        };

        if (enableChasms) weights[TerrainTypeEnum.Chasm] = 10;

        return BuildWeightedList(weights);
    }

    private static List<TerrainTypeEnum> BuildFocusWeightedList(TerrainFocusCategory category, bool enableChasms)
    {
        var weights = new Dictionary<TerrainTypeEnum, int>
        {
            { TerrainTypeEnum.Regular, 30 },
            { TerrainTypeEnum.LightCover, 15 },
            { TerrainTypeEnum.HeavyCover, 10 },
            { TerrainTypeEnum.LightRough, 15 },
            { TerrainTypeEnum.HeavyRough, 10 }
        };

        switch (category)
        {
            case TerrainFocusCategory.Cover:
                weights[TerrainTypeEnum.LightCover] += LARGE_INCREASE_VALUE;
                weights[TerrainTypeEnum.HeavyCover] += SMALL_INCREASE_VALUE;
                weights[TerrainTypeEnum.LightRough] -= DECREASE_VALUE;
                weights[TerrainTypeEnum.HeavyRough] -= DECREASE_VALUE;
                break;
            case TerrainFocusCategory.Rough:
                weights[TerrainTypeEnum.LightRough] += LARGE_INCREASE_VALUE;
                weights[TerrainTypeEnum.HeavyRough] += SMALL_INCREASE_VALUE;
                weights[TerrainTypeEnum.LightCover] -= DECREASE_VALUE;
                weights[TerrainTypeEnum.HeavyCover] -= DECREASE_VALUE;
                break;
            case TerrainFocusCategory.Light:
                weights[TerrainTypeEnum.LightCover] += LARGE_INCREASE_VALUE;
                weights[TerrainTypeEnum.LightRough] += LARGE_INCREASE_VALUE;
                weights[TerrainTypeEnum.HeavyCover] -= DECREASE_VALUE;
                weights[TerrainTypeEnum.HeavyRough] -= DECREASE_VALUE;
                break;
            case TerrainFocusCategory.Heavy:
                weights[TerrainTypeEnum.HeavyCover] += LARGE_INCREASE_VALUE;
                weights[TerrainTypeEnum.HeavyRough] += LARGE_INCREASE_VALUE;
                weights[TerrainTypeEnum.LightCover] -= DECREASE_VALUE;
                weights[TerrainTypeEnum.LightRough] -= DECREASE_VALUE;
                break;
        }

        if (enableChasms) weights[TerrainTypeEnum.Chasm] = 10;

        return BuildWeightedList(weights);
    }

    private static List<TerrainTypeEnum> BuildWeightedList(Dictionary<TerrainTypeEnum, int> weights)
    {
        var list = new List<TerrainTypeEnum>();
        foreach (var kv in weights)
        {
            int value = Mathf.Max(0, kv.Value); // prevent negative weights
            for (int i = 0; i < value; i++)
            {
                list.Add(kv.Key);
            }
        }
        return list;
    }

    // --- UTILITY ---

    private struct Pos { public int row; public int col; }

    private static IEnumerable<Pos> GetAdjacentPositions(int row, int col)
    {
        var offsets = new (int r, int c)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
        foreach (var (r, c) in offsets)
        {
            int nr = row + r, nc = col + c;
            if (nr >= 0 && nr < GameConstants.BATTLE_ROWS && nc >= 0 && nc < GameConstants.BATTLE_COLS)
                yield return new Pos { row = nr, col = nc };
        }
    }
}
