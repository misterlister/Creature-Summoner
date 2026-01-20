using System.Collections.Generic;
using static GameConstants;

public enum CreatureElement
{
    None,
    Air,
    Arcane,
    Beast,
    Cold,
    Earth,
    Electric,
    Fire,
    Metal,
    Necrotic,
    Plant,
    Radiant,
    Water
}

public enum Effectiveness
{
    VeryEffectiveSingle,
    VeryEffectiveDual,
    EffectiveSingle,
    EffectiveDual,
    Neutral,
    IneffectiveDual,
    IneffectiveSingle,
    VeryIneffectiveDual,
    VeryIneffectiveSingle,
}

public class ElementalEffectivenessChart
{
    public static Dictionary<Effectiveness, float> EffectiveMod = new Dictionary<Effectiveness, float>
    {
        {Effectiveness.VeryEffectiveSingle, V_EFF_SINGLE},
        {Effectiveness.VeryEffectiveDual, V_EFF_DUAL},
        {Effectiveness.EffectiveSingle, EFF_SINGLE},
        {Effectiveness.EffectiveDual, EFF_DUAL},
        {Effectiveness.Neutral, NEUTRAL},
        {Effectiveness.IneffectiveDual, INEFF_DUAL},
        {Effectiveness.IneffectiveSingle, INEFF_SINGLE},
        {Effectiveness.VeryIneffectiveDual, V_INEFF_DUAL},
        {Effectiveness.VeryIneffectiveSingle, V_INEFF_SINGLE},
    };

    const int NEU = 0;
    const int EFF = 1;
    const int INF = -1;

    static int[][] chart =
    {   
        //                  NON       AIR  ARC  BST  CLD  ERT  ELE  FIR  MET  NEC  PLT  RAD  WAT
        /*NON*/ new int[] { NEU,      NEU, NEU, NEU, NEU, NEU, NEU, NEU, NEU, NEU, NEU, NEU, NEU },

        /*AIR*/ new int[] { NEU,      NEU, INF, NEU, NEU, EFF, INF, EFF, NEU, NEU, NEU, NEU, NEU },
        /*ARC*/ new int[] { NEU,      EFF, INF, INF, EFF, NEU, EFF, EFF, INF, INF, NEU, EFF, NEU },
        /*BST*/ new int[] { NEU,      INF, EFF, NEU, NEU, NEU, NEU, NEU, INF, INF, EFF, EFF, NEU },
        /*CLD*/ new int[] { NEU,      NEU, NEU, NEU, INF, NEU, NEU, INF, INF, EFF, EFF, NEU, EFF },
        /*ERT*/ new int[] { NEU,      INF, NEU, NEU, NEU, NEU, EFF, EFF, EFF, NEU, INF, NEU, INF },
        /*ELE*/ new int[] { NEU,      EFF, NEU, NEU, NEU, INF, INF, NEU, EFF, NEU, INF, NEU, EFF },
        /*FIR*/ new int[] { NEU,      NEU, NEU, NEU, EFF, INF, NEU, INF, EFF, EFF, EFF, INF, INF },
        /*MET*/ new int[] { NEU,      NEU, EFF, EFF, NEU, INF, INF, INF, INF, NEU, NEU, NEU, NEU },
        /*NEC*/ new int[] { NEU,      NEU, EFF, EFF, INF, NEU, NEU, NEU, NEU, INF, EFF, INF, NEU },
        /*PLT*/ new int[] { NEU,      NEU, EFF, INF, NEU, EFF, EFF, INF, INF, NEU, INF, NEU, EFF },
        /*RAD*/ new int[] { NEU,      NEU, INF, NEU, NEU, NEU, NEU, NEU, NEU, EFF, NEU, INF, NEU },
        /*WAT*/ new int[] { NEU,      NEU, NEU, NEU, NEU, EFF, NEU, EFF, NEU, NEU, INF, NEU, INF },
    };

    public static Effectiveness GetEffectiveness(
        CreatureElement attackElement, 
        bool singleElementAttacker,
        CreatureElement defenseElement1 = CreatureElement.None, 
        CreatureElement defenseElement2 = CreatureElement.None
        )
    {
        bool singleElementDefender = (defenseElement1 == defenseElement2 || defenseElement2 == CreatureElement.None);
        int element1Effect = chart[(int)attackElement][(int)defenseElement1];
        int element2Effect = chart[(int)attackElement][(int)defenseElement2];
        int combinedEffect = element1Effect + element2Effect;

        switch (combinedEffect)
        {
            case -2:
                if (singleElementDefender)
                {
                    return Effectiveness.VeryIneffectiveSingle;
                }
                else
                {
                    return Effectiveness.VeryIneffectiveDual;
                }
            case -1:
                if (singleElementDefender)
                {
                    return Effectiveness.IneffectiveSingle;
                }
                else
                {
                    return Effectiveness.IneffectiveDual;
                }
            case 1:
                if (singleElementAttacker)
                {
                    return Effectiveness.EffectiveSingle;
                }
                else
                {
                    return Effectiveness.EffectiveDual;
                }
            case 2:
                if (singleElementAttacker)
                {
                    return Effectiveness.VeryEffectiveSingle;
                }
                else
                {
                    return Effectiveness.VeryEffectiveDual;
                }
            default:
                return Effectiveness.Neutral;
        }
    }
}
