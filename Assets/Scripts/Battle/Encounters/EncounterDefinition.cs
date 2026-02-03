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

    [Header("Terrain Layout")]
    public TerrainLayout TerrainLayout;

    [Header("Encounter Metadata")]
    public string EncounterName;
    public int ExtraRewardXP;
}