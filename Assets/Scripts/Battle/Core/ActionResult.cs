using System.Collections.Generic;

// SPLIT?

public class ActionResult
{
    public ActionBase Action { get; }
    public Creature Attacker { get; }
    public List<TargetResult> TargetResults { get; } = new List<TargetResult>();
    public bool IsDefensive { get; set; }

    private List<string> extraMessages = new List<string>();

    public ActionResult(ActionBase action, Creature attacker)
    {
        Action = action;
        Attacker = attacker;
    }

    public void AddTargetResult(TargetResult result)
    {
        TargetResults.Add(result);
    }

    public void AddMessage(string message)
    {
        extraMessages.Add(message);
    }

    public List<string> GetMessages()
    {
        var messages = new List<string>();

        if (IsDefensive)
        {
            messages.Add($"{Attacker.Nickname} defended itself");
            messages.AddRange(extraMessages);
            return messages;
        }

        // Generate messages for each target
        foreach (var targetResult in TargetResults)
        {
            messages.AddRange(targetResult.GetMessages(Attacker.Nickname));
        }

        messages.AddRange(extraMessages);
        return messages;
    }
}

public class TargetResult
{
    public Creature Target { get; }
    public HitType HitType { get; set; } = HitType.Hit;
    public int DamageDealt { get; set; }
    public int HealingDone { get; set; }
    public bool IsDefeated { get; set; }
    public Effectiveness EffectRating { get; set; } = Effectiveness.Neutral;

    public TargetResult(Creature target)
    {
        Target = target;
    }

    public List<string> GetMessages(string attackerName)
    {
        var messages = new List<string>();

        // Attack result
        messages.Add(GetAttackResultMessage(attackerName));

        // Effectiveness
        if (EffectRating != Effectiveness.Neutral)
        {
            messages.Add(GetEffectivenessMessage());
        }

        // Defeat
        if (IsDefeated)
        {
            messages.Add($"{Target.Nickname} was defeated!");
        }

        return messages;
    }

    private string GetAttackResultMessage(string attackerName)
    {
        if (HealingDone > 0)
        {
            return HitType == HitType.Critical
                ? $"{attackerName} healed {Target.Nickname} with a critical heal!"
                : $"{attackerName} healed {Target.Nickname}.";
        }

        return HitType switch
        {
            HitType.Glance => $"The attack strikes {Target.Nickname} with a glancing blow!",
            HitType.Critical => $"The attack strikes {Target.Nickname} with a critical hit!",
            _ => $"The attack strikes {Target.Nickname}."
        };
    }

    private string GetEffectivenessMessage()
    {
        return EffectRating switch
        {
            Effectiveness.VeryIneffectiveDual or Effectiveness.VeryIneffectiveSingle
                => "The attack type is very ineffective!",
            Effectiveness.IneffectiveDual or Effectiveness.IneffectiveSingle
                => "The attack type is ineffective.",
            Effectiveness.VeryEffectiveDual or Effectiveness.VeryEffectiveSingle
                => "The attack type is very effective!",
            Effectiveness.EffectiveDual or Effectiveness.EffectiveSingle
                => "The attack type is effective.",
            _ => "The attack type is neutral."
        };
    }
}