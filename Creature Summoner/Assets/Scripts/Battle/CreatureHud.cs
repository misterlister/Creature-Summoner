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
    [SerializeField] GameObject selectionArrow;
    [SerializeField] bool isEnemy;

    public GameObject SelectionArrow => selectionArrow;
    private Image selectionArrowImage;

    void Start()
    {
        // Get the Image component from the selection arrow GameObject
        selectionArrowImage = selectionArrow.GetComponent<Image>();

        // Ensure that the arrow has an Image component
        if (selectionArrowImage == null)
        {
            Debug.LogError("The selectionArrow does not have an Image component!");
        }
    }


    public void SetData(Creature creature)
    {
        nameText.text = creature.Nickname;
        levelText.text = "Level: " + creature.Level;

        UpdateHP(creature.HP, creature.MaxHP);
        UpdateEnergy(creature.Energy, creature.MaxEnergy);
        if (isEnemy)
        {
            energyBar.gameObject.SetActive(false);
        }

        if (creature.Species.Type1 == creature.Species.Type2)
        {
            typeText.text = $"{creature.Species.Type1}";
        }
        else
        {
            typeText.text = $"{creature.Species.Type1}/{creature.Species.Type2}";
        }
        if (!isEnemy)
        {
            strengthText.text = $"Strength: {creature.Strength}";
            magicText.text = $"Magic: {creature.Magic}";
            skillText.text = $"Skill: {creature.Skill}";
            speedText.text = $"Speed: {creature.Speed}";
            defenseText.text = $"Defense: {creature.Defense}";
            resistanceText.text = $"Resistance: {creature.Resistance}";
        }
        else
        {
            strengthText.text = $"Strength: ???";
            magicText.text = $"Magic: ???";
            skillText.text = $"Skill: ???";
            speedText.text = $"Speed: ???";
            defenseText.text = $"Defense: ???";
            resistanceText.text = $"Resistance: ???";
        }
        EnableCreatureInfoPanel(false);
    }

    public void UpdateHP(int currentHP, int maxHP)
    {
        float hp = ((float)currentHP / maxHP);
        hpBar.SetHP(hp);
        if (!isEnemy)
        {
            hpText.text = $"HP: {currentHP}/{maxHP}";
        }
        else
        {
            hpText.text = "HP: ???";
        }
    }

    public void UpdateEnergy(int currentEnergy, int maxEnergy)
    {
        float energy = ((float)currentEnergy / maxEnergy);
        energyBar.SetEnergy(energy);
        if (!isEnemy)
        {
            energyText.text = $"Energy: {currentEnergy}/{maxEnergy}";
        }
        else
        {
            hpText.text = "Energy: ???";
        }
    }

    public void EnableCreatureInfoPanel(bool enabled)
    {
        creatureInfoPanel.SetActive(enabled);
    }

    public void EnableSelectionArrow(bool enabled, bool isSelected = false)
    {
        selectionArrow.SetActive(enabled);

        // Set correct sprite image
        if (selectionArrowImage != null && enabled)
        {
            selectionArrowImage.sprite = isSelected
                ? CreatureHudManager.Instance.GetSelectionArrow()
                : CreatureHudManager.Instance.GetHighlightArrow();
        }
    }
}
