using Game.Battle.Modifiers;
using System.Collections.Generic;

public class CreatureCombatModifiers
{
    private readonly Creature owner;

    private List<CombatModifier> cachedPassiveMods = new();
    private bool modsDirty = true;

    public CreatureCombatModifiers(Creature owner)
    {
        this.owner = owner;
    }

    public void GetAllModifiers(BattleContext context, List<CombatModifier> outMods)
    {
        outMods.Clear();

        if (modsDirty)
        {
            RecachePassiveModifiers();
        }

        outMods.AddRange(cachedPassiveMods);

        foreach (var trait in owner.Traits)
        {
            trait.CollectCombatModifiers(owner, context, outMods);
        } 
    }

    public float GetCombatModifier(CombatModifierType type, BattleContext context)
    {
        if (modsDirty)
        {
            RecachePassiveModifiers();
        }

        float additiveMods = 0f;
        float multiplicativeMods = 1f;

        foreach (var mod in cachedPassiveMods)
        {
            if (mod.Type == type)
            {
                if (mod.Mode == CombatModifierMode.Additive)
                {
                    additiveMods += mod.Value;
                }
                else if (mod.Mode == CombatModifierMode.Multiplicative)
                {
                    multiplicativeMods *= (1 + (mod.Value / 100f));
                }
            }
        }

        return (1 + (additiveMods / 100f)) * multiplicativeMods;
    }

    public void MarkDirty()
    {
        modsDirty = true;
    }

    private void RecachePassiveModifiers()
    {
        cachedPassiveMods.Clear();

        foreach (var trait in owner.Traits)
        {
            trait.CollectPassiveCombatModifiers(cachedPassiveMods);
        }

        modsDirty = false;
    }
}
