using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBar : MonoBehaviour
{
    [SerializeField] GameObject energy;

    public void SetEnergy(float energyNormalized)
    {
        energy.transform.localScale = new Vector3(energyNormalized, 1f);
    }
}
