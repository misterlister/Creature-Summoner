using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatureTeam : MonoBehaviour
{
    [SerializeField] List<Creature> creatures;

    public List<Creature> Creatures
    {
        get
        {
            return creatures;
        }
    }

    private void Start()
    {
        for (int i = 0; i < creatures.Count; i++)
        {
            if (creatures[i] != null)
            {
                if (creatures[i].Species == null)
                {
                    creatures[i] = null; // Assign null to empty creature slots
                }
                else
                {
                    creatures[i].Init(); // Initialize the creature
                }
            }
        }
    }

    public void SetCreatures(List<Creature> newCreatures)
    {
        creatures = newCreatures;

        // Initialize the creatures if not already done
        foreach (var creature in creatures)
        {
            if (creature != null)
            {
                creature.Init();
            }
        }
    }

    public void ClearCreatures()
    {
        creatures.Clear();
    }
}
