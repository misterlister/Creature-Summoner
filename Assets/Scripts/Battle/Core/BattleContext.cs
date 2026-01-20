using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleContext
{
    public List<Creature> AllCreatures { get; private set; }
    public Creature CurrentTarget { get; private set; }
    public int TurnNumber { get; private set; }
    public bool IsPlayerTurn { get; private set; }

    public List<Creature> GetAlliesOfElement(Creature creature, CreatureElement type)
    {
        return AllCreatures.Where(c => c != creature && c.IsElement(type)).ToList();
    }
}