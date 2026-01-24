using UnityEngine;

[System.Serializable]
public class CreatureConfig
{
    [Header("Identity")]
    [Tooltip("The species of this creature.")]
    public CreatureBase Species;

    [Tooltip("Starting level")]
    [Min(1)]
    public int Level = 1;

    [Tooltip("Optional: Custom nickname. If empty, uses species name.")]
    public string Nickname;

    [Header("Actions")]
    [Tooltip("Optional: Custom action loadout. If null, uses auto-equipped actions.")]
    public CreatureActionLoadout Loadout;

    [Header("Starting Class (Optional)")]
    [Tooltip("Optional: Starting class. If null, creature has no class.")]
    public CreatureClass StartingClass;
    public bool IsValid()
    {
        if (Species == null)
        {
            Debug.LogWarning("CreatureConfig has no Species assigned");
            return false;
        }

        if (Level < 1)
        {
            Debug.LogWarning($"CreatureConfig for {Species.CreatureName} has invalid level: {Level}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Create a creature instance from this config
    /// </summary>
    public Creature CreateCreature()
    {
        return Creature.FromConfig(this);
    }
}

/// <summary>
/// Optional: ScriptableObject version for reusable configs
/// </summary>
[CreateAssetMenu(fileName = "New Creature Config", menuName = "Game/Creature/Create Creature Config")]
public class CreatureConfigAsset : ScriptableObject
{
    public CreatureConfig Config;

    public Creature CreateCreature()
    {
        return Config.CreateCreature();
    }
}