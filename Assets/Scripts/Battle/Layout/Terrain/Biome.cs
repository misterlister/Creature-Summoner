using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a biome with its visual terrain variations.
/// </summary>
[CreateAssetMenu(menuName = "Battle/Create Biome")]
public class Biome : ScriptableObject
{
    [Header("Identity")]
    public string BiomeName;

    [TextArea(2, 3)]
    public string Description;

    [Header("Terrain Types")]
    [Tooltip("Terrain types that can appear in this biome")]
    public BiomeTerrainSet AvailableTerrains;

    [Header("Procedural Generation Weights")]
    [Tooltip("Likelihood of different terrain types appearing")]
    public TerrainWeights GenerationWeights = new TerrainWeights();

    /// <summary>
    /// Get a random visual variant for a terrain type in this biome
    /// </summary>
    public TerrainVisuals GetRandomVariant(System.Type terrainType)
    {
        return AvailableTerrains?.GetRandomVariant(terrainType);
    }

    /// <summary>
    /// Get all terrain types valid for this biome
    /// </summary>
    public List<System.Type> GetValidTerrainTypes()
    {
        return AvailableTerrains?.GetAllTerrainTypes() ?? new List<System.Type>();
    }
}

/// <summary>
/// Container for all terrain visual variants in a biome
/// </summary>
[System.Serializable]
public class BiomeTerrainSet
{
    [Header("Terrain Tiles")]
    public List<TerrainVisuals> RegularVariants = new List<TerrainVisuals>();
    public List<TerrainVisuals> LightCoverVariants = new List<TerrainVisuals>();
    public List<TerrainVisuals> HeavyCoverVariants = new List<TerrainVisuals>();
    public List<TerrainVisuals> LightRoughVariants = new List<TerrainVisuals>();
    public List<TerrainVisuals> HeavyRoughVariants = new List<TerrainVisuals>();
    public List<TerrainVisuals> ChasmVariants = new List<TerrainVisuals>();

    public TerrainVisuals GetRandomVariant(System.Type terrainType)
    {
        var variants = GetVariantList(terrainType);
        if (variants == null || variants.Count == 0) return null;
        return variants[Random.Range(0, variants.Count)];
    }

    public List<System.Type> GetAllTerrainTypes()
    {
        var types = new List<System.Type>();

        if (RegularVariants.Count > 0) types.Add(typeof(RegularTerrain));
        if (LightCoverVariants.Count > 0) types.Add(typeof(LightCoverTerrain));
        if (HeavyCoverVariants.Count > 0) types.Add(typeof(HeavyCoverTerrain));
        if (LightRoughVariants.Count > 0) types.Add(typeof(LightRoughTerrain));
        if (HeavyRoughVariants.Count > 0) types.Add(typeof(HeavyRoughTerrain));
        if (ChasmVariants.Count > 0) types.Add(typeof(ChasmTerrain));

        return types;
    }

    private List<TerrainVisuals> GetVariantList(System.Type terrainType)
    {
        if (terrainType == typeof(RegularTerrain)) return RegularVariants;
        if (terrainType == typeof(LightCoverTerrain)) return LightCoverVariants;
        if (terrainType == typeof(HeavyCoverTerrain)) return HeavyCoverVariants;
        if (terrainType == typeof(LightRoughTerrain)) return LightRoughVariants;
        if (terrainType == typeof(HeavyRoughTerrain)) return HeavyRoughVariants;
        if (terrainType == typeof(ChasmTerrain)) return ChasmVariants;
        return null;
    }
}

/// <summary>
/// Visual representation data for a terrain variant
/// </summary>
[System.Serializable]
public class TerrainVisuals
{
    [Tooltip("Display name for this variant (e.g., 'Boulder', 'Bush', 'Rubble')")]
    public string VariantName;

    public Sprite Sprite;
    public Color TintColor = Color.white;

    [Tooltip("Optional particle effect")]
    public GameObject ParticleEffectPrefab;
}

/// <summary>
/// Weights for procedural terrain generation
/// </summary>
[System.Serializable]
public class TerrainWeights
{
    [Range(0f, 1f)] public float Regular = 0.3f;
    [Range(0f, 1f)] public float LightCover = 0.2f;
    [Range(0f, 1f)] public float HeavyCover = 0.1f;
    [Range(0f, 1f)] public float LightRough = 0.15f;
    [Range(0f, 1f)] public float HeavyRough = 0.1f;
    [Range(0f, 1f)] public float Chasm = 0.05f;

    public float GetWeight(System.Type terrainType)
    {
        if (terrainType == typeof(RegularTerrain)) return Regular;
        if (terrainType == typeof(LightCoverTerrain)) return LightCover;
        if (terrainType == typeof(HeavyCoverTerrain)) return HeavyCover;
        if (terrainType == typeof(LightRoughTerrain)) return LightRough;
        if (terrainType == typeof(HeavyRoughTerrain)) return HeavyRough;
        if (terrainType == typeof(ChasmTerrain)) return Chasm;
        return 0f;
    }
}