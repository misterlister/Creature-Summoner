using System.Collections.Generic;
using static GameConstants;

public enum CreatureType
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

public class TypeChart
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
        CreatureType attackType, 
        bool singleTypeAttacker,
        CreatureType defenseType1 = CreatureType.None, 
        CreatureType defenseType2 = CreatureType.None
        )
    {
        bool singleTypeDefender = (defenseType1 == defenseType2 || defenseType2 == CreatureType.None);
        int type1Effect = chart[(int)attackType][(int)defenseType1];
        int type2Effect = chart[(int)attackType][(int)defenseType2];
        int combinedEffect = type1Effect + type2Effect;

        switch (combinedEffect)
        {
            case -2:
                if (singleTypeDefender)
                {
                    return Effectiveness.VeryIneffectiveSingle;
                }
                else
                {
                    return Effectiveness.VeryIneffectiveDual;
                }
            case -1:
                if (singleTypeDefender)
                {
                    return Effectiveness.IneffectiveSingle;
                }
                else
                {
                    return Effectiveness.IneffectiveDual;
                }
            case 1:
                if (singleTypeAttacker)
                {
                    return Effectiveness.EffectiveSingle;
                }
                else
                {
                    return Effectiveness.EffectiveDual;
                }
            case 2:
                if (singleTypeAttacker)
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
