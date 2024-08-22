using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreatureHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] EnergyBar energyBar;

    public void SetData(Creature creature)
    {
        nameText.text = creature.Nickname;
        levelText.text = "Level: " + creature.Level;
        hpBar.SetHP((float) creature.HP / creature.MaxHP);
        energyBar.SetEnergy((float)creature.Energy / creature.MaxEnergy);
    }
}
