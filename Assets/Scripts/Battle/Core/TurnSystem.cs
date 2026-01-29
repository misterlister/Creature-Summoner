using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages turn order and initiative for battles.
/// Handles rolling initiative, tracking turn order, and determining next creature to act.
/// </summary>
public class TurnSystem
{
    private BattleContext context;
    private List<Creature> turnOrder = new List<Creature>();
    private int currentTurnIndex = 0;

    public TurnSystem(BattleContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Roll initiative for all creatures at the start of a round.
    /// Creatures are ordered by Initiative roll, then Speed stat, then base Speed.
    /// </summary>
    public void RollInitiative(List<Creature> allCreatures)
    {
        turnOrder.Clear();
        currentTurnIndex = 0;

        // Roll initiative for each non-defeated creature
        foreach (var creature in allCreatures)
        {
            if (creature != null && !creature.IsDefeated)
            {
                creature.RollInitiative();
                turnOrder.Add(creature);
            }
        }

        // Sort by initiative (primary), speed (secondary), base speed (tertiary)
        turnOrder = turnOrder
            .OrderByDescending(c => c.Initiative)
            .ThenByDescending(c => c.Speed)
            .ThenByDescending(c => c.Species.Speed)
            .ToList();

        // Optional: Log turn order for debugging
        LogTurnOrder();
    }

    /// <summary>
    /// Get the next creature to act in the turn order.
    /// Automatically skips defeated creatures.
    /// Returns null if the round is over.
    /// </summary>
    public Creature GetNextCreature()
    {
        while (currentTurnIndex < turnOrder.Count)
        {
            var creature = turnOrder[currentTurnIndex];
            currentTurnIndex++;

            // Skip defeated creatures
            if (creature.IsDefeated)
            {
                continue;
            }

            return creature;
        }

        // No more creatures this round
        return null;
    }

    /// <summary>
    /// Remove a creature from the turn order (e.g., when defeated mid-round).
    /// </summary>
    public void RemoveCreature(Creature creature)
    {
        int index = turnOrder.IndexOf(creature);

        if (index >= 0)
        {
            turnOrder.RemoveAt(index);

            // Adjust current index if we removed something before it
            if (index < currentTurnIndex)
            {
                currentTurnIndex--;
            }
        }
    }

    /// <summary>
    /// Check if the current round is over (all creatures have acted).
    /// </summary>
    public bool IsRoundOver()
    {
        return currentTurnIndex >= turnOrder.Count;
    }

    /// <summary>
    /// Get the current turn order (for UI display).
    /// </summary>
    public List<Creature> GetTurnOrder()
    {
        return new List<Creature>(turnOrder);
    }

    /// <summary>
    /// Get how many turns are remaining in this round.
    /// </summary>
    public int GetRemainingTurns()
    {
        int remaining = 0;

        for (int i = currentTurnIndex; i < turnOrder.Count; i++)
        {
            if (!turnOrder[i].IsDefeated)
            {
                remaining++;
            }
        }

        return remaining;
    }

    /// <summary>
    /// Check if a specific creature has already acted this round.
    /// </summary>
    public bool HasCreatureActed(Creature creature)
    {
        int index = turnOrder.IndexOf(creature);
        return index >= 0 && index < currentTurnIndex;
    }

    /// <summary>
    /// Get the creature that will act next (without advancing turn).
    /// Useful for UI preview.
    /// </summary>
    public Creature PeekNextCreature()
    {
        for (int i = currentTurnIndex; i < turnOrder.Count; i++)
        {
            if (!turnOrder[i].IsDefeated)
            {
                return turnOrder[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Reset the turn system (for starting a new battle).
    /// </summary>
    public void Reset()
    {
        turnOrder.Clear();
        currentTurnIndex = 0;
    }

    private void LogTurnOrder()
    {
        if (turnOrder.Count == 0) return;

        UnityEngine.Debug.Log("=== Turn Order ===");
        for (int i = 0; i < turnOrder.Count; i++)
        {
            var creature = turnOrder[i];
            UnityEngine.Debug.Log($"{i + 1}. {creature.Nickname} (Initiative: {creature.Initiative}, Speed: {creature.Speed})");
        }
    }
}