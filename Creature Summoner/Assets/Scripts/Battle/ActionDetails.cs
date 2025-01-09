using UnityEngine;
using System.Collections.Generic;

public class ActionDetails
{
    public string AttackerName { get; set; }
    public string DefenderName { get; set; }

    public bool IsDefeated { get; set; }
    public bool IsCrit { get; set; }
    public bool IsMiss { get; set; }
    public bool IsGlancingBlow { get; set; }
    public bool IsDefensive { get; set; }
    public Effectiveness EffectRating { get; set; }

    private List<string> actionMessages;
    private List<string> extraMessages;

    public ActionDetails(string attackerName, string defenderName)
    {
        AttackerName = attackerName;
        DefenderName = defenderName;
        IsDefeated = false;
        IsCrit = false;
        IsMiss = false;
        IsGlancingBlow = false;
        EffectRating = Effectiveness.Neutral;
        actionMessages = new List<string>();
        extraMessages = new List<string>();
    }

    public void AddMessage(string message)
    {
        extraMessages.Add(message);
    }

    private void GenerateMessages()
    {
        if (IsMiss) 
        {
            actionMessages.Add($"The attack missed {DefenderName}.");
            return;
        }
        actionMessages.Add(getAttackResultMessage());
        if (EffectRating != Effectiveness.Neutral)
        {
            actionMessages.Add(getEffectivenessMessage());
        }
        if (IsDefeated)
        {
            actionMessages.Add($"{DefenderName} was defeated!");
        }
    }

    public List<string> GetMessages()
    {
        GenerateMessages();
        actionMessages.AddRange(extraMessages);
        return actionMessages;
    }

    private string getAttackResultMessage()
    {
        if (IsGlancingBlow)
            return $"The attack strikes {DefenderName} with a glancing blow!";

        if (IsCrit)
            return $"The attack strikes {DefenderName} with a critical hit!";
        if (IsDefensive)
        {
            return $"{AttackerName} defended itself";
        }

        return $"The attack strikes {DefenderName}.";
    }

    private string getEffectivenessMessage()
    {
        if (EffectRating == Effectiveness.VeryIneffectiveDual || EffectRating == Effectiveness.VeryIneffectiveSingle)
        {
            return "The attack type is very ineffective!";
        }
        else if (EffectRating == Effectiveness.IneffectiveDual || EffectRating == Effectiveness.IneffectiveSingle)
        {
            return "The attack type is ineffective.";
        }
        else if (EffectRating == Effectiveness.VeryEffectiveDual || EffectRating == Effectiveness.VeryEffectiveSingle)
        {
            return "The attack type is very effective!";
        }
        else if (EffectRating == Effectiveness.EffectiveDual || EffectRating == Effectiveness.EffectiveSingle)
        {
            return "The attack type is effective.";
        }
        else
        {
            // This case should not happen
            return "The attack type is neutral.";
        }
    }
}
