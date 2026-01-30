using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A simple pool of creatures that can be drawn from
/// </summary>
[CreateAssetMenu(menuName = "Battle/Create Creature Pool")]
public class CreaturePool : ScriptableObject
{
    [Header("Pool Configuration")]
    [Tooltip("Name/description of this pool")]
    [SerializeField] private string poolDescription;

    [Tooltip("Creatures available in this pool")]
    public List<CreatureConfig> Creatures = new List<CreatureConfig>();

    /// <summary>
    /// Get a random creature from this pool
    /// </summary>
    public CreatureConfig GetRandomCreature()
    {
        if (Creatures == null || Creatures.Count == 0)
        {
            Debug.LogWarning($"CreaturePool '{name}' is empty");
            return null;
        }

        return Creatures[Random.Range(0, Creatures.Count)];
    }

    /// <summary>
    /// Get multiple random creatures from this pool
    /// </summary>
    public List<CreatureConfig> GetRandomCreatures(int count)
    {
        var result = new List<CreatureConfig>();

        for (int i = 0; i < count; i++)
        {
            var creature = GetRandomCreature();
            if (creature != null)
            {
                result.Add(creature);
            }
        }

        return result;
    }

    public bool IsValid()
    {
        if (Creatures == null || Creatures.Count == 0)
        {
            Debug.LogWarning($"CreaturePool '{name}' has no creatures", this);
            return false;
        }

        foreach (var config in Creatures)
        {
            if (config == null || !config.IsValid())
            {
                Debug.LogWarning($"CreaturePool '{name}' contains invalid creature config", this);
                return false;
            }
        }

        return true;
    }

    private void OnValidate()
    {
        IsValid();
    }
}