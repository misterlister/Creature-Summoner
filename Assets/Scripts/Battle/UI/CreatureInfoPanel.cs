using Game.Statuses;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattleUIConstants;

public class CreatureInfoPanel : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI speciesText;
    [SerializeField] private TextMeshProUGUI classText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Elements")]
    [SerializeField] private GameObject element1Container;
    [SerializeField] private Image element1Icon;
    [SerializeField] private TextMeshProUGUI element1Text;
    [SerializeField] private GameObject element2Container;
    [SerializeField] private Image element2Icon;
    [SerializeField] private TextMeshProUGUI element2Text;
    [SerializeField] private ElementDefinitionLibrary elementLibrary;

    [Header("Resources")]
    [SerializeField] private StatRow hpRow;
    [SerializeField] private StatRow shieldRow;
    [SerializeField] private StatRow energyRow;

    [Header("Combat Stats")]
    [SerializeField] private StatRow strengthRow;
    [SerializeField] private StatRow magicRow;
    [SerializeField] private StatRow skillRow;
    [SerializeField] private StatRow speedRow;
    [SerializeField] private StatRow defenseRow;
    [SerializeField] private StatRow resistanceRow;

    [Header("Traits")]
    [SerializeField] private Transform traitsContainer;
    [SerializeField] private TextMeshProUGUI traitTextPrefab;
    private List<TextMeshProUGUI> spawnedTraitTexts = new();


    [Header("Status Effects")]
    [SerializeField] private Transform boonsContainer;
    [SerializeField] private Transform banesContainer;
    [SerializeField] private StatusEffectIcon statusIconPrefab;
    [SerializeField] private StatusDefinitionLibrary statusLibrary;

    [Header("Views")]
    [SerializeField] private GameObject statsView;
    [SerializeField] private GameObject modifiersView;
    [SerializeField] private Button viewToggleButton;
    [SerializeField] private TextMeshProUGUI toggleButtonLabel;

    [Header("Elemental Resistances")]
    [SerializeField] private Transform elementalResistancesContainer;
    private Dictionary<CreatureElement, StatRow> elementalResistRows = new();

    [Header("Modifiers")]
    [SerializeField] private Transform modifiersContainer;
    [SerializeField] private StatRow rowPrefab;

    private Dictionary<CombatModifierType, StatRow> modifierRows = new();
    private List<StatRow> spawnedTraitRows = new();
    private List<StatusEffectIcon> spawnedStatusIcons = new();

    private Creature boundCreature;
    private bool showingModifiers;

    // -------------------------------------------------------

    private static readonly Dictionary<CreatureElement, CombatModifierType> elementToModifierType = new()
    {
        { CreatureElement.Fire, CombatModifierType.FireResist },
        { CreatureElement.Water, CombatModifierType.WaterResist },
        { CreatureElement.Earth, CombatModifierType.EarthResist },
        { CreatureElement.Air, CombatModifierType.AirResist },
        { CreatureElement.Radiant, CombatModifierType.RadiantResist },
        { CreatureElement.Necrotic, CombatModifierType.NecroticResist },
        { CreatureElement.Beast, CombatModifierType.BeastResist },
        { CreatureElement.Plant, CombatModifierType.PlantResist },
        { CreatureElement.Electric, CombatModifierType.ElectricResist },
        { CreatureElement.Metal, CombatModifierType.MetalResist },
        { CreatureElement.Arcane, CombatModifierType.ArcaneResist },
        { CreatureElement.Cold, CombatModifierType.ColdResist },
    };

    // -------------------------------------------------------

    private void Awake()
    {
        viewToggleButton.onClick.AddListener(ToggleView);
        PreSpawnElementalResistRows();
        PreSpawnModifierRows();
        SetupStatRows();
    }

    private void SetupStatRows()
    {
        hpRow.SetupRow("HP", false);
        shieldRow.SetupRow("Shield", true);
        energyRow.SetupRow("Energy", false);
        strengthRow.SetupRow("STR", false);
        magicRow.SetupRow("MAG", false);
        skillRow.SetupRow("SKL", false);
        speedRow.SetupRow("SPD", false);
        defenseRow.SetupRow("DEF", false);
        resistanceRow.SetupRow("RES", false);
    }

    private void PreSpawnElementalResistRows()
    {
        foreach (CreatureElement element in System.Enum.GetValues(typeof(CreatureElement)))
        {
            if (element == CreatureElement.None) continue;
            var row = Instantiate(rowPrefab, elementalResistancesContainer);
            row.SetupRow(element.ToString(), true);
            elementalResistRows[element] = row;
        }
    }

    private void PreSpawnModifierRows()
    {
        foreach (CombatModifierType modType in System.Enum.GetValues(typeof(CombatModifierType)))
        {
            var row = Instantiate(rowPrefab, modifiersContainer);
            row.SetupRow(FormatModifierName(modType), true);
            row.gameObject.SetActive(false);
            modifierRows[modType] = row;
        }
    }

    // -------------------------------------------------------

    public void Bind(Creature creature)
    {
        if (boundCreature != null)
        {
            boundCreature.OnHPChanged -= RefreshHP;
            boundCreature.OnEnergyChanged -= RefreshEnergy;
            boundCreature.OnShieldChanged -= RefreshShield;
            boundCreature.OnStatusAdded -= RefreshStatuses;
            boundCreature.OnStatusRemoved -= RefreshStatuses;
            boundCreature.OnStatsChanged -= RefreshStats;
            boundCreature.OnLevelUp -= RefreshLevel;
        }

        boundCreature = creature;

        if (creature == null) { ClearAll(); return; }

        creature.OnHPChanged += RefreshHP;
        creature.OnEnergyChanged += RefreshEnergy;
        creature.OnShieldChanged += RefreshShield;
        creature.OnStatusAdded += RefreshStatuses;
        creature.OnStatusRemoved += RefreshStatuses;
        creature.OnStatsChanged += RefreshStats;
        creature.OnLevelUp += RefreshLevel;

        showingModifiers = false;
        statsView.SetActive(true);
        modifiersView.SetActive(false);
        RefreshAll();
    }

    public void Clear() => Bind(null);

    // -------------------------------------------------------

    private void ToggleView()
    {
        showingModifiers = !showingModifiers;
        statsView.SetActive(!showingModifiers);
        modifiersView.SetActive(showingModifiers);
        toggleButtonLabel.text = showingModifiers ? "Stats" : "Modifiers";

        if (showingModifiers)
            RefreshModifiers();
        else
            RefreshStats();
    }

    // -------------------------------------------------------

    private void RefreshAll()
    {
        RefreshIdentity();
        RefreshStatuses();

        if (showingModifiers)
        { 
            RefreshElementalResistances();
            RefreshModifiers();
        }
        else
        { 
            RefreshStats(); 
        }
    }

    private void RefreshIdentity()
    {
        if (boundCreature == null) return;
        nicknameText.text = boundCreature.Nickname;
        speciesText.text = boundCreature.Species.CreatureName;
        classText.text = boundCreature.ClassName;
        levelText.text = $"Lv. {boundCreature.Level}";
        portrait.sprite = boundCreature.Species.Portrait;
        RefreshElements();
    }

    private void RefreshStats()
    {
        if (boundCreature == null || showingModifiers) return;

        hpRow.UpdateStats(boundCreature.MaxHP, boundCreature.HP);
        shieldRow.UpdateResource(boundCreature.Shielding, boundCreature.MaxHP);
        energyRow.UpdateStats(boundCreature.MaxEnergy, boundCreature.Energy);

        strengthRow.UpdateStats(boundCreature.Stats.GetBaseStat(StatType.Strength), boundCreature.Strength);
        magicRow.UpdateStats(boundCreature.Stats.GetBaseStat(StatType.Magic), boundCreature.Magic);
        skillRow.UpdateStats(boundCreature.Stats.GetBaseStat(StatType.Skill), boundCreature.Skill);
        speedRow.UpdateStats(boundCreature.Stats.GetBaseStat(StatType.Speed), boundCreature.Speed);
        defenseRow.UpdateStats(boundCreature.Stats.GetBaseStat(StatType.Defense), boundCreature.Defense);
        resistanceRow.UpdateStats(boundCreature.Stats.GetBaseStat(StatType.Resistance), boundCreature.Resistance);

        RefreshTraits();
    }

    private void RefreshElements()
    {
        if (boundCreature == null)
        {
            element1Container.SetActive(false);
            element2Container.SetActive(false);
            return;
        }

        SetElement(element1Container, element1Icon, element1Text, boundCreature.Species.Element1);

        bool hasSecondElement = boundCreature.Species.Element2 != CreatureElement.None
            && boundCreature.Species.Element2 != boundCreature.Species.Element1;

        element2Container.SetActive(hasSecondElement);
        if (hasSecondElement)
            SetElement(element2Container, element2Icon, element2Text, boundCreature.Species.Element2);
    }

    private void SetElement(GameObject container, Image icon, TextMeshProUGUI text, CreatureElement element)
    {
        container.SetActive(true);
        text.text = element.ToString();
        text.color = GameConstants.ElementColours.TryGetValue(element, out Color color)
            ? color
            : Color.black;
        icon.sprite = elementLibrary.GetIcon(element);
    }

    private void RefreshTraits()
    {
        foreach (var row in spawnedTraitRows)
            Destroy(row.gameObject);
        spawnedTraitRows.Clear();

        foreach (var trait in boundCreature.Traits)
        {
            var text = Instantiate(traitTextPrefab, traitsContainer);
            text.text = trait.TraitName;
            spawnedTraitTexts.Add(text);
        }
    }

    private void RefreshStatuses()
    {
        foreach (var icon in spawnedStatusIcons) Destroy(icon.gameObject);
        spawnedStatusIcons.Clear();

        if (boundCreature == null) return;

        foreach (var status in boundCreature.Statuses.GetAllBoons())
            SpawnStatusIcon(status, boonsContainer);

        foreach (var status in boundCreature.Statuses.GetAllBanes())
            SpawnStatusIcon(status, banesContainer);
    }

    private void SpawnStatusIcon(StatusEffect status, Transform container)
    {
        var icon = Instantiate(statusIconPrefab, container);
        icon.Setup(status, statusLibrary);
        spawnedStatusIcons.Add(icon);
    }

    private void RefreshElementalResistances()
    {
        foreach (var (element, row) in elementalResistRows)
        {
            float modifier = boundCreature.CombatModifiers.GetCombatModifier(elementToModifierType[element]);
            float damageTaken = (1f + modifier) * 100f;
            Color color = modifier < 0 ? NegativeColour
                        : modifier > 0 ? PositiveColour
                        : NeutralColour;
            row.UpdateSingleText($"{damageTaken:0}%", color);
        }
    }

    private void RefreshModifiers()
    {
        if (boundCreature == null) return;

        foreach (var (modType, row) in modifierRows)
        {
            float value = boundCreature.CombatModifiers.GetCombatModifier(modType);
            bool hasValue = !Mathf.Approximately(value, 0f);
            row.gameObject.SetActive(hasValue);
            if (hasValue) row.UpdateModifier(value);
        }
    }

    // -------------------------------------------------------
    // Event callbacks

    private void RefreshHP(int current, int max) => RefreshStats();
    private void RefreshShield(int current, int max) => RefreshStats();
    private void RefreshEnergy(int current, int max) => RefreshStats();
    private void RefreshLevel(int oldLevel, int newLevel) => RefreshIdentity();
    private void RefreshStatuses(StatusEffect _) => RefreshStatuses();

    // -------------------------------------------------------

    private void ClearAll()
    {
        nicknameText.text = "";
        speciesText.text = "";
        classText.text = "";
        levelText.text = "";
        portrait.sprite = null;

        RefreshElements();

        foreach (var row in spawnedTraitRows) Destroy(row.gameObject);
        spawnedTraitRows.Clear();

        foreach (var icon in spawnedStatusIcons) Destroy(icon.gameObject);
        spawnedStatusIcons.Clear();

        foreach (var row in modifierRows.Values) row.gameObject.SetActive(false);
    }

    private string FormatModifierName(CombatModifierType type)
    {
        string name = type.ToString().Replace("Mod", "").Replace("Resist", " Resist");
        return System.Text.RegularExpressions.Regex.Replace(name, "([A-Z])", " $1").Trim();
    }
}