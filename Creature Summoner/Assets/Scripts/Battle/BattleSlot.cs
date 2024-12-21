using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using TMPro;

public class BattleSlot : MonoBehaviour
{
    const float ATTACK_ANIMATION_DIST = 40f;
    const float ATTACK_ANIMATION_SPEED = 0.2f;
    const float HIT_ANIMATION_DUR = 0.1f;
    const float SUMMON_ANIMATION_DURATION = 1f;
    const float DISPERSE_ANIMATION_DURATION = 1.5f;

    private const int DEFAULT_SPRITE_SIZE = 60;
    private const int SIZE_CATEGORY_DIFF = 10;
    private const float AURA_SIZE_MOD = 1.5f;
    private const float SLIDER_CHANGE_DURATION = 0.5f;
    
    [SerializeField] private Image spriteHolder;
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Slider xpBar;
    [SerializeField] private Image highlightAura;
    [SerializeField] private GameObject selectionArrowValid;
    [SerializeField] private GameObject selectionArrowInvalid;
    [SerializeField] private GameObject selectionArrowSelected;
    [SerializeField] private GameObject statusWindow;
    [SerializeField] private TextMeshProUGUI nameField;
    [SerializeField] private TextMeshProUGUI levelField;
    [SerializeField] private GameObject namePanel;
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private GameObject horizontalArrangement;
    [SerializeField] private GameObject statRowPrefab;

    private List<GameObject> statRows = new List<GameObject>();

    public Creature Creature { get; private set; }
    public bool IsPlayerSlot { get; private set; }
    public bool IsEmpty { get; private set; } = true;

    public int Initiative { get; private set; }
    public bool IsDefeated { get; private set; } = false;

    public bool HPUpdating { get; private set; } = false;
    public bool EnergyUpdating { get; private set; } = false;
    public bool XPUpdating { get; private set; } = false;

    private Queue<float> HPUpdates = new Queue<float>();
    private Queue<float> EnergyUpdates = new Queue<float>();
    private Queue<float> XPUpdates = new Queue<float>();

    Image image;
    Vector3 originalPos;
    Color originalColor;

    public int Row { get; private set; }
    public int Col { get; private set; }

    public void Initialize(bool playerSlot, int row, int col)
    {
        Row = row; 
        Col = col;
        SetupStatusWindow();
        IsPlayerSlot = playerSlot;
        if (!playerSlot)
        {
            ReverseSlotArrangement();
        } 
        else
        {
            ReverseSpriteDirection();
        }
        ClearSlot();
    }
    public void Setup(Creature newCreature)
    {
        Creature = newCreature;
        UpdateUi();
        UpdateStats();
        if (newCreature != null)
        {
            PlaySummonCreatureAnimation();
        }
    }

    public void ClearSlot()
    {
        spriteHolder.sprite = null;
        spriteHolder.gameObject.SetActive(false);

        hpBar.value = 0;
        energyBar.value = 0;
        xpBar.value = 0;
        hpBar.gameObject.SetActive(false);
        energyBar.gameObject.SetActive(false);
        xpBar.gameObject.SetActive(false);

        ToggleHighlightAura(false);
        ToggleStatusWindow(false);
        UpdateSelectionArrow(SelectionArrowState.None);

        namePanel.SetActive(false);
        levelPanel.SetActive(false);

        IsEmpty = true;
    }

    public void UpdateStats()
    {
        if (Creature != null) {
            UpdateStatusWindow();
            levelField.text = Creature.Level.ToString();
        }
    }

