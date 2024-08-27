using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreatureHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI typeText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI energyText;
    [SerializeField] TextMeshProUGUI strengthText;
    [SerializeField] TextMeshProUGUI magicText;
    [SerializeField] TextMeshProUGUI skillText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI defenseText;
    [SerializeField] TextMeshProUGUI resistanceText;
    [SerializeField] HPBar hpBar;
    [SerializeField] EnergyBar energyBar;
    [SerializeField] GameObject creatureInfoPanel;

    public void SetData(Creature creature)
    {
        nameText.text = creature.Nickname;
        levelText.text = "Level: " + creature.Level;
        float hp = ((float)creature.HP / creature.MaxHP);
        float energy = ((float)creature.Energy / creature.MaxEnergy);

        UpdateHP(hp);
        UpdateEnergy(energy);

        if (creature.Species.Type1 == creature.Species.Type2)
        {
            typeText.text = $"{creature.Species.Type1}";
        }
        else
        {
            typeText.text = $"{creature.Species.Type1}/{creature.Species.Type2}";
        }
        hpText.text = $"HP: {creature.HP}/{creature.MaxHP}";
        energyText.text = $"Energy: {creature.Energy}/{creature.MaxEnergy}";
        strengthText.text = $"Strength: {creature.Strength}";
        magicText.text = $"Magic: {creature.Magic}";
        skillText.text = $"Skill: {creature.Skill}";
        speedText.text = $"Speed: {creature.Speed}";
        defenseText.text = $"Defense: {creature.Defense}";
        resistanceText.text = $"Resistance: {creature.Resistance}";
        EnableCreatureInfoPanel(false);
    }

    public void UpdateHP(float hp)
    {
        hpBar.SetHP(hp);
    }

    public void UpdateEnergy(float energy)
    {
        energyBar.SetEnergy(energy);
    }

    public void EnableCreatureInfoPanel(bool enabled)
    {
        creatureInfoPanel.SetActive(enabled);
    }
}
