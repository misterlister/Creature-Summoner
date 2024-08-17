using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CoreAction", menuName = "Talents/Create new Core Action")]
public class CoreActionBase : ActionBase
{
    [SerializeField] int energyGain = 40;

    public int EnergyGain => energyGain;
}