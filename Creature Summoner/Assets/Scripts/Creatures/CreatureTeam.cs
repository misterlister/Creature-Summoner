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
        foreach (var creature in creatures)
        {
            creature.Init();
        }
    }

    public void SetCreatures(List<Creature> newCreatures)
    {
        creatures = newCreatures;

        // Initialize the creatures if not already done
        foreach (var creature in creatures)
        {
            creature.Init();
        }
    }

    public void ClearCreatures()
    {
        creatures.Clear();
    }
}
