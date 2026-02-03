using UnityEngine;
using System.Collections.Generic;

public class MapArea : MonoBehaviour
{
    [Header("Area Configuration")]
    [SerializeField] private string areaName = "Unnamed Area";

    [Header("Encounter Zones")]
    [Tooltip("Encounter zones within this area")]
    [SerializeField] private List<EncounterZone> encounterZones = new List<EncounterZone>();

    [Tooltip("Which zone is currently active (index)")]
    [SerializeField] private int activeZoneIndex = 0;

    [Header("Team Setup")]
    [SerializeField] private CreatureTeam wildTeam;

    private int stepsSinceLastEncounter = 0;
    private int stepsUntilNextEncounter;
    private bool playerInArea = false;

    /// <summary>
    /// Get the currently active encounter zone
    /// </summary>
    public EncounterZone GetActiveZone()
    {
        if (encounterZones == null || encounterZones.Count == 0)
        {
            Debug.LogWarning($"MapArea '{areaName}' has no encounter zones");
            return null;
        }

        if (activeZoneIndex < 0 || activeZoneIndex >= encounterZones.Count)
        {
            Debug.LogWarning($"MapArea '{areaName}' has invalid active zone index");
            activeZoneIndex = 0;
        }

        return encounterZones[activeZoneIndex];
    }

    /// <summary>
    /// Set which encounter zone is active
    /// </summary>
    public void SetActiveZone(int index)
    {
        if (index >= 0 && index < encounterZones.Count)
        {
            activeZoneIndex = index;
            ResetEncounterSteps();
        }
        else
        {
            Debug.LogWarning($"Invalid zone index: {index}");
        }
    }

    /// <summary>
    /// Set active zone by name
    /// </summary>
    public void SetActiveZone(string zoneName)
    {
        for (int i = 0; i < encounterZones.Count; i++)
        {
            if (encounterZones[i].ZoneName == zoneName)
            {
                SetActiveZone(i);
                return;
            }
        }

        Debug.LogWarning($"No zone found with name: {zoneName}");
    }

    /// <summary>
    /// Called when player enters this map area
    /// </summary>
    public void OnPlayerEnter()
    {
        playerInArea = true;
        ResetEncounterSteps();
        Debug.Log($"Player entered {areaName}");
    }

    /// <summary>
    /// Called when player exits this map area
    /// </summary>
    public void OnPlayerExit()
    {
        playerInArea = false;
        stepsSinceLastEncounter = 0;
        Debug.Log($"Player left {areaName}");
    }

    /// <summary>
    /// Called when player takes a step - returns true if encounter triggered
    /// </summary>
    public bool OnPlayerStep()
    {
        if (!playerInArea)
            return false;

        stepsSinceLastEncounter++;

        if (stepsSinceLastEncounter >= stepsUntilNextEncounter)
        {
            ResetEncounterSteps();
            return true; // Trigger encounter
        }

        return false;
    }

    /// <summary>
    /// Reset the encounter step counter
    /// </summary>
    private void ResetEncounterSteps()
    {
        stepsSinceLastEncounter = 0;
        var activeZone = GetActiveZone();
        stepsUntilNextEncounter = activeZone?.RollStepsUntilEncounter() ?? 20;
    }

    /// <summary>
    /// Generate a wild creature team from the active zone
    /// </summary>
    public CreatureTeam GenerateWildCreatureTeam()
    {
        if (wildTeam == null)
        {
            Debug.LogError($"MapArea '{areaName}' has no wildTeam assigned");
            return null;
        }

        wildTeam.ClearCreatures();

        var activeZone = GetActiveZone();
        if (activeZone == null)
        {
            Debug.LogError($"MapArea '{areaName}' has no active encounter zone");
            return wildTeam;
        }

        var encounterCreatures = activeZone.GenerateEncounter();

        foreach (var config in encounterCreatures)
        {
            if (config != null && config.IsValid())
            {
                wildTeam.AddCreatureFromConfig(config);
            }
        }

        return wildTeam;
    }

    /// <summary>
    /// Get terrain layout from active zone
    /// </summary>
    public TerrainLayout GetTerrainLayout()
    {
        return GetActiveZone()?.GenerateTerrain();
    }

    public Biome GetBiome()
    {
        return GetActiveZone()?.GetBiome();
    }

    private void OnValidate()
    {
        if (encounterZones == null || encounterZones.Count == 0)
        {
            Debug.LogWarning($"MapArea '{areaName}' has no encounter zones", this);
        }
        else
        {
            foreach (var zone in encounterZones)
            {
                zone?.IsValid();
            }
        }

        if (activeZoneIndex >= encounterZones.Count)
        {
            activeZoneIndex = Mathf.Max(0, encounterZones.Count - 1);
        }
    }
}