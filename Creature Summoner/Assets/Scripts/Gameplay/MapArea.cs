using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static GameConstants;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Creature> wildCreaturesFront;
    [SerializeField] List<Creature> wildCreaturesBack;
    [SerializeField] int frontColMinSize = 1;
    [SerializeField] int frontColMaxSize = 3;
    [SerializeField] int backColMinSize = 1;
    [SerializeField] int backColMaxSize = 3;
    [SerializeField] CreatureTeam wildTeam;

    public Creature GetRandomCreatureEncounter(List<Creature>wildCreatures)
    {
        var encounteredCreature = wildCreatures[Random.Range(0, wildCreatures.Count)];
        encounteredCreature.Init();
        return encounteredCreature;
    }

    public CreatureTeam GenerateWildCreatureTeam()
    {
        // Clamp front and back column sizes to respect the constraints
        frontColMinSize = Mathf.Clamp(frontColMinSize, 1, BATTLE_COLS); // Min size for front is 1
        frontColMaxSize = Mathf.Clamp(frontColMaxSize, frontColMinSize, BATTLE_COLS);
        backColMinSize = Mathf.Clamp(backColMinSize, 0, BATTLE_COLS);  // Min size for back is 0
        backColMaxSize = Mathf.Clamp(backColMaxSize, backColMinSize, BATTLE_COLS);

        wildTeam.ClearCreatures();
        var selectedCreatures = new List<Creature>();

        // Generate creatures for the front column
        int frontCount = Random.Range(frontColMinSize, frontColMaxSize + 1);
        int emptyFront = frontColMaxSize - frontCount;
        var frontCreatures = GenerateColumnCreatures(wildCreaturesFront, frontCount, emptyFront);

        // Generate creatures for the back column
        int backCount = Random.Range(backColMinSize, backColMaxSize + 1);
        int emptyBack = backColMaxSize - backCount;
        var backCreatures = GenerateColumnCreatures(wildCreaturesBack, backCount, emptyBack);

        // Combine front and back creatures into the team
        selectedCreatures.AddRange(frontCreatures);
        selectedCreatures.AddRange(backCreatures);

        wildTeam.SetCreatures(selectedCreatures);

        return wildTeam;
    }

    private List<Creature> GenerateColumnCreatures(List<Creature> sourcePool, int count, int emptySlots)
    {
        // Initialize the column list with generated creatures
        var columnCreatures = new List<Creature>();
        for (int i = 0; i < count; i++)
        {
            var creature = GetRandomCreatureEncounter(sourcePool);
            columnCreatures.Add(creature);
        }

        // Fill in empty slots with null, alternating between the beginning and end
        for (int i = 0; i < emptySlots; i++)
        {
            if (i % 2 == 0) // Add to the beginning
            {
                columnCreatures.Insert(0, null);
            }
            else // Add to the end
            {
                columnCreatures.Add(null);
            }
        }

        // Ensure the list size matches the max size for the column
        while (columnCreatures.Count < BATTLE_COLS)
        {
            columnCreatures.Add(null);
        }

        return columnCreatures;
    }

}
