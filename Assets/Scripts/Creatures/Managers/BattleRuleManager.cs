using Game.Battle.Modifiers;
using System;
using System.Collections.Generic;

public class BattleRuleManager
{
    private readonly Creature owner;
    private readonly Dictionary<BattleRuleType, bool> rules = new();

    public BattleRuleManager(Creature owner)
    {
        this.owner = owner;
        Rebuild();
    }

    public bool Has(BattleRuleType rule) => rules[rule];

    public void Apply(PassiveBattleRuleModifier modifier)
    {
        rules[modifier.Type] = true;
    }

    public void Rebuild()
    {
        foreach (BattleRuleType rule in Enum.GetValues(typeof(BattleRuleType)))
        {
            rules[rule] = false;
        }

        List<BattleRuleModifier> mods = new();

        foreach (var trait in owner.Traits)
        {
            trait.CollectBattleRuleModifiers(mods);
        }

        foreach (var mod in mods)
        {
            rules[mod.Type] = true;
        }
    }
}