using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Displays detailed information about a creature.
/// Shows stats, HP/Energy, status effects, and other battle info.
/// Typically shown when hovering over or examining a creature.
/// </summary>
public class CreatureStatusWindow : MonoBehaviour
{
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI creatureNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI classText;

    [Header("Type/Element")]
    [SerializeField] private Image element1Image;
    [SerializeField] private Image element2Image;
    [SerializeField] private TextMeshProUGUI element1Text;
    [SerializeField] private TextMeshProUGUI element2Text;

    [Header("Resources")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI xpText;

    [Header("Stats")]
    [SerializeField] private Transform statsContainer;
    [SerializeField] private GameObject statRowPrefab;

    [Header("Status Effects")]
    [SerializeField] private Transform statusEffectsContainer;
    [SerializeField] private GameObject statusEffectIconPrefab;

    [Header("Additional Info")]
    [SerializeField] private TextMeshProUGUI coverStatusText;

    private List<GameObject> statRows = new List<GameObject>();
    private List<GameObject> statusEffectIcons = new List<GameObject>();
    private Creature currentCreature;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Show the status window with information about a creature.
    /// </summary>
    public void Show(Creature creature)
    {
        if (creature == null)
        {
            Hide();
            return;
        }

        currentCreature = creature;
        UpdateDisplay();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Update the creature being displayed (if already showing).
    /// </summary>
    public void UpdateCreature(Creature creature)
    {
        currentCreature = creature;
        if (gameObject.activeSelf)
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// Hide the status window.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        currentCreature = null;
    }

    /// <summary>
    /// Clear all displayed information.
    /// </summary>
    public void Clear()
    {
        Hide();
    }

    private void UpdateDisplay()
    {
        if (currentCreature == null) return;

        UpdateBasicInfo();
        UpdateElements();
        UpdateResources();
        UpdateStats();
        UpdateStatusEffects();
        UpdateAdditionalInfo();
    }

    private void UpdateBasicInfo()
    {
        if (creatureNameText != null)
            creatureNameText.text = currentCreature.Nickname;

        if (levelText != null)
            levelText.text = $"Lv. {currentCreature.Level}";

        if (classText != null)
            classText.text = currentCreature.ClassName;
    }

    private void UpdateElements()
    {
        if (currentCreature.Species == null) return;

        // Primary element
        if (element1Text != null)
            element1Text.text = currentCreature.Species.Element1.ToString();

        // Secondary element (may be None)
        if (element2Text != null && element2Image != null)
        {
            if (currentCreature.Species.Element2 != CreatureElement.None)
            {
                element2Text.text = currentCreature.Species.Element2.ToString();
                element2Image.gameObject.SetActive(true);
            }
            else
            {
                element2Image.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateResources()
    {
        // HP
        if (hpSlider != null)
        {
            hpSlider.maxValue = currentCreature.MaxHP;
            hpSlider.value = currentCreature.HP;
        }
        if (hpText != null)
            hpText.text = $"{currentCreature.HP}/{currentCreature.MaxHP}";

        // Energy
        if (energySlider != null)
        {
            energySlider.maxValue = currentCreature.MaxEnergy;
            energySlider.value = currentCreature.Energy;
        }
        if (energyText != null)
            energyText.text = $"{currentCreature.Energy}/{currentCreature.MaxEnergy}";

        // XP
        if (xpSlider != null)
        {
            xpSlider.maxValue = 100; // TODO: Use actual XP requirement
            xpSlider.value = currentCreature.XP;
        }
        if (xpText != null)
            xpText.text = $"XP: {currentCreature.XP}";
    }

    private void UpdateStats()
    {
        // Clear existing stat rows
        foreach (var row in statRows)
        {
            if (row != null)
                Destroy(row);
        }
        statRows.Clear();

        if (statRowPrefab == null || statsContainer == null) return;

        // Add header row
        AddHeaderRow("--Stat--", "Base", "Modified");

        // Create stat rows (shows base and modified values)
        AddStatRow("STR", currentCreature.BaseStrength, currentCreature.Strength);
        AddStatRow("MAG", currentCreature.BaseMagic, currentCreature.Magic);
        AddStatRow("SKL", currentCreature.BaseSkill, currentCreature.Skill);
        AddStatRow("SPD", currentCreature.BaseSpeed, currentCreature.Speed);
        AddStatRow("DEF", currentCreature.BaseDefense, currentCreature.Defense);
        AddStatRow("RES", currentCreature.BaseResistance, currentCreature.Resistance);
    }

    private void AddHeaderRow(string label, string leftHeader, string rightHeader)
    {
        GameObject row = Instantiate(statRowPrefab, statsContainer);
        var statRowComponent = row.GetComponent<StatRow>();

        if (statRowComponent != null)
        {
            statRowComponent.SetupRow(label, singleField: false);
            statRowComponent.UpdateDoubleText(leftHeader, rightHeader);
        }

        statRows.Add(row);
    }

    private void AddStatRow(string statName, int baseStat, int modifiedStat)
    {
        GameObject row = Instantiate(statRowPrefab, statsContainer);
        var statRowComponent = row.GetComponent<StatRow>();

        if (statRowComponent != null)
        {
            statRowComponent.SetupRow(statName, singleField: false);
            statRowComponent.UpdateStats(baseStat, modifiedStat);
        }

        statRows.Add(row);
    }

    private void UpdateStatusEffects()
    {
        // Clear existing status effect icons
        foreach (var icon in statusEffectIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        statusEffectIcons.Clear();

        if (statusEffectIconPrefab == null || statusEffectsContainer == null) return;

        // TODO: When you implement status effects, add them here
        // Example:
        // foreach (var status in currentCreature.ActiveStatusEffects)
        // {
        //     GameObject icon = Instantiate(statusEffectIconPrefab, statusEffectsContainer);
        //     // Set icon sprite, tooltip, etc.
        //     statusEffectIcons.Add(icon);
        // }
    }

    private void UpdateAdditionalInfo()
    {
        // Show cover status if applicable
        if (coverStatusText != null)
        {
            // TODO: This requires UnifiedBattlefield reference
            // For now, just hide it
            coverStatusText.gameObject.SetActive(false);

            // When implemented:
            // string coverStatus = CombatUtilities.GetCoverStatusDescription(currentCreature, battlefield);
            // coverStatusText.text = coverStatus;
            // coverStatusText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Refresh the display (call when creature stats change).
    /// </summary>
    public void Refresh()
    {
        if (currentCreature != null && gameObject.activeSelf)
        {
            UpdateDisplay();
        }
    }
}