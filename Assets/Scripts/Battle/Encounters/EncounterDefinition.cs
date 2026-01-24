using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Create Encounter Definition")]
public class EncounterDefinition : ScriptableObject
{
    [Header("Environment")]
    [Tooltip("The biome this encounter takes place in")]
    public Biome EncounterBiome;

    [Header("Enemy Team")]
    public List<PositionedCreature> EnemyCreatures;

    [Header("Terrain Layout (Optional)")]
    [Tooltip("Terrain layouts - should match EncounterBiome")]
    public TerrainLayout PlayerTerrainLayout;
    public TerrainLayout EnemyTerrainLayout;

    [Header("Encounter Metadata")]
    public string EncounterName;
    public int RewardXP;

    // Validation to catch mismatches
    private void OnValidate()
    {
        if (EncounterBiome == null)
        {
            Debug.LogWarning($"Encounter '{name}' has no biome set", this);
            return;
        }

        // Check player terrain matches
        if (PlayerTerrainLayout != null)
        {
            if (PlayerTerrainLayout.LayoutBiome == null)
            {
                Debug.LogWarning(
                    $"Player terrain layout '{PlayerTerrainLayout.name}' has no biome set. " +
                    $"Should be '{EncounterBiome.BiomeName}'", this);
            }
            else if (PlayerTerrainLayout.LayoutBiome != EncounterBiome)
            {
                Debug.LogError(
                    $"Player terrain biome mismatch! " +
                    $"Encounter is '{EncounterBiome.BiomeName}' but " +
                    $"terrain is '{PlayerTerrainLayout.LayoutBiome.BiomeName}'", this);
            }
        }

        // Check enemy terrain matches
        if (EnemyTerrainLayout != null)
        {
            if (EnemyTerrainLayout.LayoutBiome == null)
            {
                Debug.LogWarning(
                    $"Enemy terrain layout '{EnemyTerrainLayout.name}' has no biome set. " +
                    $"Should be '{EncounterBiome.BiomeName}'", this);
            }
            else if (EnemyTerrainLayout.LayoutBiome != EncounterBiome)
            {
                Debug.LogError(
                    $"Enemy terrain biome mismatch! " +
                    $"Encounter is '{EncounterBiome.BiomeName}' but " +
                    $"terrain is '{EnemyTerrainLayout.LayoutBiome.BiomeName}'", this);
            }
        }
    }
}