    public void UpdateUi()
    {
        if (Creature != null)
        {
            spriteHolder.sprite = Creature.Species.FrontSprite;

            SetSpriteSize(spriteHolder.GetComponent<RectTransform>(), Creature.Species.Size);

            image = spriteHolder.GetComponent<Image>();
            image.sprite = Creature.Species.FrontSprite;

            originalColor = image.color;
            originalPos = image.transform.localPosition;

            nameField.text = Creature.Nickname;

            Creature.Energy = (int)(Creature.StartingEnergy * Creature.MaxEnergy);

            energyBar.gameObject.SetActive(IsPlayerSlot);
            xpBar.gameObject.SetActive(IsPlayerSlot);

            hpBar.gameObject.SetActive(true);

            hpBar.value = (Creature.HP / (float)Creature.MaxHP);
            energyBar.value = (Creature.Energy / (float)Creature.MaxEnergy);
            xpBar.value = (Creature.XP / 100); // TEMP SIMPLIFIED VALUE
            spriteHolder.gameObject.SetActive(true);

            namePanel.SetActive(true);
            levelPanel.SetActive(true);

            IsEmpty = false;
        }
        else
        {
            ClearSlot();
        }
    }

    public void ToggleHighlightAura(bool enabled)
    {
        highlightAura.gameObject.SetActive(enabled);
    }

    public void UpdateSelectionArrow(SelectionArrowState state)
    {
        // Disable all arrows initially
        selectionArrowValid.SetActive(false);
        selectionArrowInvalid.SetActive(false);
        selectionArrowSelected.SetActive(false);

        // Enable the appropriate arrow based on the state
        switch (state)
        {
            case SelectionArrowState.Valid:
                selectionArrowValid.SetActive(true);
                break;
            case SelectionArrowState.Invalid:
                selectionArrowInvalid.SetActive(true);
                break;
            case SelectionArrowState.Selected:
                selectionArrowSelected.SetActive(true);
                break;
            case SelectionArrowState.None:
                // No arrow is displayed, so do nothing
                break;
        }
    }

    public void ToggleStatusWindow(bool enabled)
    {
        if (!IsEmpty)
        {
            UpdateStatusWindow();
            statusWindow.SetActive(enabled);
        }
    }

    private void SetSpriteSize(RectTransform rectTransform, CreatureSize size)
    {
        int dimension = DEFAULT_SPRITE_SIZE;
        switch (size)
        {
            case CreatureSize.VerySmall:
                dimension += (-2 * SIZE_CATEGORY_DIFF);
                break;
            case CreatureSize.Small:
                dimension += (-1 * SIZE_CATEGORY_DIFF);
                break;
            case CreatureSize.Large:
                dimension += SIZE_CATEGORY_DIFF;
                break;
            case CreatureSize.ExtraLarge:
                dimension += (2 * SIZE_CATEGORY_DIFF);
                break;
            default:
                break;
        }
        rectTransform.sizeDelta = new Vector2(dimension, dimension);

        // Adjust the aura size to be slightly larger than the sprite
        RectTransform auraRectTransform = highlightAura.GetComponent<RectTransform>();
        float auraDimension = dimension * AURA_SIZE_MOD;
        auraRectTransform.sizeDelta = new Vector2(auraDimension, auraDimension);
    }


    private void ReverseSpriteDirection()
    {
        Vector3 newScale = spriteHolder.transform.localScale;
        newScale.x *= -1;
        spriteHolder.transform.localScale = newScale;
    }

    private void ReverseSlotArrangement()
    {
        // Reverse sprite and stat bar layout for enemies
        HorizontalLayoutGroup horizontalLayoutGroup = horizontalArrangement.GetComponent<HorizontalLayoutGroup>();
        horizontalLayoutGroup.reverseArrangement = true;

        // Reverse bar anchor direction for enemies
        GameObject barLayout = horizontalArrangement.transform.Find("Bars")?.gameObject;
        if (barLayout != null)
        {
            HorizontalLayoutGroup barLayoutGroup = barLayout.GetComponent<HorizontalLayoutGroup>();

            if (barLayoutGroup != null)
            {
                // Change child alignment to Middle Left
                horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            }
        }
    }

