using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Create Random Encounter Ruleset")]
public class RandomEncounterRules : ScriptableObject
{
    [Header("Environment")]
    [Tooltip("What biome does this encounter take place in?")]
    public Biome EncounterBiome;

    [Header("Creature Pools")]
    [Tooltip("Pool for frontline creatures")]
    public CreaturePool FrontlinePool;

    [Tooltip("Pool for midline creatures")]
    public CreaturePool MidlinePool;

    [Tooltip("Pool for backline creatures")]
    public CreaturePool BacklinePool;

    [Header("Spawn Rules")]
    [Tooltip("Minimum total creatures in encounter")]
    [Min(1)]
    public int MinTotalCreatures = 2;

    [Tooltip("Maximum total creatures in encounter")]
    [Min(1)]
    public int MaxTotalCreatures = 5;

    [Header("Position Distribution")]
    [Tooltip("Minimum frontline creatures")]
    [Min(0)]
    public int MinFrontline = 1;

    [Tooltip("Maximum frontline creatures")]
    [Min(0)]
    public int MaxFrontline = 3;

    [Tooltip("Minimum midline creatures")]
    [Min(0)]
    public int MinMidline = 0;

    [Tooltip("Maximum midline creatures")]
    [Min(0)]
    public int MaxMidline = 2;

    [Tooltip("Minimum backline creatures")]
    [Min(0)]
    public int MinBackline = 0;

    [Tooltip("Maximum backline creatures")]
    [Min(0)]
    public int MaxBackline = 2;

    [Header("Terrain Generation")]
    public TerrainGenerationMode TerrainMode = TerrainGenerationMode.UsePreset;

    [Tooltip("Preset layouts")]
    public List<TerrainLayout> PresetTerrainLayouts;

    [Header("Procedural Terrain")]
    [Range(0f, 1f)]
    public float TerrainDensity = 0.4f;
    public TerrainGenerationPattern GenerationPattern = TerrainGenerationPattern.Default;
    public bool EnableChasms = false;

    /// <summary>
    /// Generate a complete encounter from this ruleset
    /// </summary>
    public List<CreatureConfig> GenerateEncounter()
    {
        // First, determine how many creatures to spawn per position
        var spawnCounts = CalculateSpawnCounts();

        // Then generate the actual creatures
        var encounterCreatures = new List<CreatureConfig>();

        // Generate frontline
        if (FrontlinePool != null && spawnCounts.Frontline > 0)
        {
            encounterCreatures.AddRange(FrontlinePool.GetRandomCreatures(spawnCounts.Frontline));
        }

        // Generate midline
        if (MidlinePool != null && spawnCounts.Midline > 0)
        {
            encounterCreatures.AddRange(MidlinePool.GetRandomCreatures(spawnCounts.Midline));
        }

        // Generate backline
        if (BacklinePool != null && spawnCounts.Backline > 0)
        {
            encounterCreatures.AddRange(BacklinePool.GetRandomCreatures(spawnCounts.Backline));
        }

        // Filter out any null or invalid creatures
        return encounterCreatures.Where(c => c != null && c.IsValid()).ToList();
    }

    /// <summary>
    /// Calculate how many creatures to spawn in each position
    /// Ensures total stays within MinTotalCreatures and MaxTotalCreatures
    /// </summary>
    private (int Frontline, int Midline, int Backline) CalculateSpawnCounts()
    {
        // Roll a target total within our limits
        int targetTotal = Random.Range(MinTotalCreatures, MaxTotalCreatures + 1);

        // Start with minimum required from each position
        int frontline = MinFrontline;
        int midline = MinMidline;
        int backline = MinBackline;

        int currentTotal = frontline + midline + backline;

        // If minimums already exceed target, clamp down proportionally
        if (currentTotal > targetTotal)
        {
            float ratio = (float)targetTotal / currentTotal;
            frontline = Mathf.RoundToInt(frontline * ratio);
            midline = Mathf.RoundToInt(midline * ratio);
            backline = Mathf.RoundToInt(backline * ratio);

            // Ensure at least the target (rounding might have reduced too much)
            currentTotal = frontline + midline + backline;
            while (currentTotal < targetTotal && currentTotal < MaxTotalCreatures)
            {
                // Add to a random position that has room
                if (TryAddToRandomPosition(ref frontline, ref midline, ref backline))
                {
                    currentTotal++;
                }
                else
                {
                    break;
                }
            }

            return (frontline, midline, backline);
        }

        // We need to add more creatures to reach target
        int remaining = targetTotal - currentTotal;

        // Distribute remaining spawns randomly among positions that have room
        while (remaining > 0)
        {
            if (!TryAddToRandomPosition(ref frontline, ref midline, ref backline))
            {
                // No more room in any position
                break;
            }
            remaining--;
        }

        return (frontline, midline, backline);
    }

