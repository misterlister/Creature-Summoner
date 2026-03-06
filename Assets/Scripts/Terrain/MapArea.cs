using UnityEngine;
using System.Collections.Generic;

public class MapArea : MonoBehaviour
{
    [Header("Area Configuration")]
    [SerializeField] private string areaName = "Unnamed Area";

    [Header("Team Setup")]
    [SerializeField] private CreatureTeam wildTeam;

    [Header("Debug")]
    [Tooltip("Overrides randomized encounters. Use for testing specific teams.")]
    [SerializeField] private CreatureTeam debugWildTeam;

    private RandomEncounterRules activeRules;
    private int stepsSinceLastEncounter = 0;
    private int stepsUntilNextEncounter;
    private bool playerInArea = false;

    // Called by EncounterZoneTrigger when player enters it
    public void OnPlayerEnterZone(RandomEncounterRules rules)
    {
        playerInArea = true;

        if (activeRules != rules)
        {
            activeRules = rules;
            ResetEncounterSteps();
        }
    }

    /// <summary>
    /// Called when player exits this map area
    /// </summary>
    public void OnPlayerExit()
    {
        playerInArea = false;
        stepsSinceLastEncounter = 0;
        activeRules = null;
    }

    /// <summary>
    /// Called when player takes a step - returns true if encounter triggered
    /// </summary>
    public bool OnPlayerStep()
    {
        if (!playerInArea || activeRules == null) return false;
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
        stepsUntilNextEncounter = activeRules?.RollStepsUntilEncounter() ?? int.MaxValue;
        // int.MaxValue means "never encounter" for non-combat zones
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

        if (activeRules == null)
        {
            Debug.LogError($"MapArea '{areaName}' has no active encounter rules");
            return null;
        }

        wildTeam.ClearCreatures();

        var encounterCreatures = activeRules.GenerateEncounter();

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
        return activeRules?.GenerateTerrain();
    }

    public Biome GetBiome()
    {
        return activeRules?.EncounterBiome;
    }

    private void OnValidate()
    {

        #if UNITY_EDITOR
        var triggers = GetComponentsInChildren<EncounterZone>();
        foreach (var trigger in triggers)
        {
            if (trigger.GetEncounterRules() == null)
                Debug.LogWarning($"EncounterZoneTrigger '{trigger.name}' has no rules assigned", trigger);
        }
        if (debugWildTeam != null && debugWildTeam.ConfigTeamSize > 0)
            Debug.LogWarning($"MapArea '{areaName}' has a DEBUG wild team — random encounters disabled!", this);
        #endif
    }
}