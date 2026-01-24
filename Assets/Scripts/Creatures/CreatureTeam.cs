
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages a team of creatures, creating them from configuration data.
/// </summary>
public class CreatureTeam : MonoBehaviour
{
    [Header("Team Configuration")]
    [Tooltip("Configure each creature in the team")]
    [SerializeField] private List<CreatureConfig> creatureConfigs = new List<CreatureConfig>();

    [Header("Runtime (Read Only)")]
    [Tooltip("The actual creature instances created at runtime")]
    [SerializeField, ReadOnly] private List<Creature> creatures = new List<Creature>();

    public IReadOnlyList<Creature> Creatures => creatures;

    private void Start()
    {
        InitializeCreatures();
    }

    /// <summary>
    /// Create creature instances from the configured data
    /// </summary>
    public void InitializeCreatures()
    {
        creatures.Clear();

        foreach (var config in creatureConfigs)
        {
            if (config == null || !config.IsValid())
            {
                Debug.LogWarning($"Skipping invalid creature config in {gameObject.name}");
                continue;
            }

            Creature creature = Creature.FromConfig(config);
            if (creature != null)
            {
                creatures.Add(creature);
                Debug.Log($"Created creature: {creature.Nickname} (Lv.{creature.Level} {creature.Species.CreatureName})");
            }
        }

        if (creatures.Count == 0)
        {
            Debug.LogWarning($"CreatureTeam '{gameObject.name}' has no valid creatures!");
        }
    }

    /// <summary>
    /// Replace the team with new creatures at runtime
    /// </summary>
    public void SetCreatures(List<Creature> newCreatures)
    {
        creatures = new List<Creature>(newCreatures);
    }

    /// <summary>
    /// Clear all creatures from the team
    /// </summary>
    public void ClearCreatures()
    {
        creatures.Clear();
    }

    /// <summary>
    /// Add a creature to the team at runtime
    /// </summary>
    public void AddCreature(Creature creature)
    {
        if (creature != null)
        {
            creatures.Add(creature);
        }
    }

    /// <summary>
    /// Add a creature from a config at runtime
    /// </summary>
    public void AddCreatureFromConfig(CreatureConfig config)
    {
        if (config != null && config.IsValid())
        {
            Creature creature = Creature.FromConfig(config);
            if (creature != null)
            {
                creatures.Add(creature);
            }
        }
    }

    /// <summary>
    /// Remove a creature from the team
    /// </summary>
    public void RemoveCreature(Creature creature)
    {
        creatures.Remove(creature);
    }

    /// <summary>
    /// Get all alive (non-defeated) creatures
    /// </summary>
    public List<Creature> GetAliveCreatures()
    {
        return creatures.Where(c => c != null && !c.IsDefeated).ToList();
    }

    /// <summary>
    /// Check if entire team is defeated
    /// </summary>
    public bool IsTeamDefeated()
    {
        return creatures.All(c => c == null || c.IsDefeated);
    }

    /// <summary>
    /// Get the number of creatures in the team
    /// </summary>
    public int GetCreatureCount()
    {
        return creatures.Count;
    }

    /// <summary>
    /// Get a specific creature by index
    /// </summary>
    public Creature GetCreature(int index)
    {
        if (index >= 0 && index < creatures.Count)
        {
            return creatures[index];
        }
        return null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor utility to reload creatures (useful for testing)
    /// </summary>
    [ContextMenu("Reload Creatures")]
    private void ReloadCreatures()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Can only reload creatures in Play Mode");
            return;
        }

        InitializeCreatures();
        Debug.Log($"Reloaded {creatures.Count} creatures");
    }
#endif
}

/// <summary>
/// Custom property drawer attribute for read-only fields in inspector
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute { }