using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Creature> wildCreatures;
    [SerializeField] int groupMinSize = 6;
    [SerializeField] int groupMaxSize = 6;
    [SerializeField] CreatureTeam wildTeam;

    public Creature GetRandomCreatureEncounter()
    {
        var encounteredCreature = wildCreatures[Random.Range(0, wildCreatures.Count)];
        encounteredCreature.Init();
        return encounteredCreature;
    }

    public CreatureTeam GenerateWildCreatureTeam()
    {
        int teamsize = Random.Range(groupMinSize, groupMaxSize + 1);
        wildTeam.ClearCreatures();
        var selectedCreatures = new List<Creature>();

        for (int i = 0; i < teamsize; i++)
        {
            var creature = GetRandomCreatureEncounter();
            selectedCreatures.Add(creature);
        }

        // Assign the selected creatures to the team's list
        wildTeam.SetCreatures(selectedCreatures);

        return wildTeam;
    }
}
