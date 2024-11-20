using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    VeryIneffective,
    Ineffective,
    Neutral,
    Effective,
    VeryEffective
}

public class TypeChart
{
    public static Dictionary<Effectiveness, float> EffectiveMod = new Dictionary<Effectiveness, float>
    {
        {Effectiveness.VeryIneffective, 0.44f},
        {Effectiveness.Ineffective, 0.66f},
        {Effectiveness.Neutral, 1f},
        {Effectiveness.Effective, 1.5f},
        {Effectiveness.VeryEffective, 2.25f}
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
        CreatureType defenseType1 = CreatureType.None, 
        CreatureType defenseType2 = CreatureType.None)
    {
        int type1Effect = chart[(int)attackType][(int)defenseType1];
        int type2Effect = chart[(int)attackType][(int)defenseType2];
        int combinedEffect = type1Effect + type2Effect;

        switch (combinedEffect)
        {
            case -2:
                return Effectiveness.VeryIneffective;
            case -1:
                return Effectiveness.Ineffective;
            case 1:
                return Effectiveness.Effective;
            case 2:
                return Effectiveness.VeryEffective;
            default:
                return Effectiveness.Neutral;
        }
    }
}