    private void SetupStatusWindow()
    {
        AddStatRow("Species", singleValue: true);
        AddStatRow("Type", singleValue: false);
        AddStatRow("HP", singleValue: true);
        AddStatRow("Energy", singleValue: true);
        AddStatRow("XP", singleValue: true);
        AddStatRow("--Stat--", singleValue: false);
        AddStatRow("Strength", singleValue: false);
        AddStatRow("Magic", singleValue: false);
        AddStatRow("Skill", singleValue: false);
        AddStatRow("Speed", singleValue: false);
        AddStatRow("Defense", singleValue: false);
        AddStatRow("Resistance", singleValue: false);
    }

    public void UpdateStatusWindow()
    {
        if (Creature != null)
        {
            statRows[0].GetComponent<StatRow>().UpdateSingleText(Creature.Species.CreatureName, Color.blue);
            statRows[1].GetComponent<StatRow>().UpdateType(Creature.Species.Type1, Creature.Species.Type2);
            statRows[2].GetComponent<StatRow>().UpdateResource(Creature.MaxHP, Creature.HP, Color.green);
            statRows[3].GetComponent<StatRow>().UpdateResource(Creature.MaxEnergy, Creature.Energy, Color.magenta);
            statRows[4].GetComponent<StatRow>().UpdateResource(100, Creature.XP, Color.gray); // PLACEHOLDER XP REQUIREMENT
            statRows[5].GetComponent<StatRow>().UpdateDoubleText("Base", "Modified");
            statRows[6].GetComponent<StatRow>().UpdateStats(Creature.Strength, Creature.Strength);
            statRows[7].GetComponent<StatRow>().UpdateStats(Creature.Magic, Creature.Magic);
            statRows[8].GetComponent<StatRow>().UpdateStats(Creature.Skill, Creature.Skill);
            statRows[9].GetComponent<StatRow>().UpdateStats(Creature.Speed, Creature.Speed);
            statRows[10].GetComponent<StatRow>().UpdateStats(Creature.Defense, Creature.Defense);
            statRows[11].GetComponent<StatRow>().UpdateStats(Creature.Resistance, Creature.Resistance);
        }
    }

    private void AddStatRow(string statName, bool singleValue)
    {
        GameObject newRow = Instantiate(statRowPrefab, statusWindow.transform);

        StatRow statRow = newRow.GetComponent<StatRow>();

        statRow.SetupRow(statName, singleValue);

        statRows.Add(newRow);
    }

    public void RollInitiative()
    {
        Initiative = Random.Range(Creature.Speed / 2, Creature.Speed);
    }
    public void Defeated()
    {
        IsDefeated = true;
        PlayDisperseAnimation();
    }
    public void HitByAttack(int damage)
    {
        PlayHitAnimation();
        AdjustHP(-damage);
    }


    public void AdjustHP(int amount)
    {
        if (amount > 0)
        {
            Creature.AddHP(amount);
        }
        else
        {
            Creature.RemoveHP(-amount);
        }
        
        HPUpdates.Enqueue(NormalizeInt(Creature.HP, Creature.MaxHP));
        if (!HPUpdating)
        {
            StartCoroutine(ProcessSliderUpdates(HPUpdates, hpBar, nameof(HPUpdating)));
        }
        if (Creature.IsDefeated)
        {
            Defeated();
        }
    }

    public void AdjustEnergy(int amount)
    {
        if (amount > 0)
        {
            Creature.AddEnergy(amount);
        }
        else
        {
            Creature.RemoveEnergy(-amount);
        }
        
        EnergyUpdates.Enqueue(NormalizeInt(Creature.Energy, Creature.MaxEnergy));
        if (!EnergyUpdating)
        {
            StartCoroutine(ProcessSliderUpdates(EnergyUpdates, energyBar, nameof(EnergyUpdating)));
        }
    }

    public void AddXP(int amount)
    {
        Creature.AddXP(amount);
        XPUpdates.Enqueue(NormalizeInt(Creature.XP, 100));
        if (!XPUpdating)
        {
            StartCoroutine(ProcessSliderUpdates(XPUpdates, xpBar, nameof(XPUpdating)));
        }
    }

