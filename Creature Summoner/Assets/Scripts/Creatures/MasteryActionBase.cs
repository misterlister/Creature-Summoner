using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MasteryAction", menuName = "Talents/Create new Mastery Action")]
public class MasteryActionBase : ActionBase
{
    [SerializeField] int masteryCost;

    public int MasteryCost => masteryCost;
}
