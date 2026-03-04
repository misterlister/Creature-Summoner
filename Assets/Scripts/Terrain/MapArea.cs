using UnityEngine;
using System.Collections.Generic;

public class MapArea : MonoBehaviour
{
    [Header("Area Configuration")]
    [SerializeField] private string areaName = "Unnamed Area";

    [Header("Encounter Zones")]
    [Tooltip("Encounter zones within this area")]
    [SerializeField] private List<EncounterZone> encounterZones = new List<EncounterZone>();

    [Header("Team Setup")]
    [SerializeField] private CreatureTeam wildTeam;

    [Header("Debug")]
    [Tooltip("If assigned, this specific team will always be used instead of generating one randomly. " +
         "Use for testing specific encounters.")]
    [SerializeField] private CreatureTeam debugWildTeam;

    private EncounterZone activeZone;
    private int stepsSinceLastEncounter = 0;
    private int stepsUntilNextEncounter;
    private bool playerInArea = false;

    private Dictionary<string, EncounterZone> zonesByName;

    private void Awake()
    {
        BuildZoneLookup();
    }

    private void BuildZoneLookup()
    {
        zonesByName = new Dictionary<string, EncounterZone>();
        foreach (var zone in encounterZones)
        {
            if (zone != null && !string.IsNullOrEmpty(zone.ZoneName))
                zonesByName[zone.ZoneName] = zone;
        }
    }

    // Called by EncounterZoneTrigger when player enters it
    public void OnPlayerEnterZone(string zoneName)
    {
        playerInArea = true;

        if (zonesByName.TryGetValue(zoneName, out var zone))
        {
            if (activeZone != zone)
            {
                activeZone = zone;
                ResetEncounterSteps();
            }
        }
        else
        {
            Debug.LogWarning($"MapArea '{areaName}': no zone named '{zoneName}'");
        }
    }

    public void OnPlayerExitZone()
    {
        // Only fully exit if player isn't in any other trigger in this area
        // PlayerController handles setting currentMapArea to null
        playerInArea = false;
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
        if (!playerInArea) return false;

        // If no zones, this is a safe area — never trigger encounters
        if (encounterZones == null || encounterZones.Count == 0) return false;

        stepsSinceLastEncounter++;
        if (stepsSinceLastEncounter >= stepsUntilNextEncounter)
        {
            ResetEncounterSteps();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Reset the encounter step counter
    /// </summary>
    private void ResetEncounterSteps()
    {
        stepsSinceLastEncounter = 0;
        stepsUntilNextEncounter = activeZone?.RollStepsUntilEncounter() ?? int.MaxValue;
        // int.MaxValue means "never encounter" for zone-free areas
    }

    /// <summary>
    /// Generate a wild creature team from the active zone
    /// </summary>
    public CreatureTeam GenerateWildCreatureTeam()
    {
        // Debug override — return the fixed team directly if assigned
        if (debugWildTeam != null && debugWildTeam.ConfigTeamSize > 0)
        {
            Debug.Log($"[DEBUG] MapArea '{areaName}' using debug wild team");
            return debugWildTeam;
        }

        if (wildTeam == null)
        {
            Debug.LogError($"MapArea '{areaName}' has no wildTeam assigned");
            return null;
        }

        wildTeam.ClearCreatures();

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
        return activeZone?.GenerateTerrain();
    }

    public Biome GetBiome()
    {
        return activeZone?.GetBiome();
    }

    private void OnValidate()
    {
        if (encounterZones == null) encounterZones = new List<EncounterZone>();

        #if UNITY_EDITOR
        // Check that all child triggers have matching zone names
        var triggers = GetComponentsInChildren<EncounterZoneTrigger>();
        foreach (var trigger in triggers)
        {
            bool found = encounterZones.Exists(z => z?.ZoneName == trigger.ZoneName);
            if (!found)
                Debug.LogWarning($"EncounterZoneTrigger '{trigger.name}' has zone name " +
                                 $"'{trigger.ZoneName}' which doesn't match any zone in MapArea '{areaName}'", trigger);
        }

        if (debugWildTeam != null && debugWildTeam.ConfigTeamSize > 0)
            Debug.LogWarning($"MapArea '{areaName}' has a DEBUG wild team assigned — " +
                             $"random encounters are disabled!", this);
        #endif
    }
}