    private IEnumerator ProcessSliderUpdates(Queue<float> updatesQueue, Slider slider, string updatingFlagName)
    {
        SetUpdatingFlag(updatingFlagName, true);

        while (updatesQueue.Count > 0)
        {
            // Get the new target value for the slider
            float targetValue = updatesQueue.Dequeue();

            // Ensure the target value stays within bounds
            targetValue = Mathf.Clamp(targetValue, 0f, 1f);

            // Smoothly transition the slider
            yield return StartCoroutine(SmoothSliderChange(slider, targetValue));
        }

        SetUpdatingFlag(updatingFlagName, false);
    }

    // Smooth slider transition
    private IEnumerator SmoothSliderChange(Slider slider, float targetValue)
    {
        float startValue = slider.value;
        float elapsedTime = 0f;

        while (elapsedTime < SLIDER_CHANGE_DURATION)
        {
            slider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / SLIDER_CHANGE_DURATION);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        slider.value = targetValue;
    }

    // Helper function to set the updating flags dynamically
    private void SetUpdatingFlag(string flagName, bool value)
    {
        switch (flagName)
        {
            case nameof(HPUpdating):
                HPUpdating = value;
                break;
            case nameof(EnergyUpdating):
                EnergyUpdating = value;
                break;
            case nameof(XPUpdating):
                XPUpdating = value;
                break;
        }
    }

    public void PlaySummonCreatureAnimation()
    {
        // Set the sprite to transparent to allow fade-in
        Color fadeInStartColor = originalColor;
        fadeInStartColor.a = 0;
        image.color = fadeInStartColor;
        image.DOFade(1f, SUMMON_ANIMATION_DURATION).SetEase(Ease.InOutQuad);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (IsPlayerSlot) 
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + ATTACK_ANIMATION_DIST, ATTACK_ANIMATION_SPEED));

        }
        else
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - ATTACK_ANIMATION_DIST, ATTACK_ANIMATION_SPEED));
        }

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, ATTACK_ANIMATION_SPEED));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, HIT_ANIMATION_DUR));
        sequence.Append(image.DOColor(originalColor, HIT_ANIMATION_DUR));
    }

    public void PlayDisperseAnimation()
    {
        image.DOFade(0f, DISPERSE_ANIMATION_DURATION).SetEase(Ease.InOutQuad);
    }

    public bool IsUpdating()
    {
        if (HPUpdating || EnergyUpdating || XPUpdating)
        {
            return true;
        }
        return false;
    }

    private float NormalizeInt(int value, int max)
    {
        return (float)value / max;
    }
}


public enum SelectionArrowState
{
    Valid,      // Arrow for valid targets
    Invalid,    // Arrow for invalid targets
    Selected,   // Arrow for already selected creatures
    None        // No arrow should be displayed
}


/*
public IEnumerator UpdateHud()
{
    yield return hud.UpdateHP();
    yield return hud.UpdateEnergy();
}
*/


/*

private void Awake()
{
    creatureSprite.gameObject.SetActive(false);
    Empty = true;
    Creature = null;
    hud.HideBars();
    if (IsPlayerUnit)
    {
        ReverseSpriteDirection();
    }
}

public void Setup(Creature creature)
{
    Creature = creature;

    Empty = false;

    hud.ShowBars();

    Select(false);

    IsDefeated = false;

    creatureSprite.gameObject.SetActive(true);
    image = creatureSprite.GetComponent<Image>();
    image.sprite = Creature.Species.FrontSprite;

    originalColor = image.color;

    // Set the sprite to transparent to allow fade-in
    Color fadeInStartColor = originalColor;
    fadeInStartColor.a = 0;
    image.color = fadeInStartColor;

    originalPos = image.transform.localPosition;

    // Set sprite size
    RectTransform spriteRectTransform = creatureSprite.GetComponent<RectTransform>();
    SetSpriteSize(spriteRectTransform, CreatureInstance.Species.Size);

    PlaySummonCreatureAnimation();

    hud.SetData(Creature);
}

*/