    /// <summary>
    /// Try to add a creature to a random position that has room
    /// Returns true if successful, false if no positions have room
    /// </summary>
    private bool TryAddToRandomPosition(ref int frontline, ref int midline, ref int backline)
    {
        var availablePositions = new List<int>();

        // Check which positions have room and a valid pool
        // 0 = frontline, 1 = midline, 2 = backline
        if (frontline < MaxFrontline && FrontlinePool != null && FrontlinePool.IsValid())
        {
            availablePositions.Add(0);
        }

        if (midline < MaxMidline && MidlinePool != null && MidlinePool.IsValid())
        {
            availablePositions.Add(1);
        }

        if (backline < MaxBackline && BacklinePool != null && BacklinePool.IsValid())
        {
            availablePositions.Add(2);
        }

        if (availablePositions.Count == 0)
        {
            return false;
        }

        // Pick a random position and increment it
        int selectedPosition = availablePositions[Random.Range(0, availablePositions.Count)];

        switch (selectedPosition)
        {
            case 0:
                frontline++;
                break;
            case 1:
                midline++;
                break;
            case 2:
                backline++;
                break;
        }

        return true;
    }

    public TerrainLayout GenerateTerrain()
    {
        switch (TerrainMode)
        {
            case TerrainGenerationMode.UsePreset:
                return GetRandomPresetLayout();
            case TerrainGenerationMode.Procedural:
                return TerrainGenerator.GenerateByPattern(GenerationPattern, TerrainDensity, EnableChasms);
            case TerrainGenerationMode.None:
            default:
                return null;
        }
    }

    private TerrainLayout GetRandomPresetLayout()
    {
        if (PresetTerrainLayouts == null || PresetTerrainLayouts.Count == 0)
            return null;

        var matchingLayouts = PresetTerrainLayouts
            .Where(layout => layout != null)
            .ToList();

        if (matchingLayouts.Count > 0)
        {
            return matchingLayouts[Random.Range(0, matchingLayouts.Count)];
        }

        return PresetTerrainLayouts[Random.Range(0, PresetTerrainLayouts.Count)];
    }

    private void OnValidate()
    {
        // Validate biome
        if (EncounterBiome == null)
        {
            Debug.LogWarning($"Encounter '{name}' has no biome set", this);
        }

        // Validate total limits
        if (MaxTotalCreatures < MinTotalCreatures)
        {
            Debug.LogWarning($"Encounter '{name}': MaxTotalCreatures < MinTotalCreatures", this);
        }

        // Validate position limits
        if (MaxFrontline < MinFrontline)
        {
            Debug.LogWarning($"Encounter '{name}': MaxFrontline < MinFrontline", this);
        }
        if (MaxMidline < MinMidline)
        {
            Debug.LogWarning($"Encounter '{name}': MaxMidline < MinMidline", this);
        }
        if (MaxBackline < MinBackline)
        {
            Debug.LogWarning($"Encounter '{name}': MaxBackline < MinBackline", this);
        }

        // Check if minimum requirements can be met
        int minPossible = MinFrontline + MinMidline + MinBackline;
        if (minPossible > MaxTotalCreatures)
        {
            Debug.LogWarning(
                $"Encounter '{name}': Position minimums ({minPossible}) exceed MaxTotalCreatures ({MaxTotalCreatures})",
                this);
        }

        // Check if maximum is achievable
        int maxPossible = MaxFrontline + MaxMidline + MaxBackline;
        if (maxPossible < MinTotalCreatures)
        {
            Debug.LogWarning(
                $"Encounter '{name}': Position maximums ({maxPossible}) cannot meet MinTotalCreatures ({MinTotalCreatures})",
                this);
        }

        // Validate pools
        if (MinFrontline > 0 && (FrontlinePool == null || !FrontlinePool.IsValid()))
        {
            Debug.LogWarning($"Encounter '{name}': Requires frontline creatures but pool is invalid", this);
        }
        if (MinMidline > 0 && (MidlinePool == null || !MidlinePool.IsValid()))
        {
            Debug.LogWarning($"Encounter '{name}': Requires midline creatures but pool is invalid", this);
        }
        if (MinBackline > 0 && (BacklinePool == null || !BacklinePool.IsValid()))
        {
            Debug.LogWarning($"Encounter '{name}': Requires backline creatures but pool is invalid", this);
        }
    }
}