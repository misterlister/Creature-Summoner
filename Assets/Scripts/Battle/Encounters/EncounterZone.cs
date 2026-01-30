using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a zone within a MapArea where encounters can occur
/// </summary>
[System.Serializable]
public class EncounterZone
{
    [Header("Zone Configuration")]
    [Tooltip("Name for this encounter zone")]
    public string ZoneName = "Default Zone";

    [Tooltip("The rules that govern encounters in this zone")]
    public RandomEncounterRules EncounterRules;

    [Header("Encounter Rate")]
    [Tooltip("Steps between encounters (lower = more frequent)")]
    [Range(1, 100)]
    public int StepsPerEncounter = 20;

    [Tooltip("Random variance in steps")]
    [Range(0, 20)]
    public int StepVariance = 5;

    /// <summary>
    /// Generate an encounter using this zone's rules
    /// </summary>
    public List<CreatureConfig> GenerateEncounter()
    {
        if (EncounterRules == null)
        {
            Debug.LogWarning($"EncounterZone '{ZoneName}' has no rules assigned");
            return new List<CreatureConfig>();
        }

        return EncounterRules.GenerateEncounter();
    }

    /// <summary>
    /// Get terrain layout for this zone
    /// </summary>
    public TerrainLayout GenerateTerrain()
    {
        return EncounterRules?.GenerateTerrain();
    }

    /// <summary>
    /// Calculate steps until next encounter
    /// </summary>
    public int RollStepsUntilEncounter()
    {
        int variance = Random.Range(-StepVariance, StepVariance + 1);
        return Mathf.Max(1, StepsPerEncounter + variance);
    }

    public bool IsValid()
    {
        if (EncounterRules == null)
        {
            Debug.LogWarning($"EncounterZone '{ZoneName}' has no rules");
            return false;
        }

        return true;
    }
}