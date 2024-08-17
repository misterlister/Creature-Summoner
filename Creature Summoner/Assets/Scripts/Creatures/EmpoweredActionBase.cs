using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmpoweredAction", menuName = "Talents/Create new Empowered Action")]
public class EmpoweredActionBase : ActionBase
{
    [SerializeField] int energyCost;

    public int EnergyCost => energyCost;
}