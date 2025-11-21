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
    const float HEAL_ANIMATION_DURATION = 0.5f;

    private const int DEFAULT_SPRITE_SIZE = 60;
    private const int SIZE_CATEGORY_DIFF = 10;
    private const float AURA_SIZE_MOD = 1.5f;
    private const float SLIDER_CHANGE_DURATION = 0.5f;
    
    [SerializeField] private Image spriteHolder;
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Slider xpBar;

    [SerializeField] private Image highlightAura;
    [SerializeField] private Sprite activeCreatureAura;
    [SerializeField] private Sprite negativeTargetAura;
    [SerializeField] private Sprite positiveTargetAura;
    [SerializeField] private Sprite moveTargetAura;

    [SerializeField] private Image selectionArrowImage;
    [SerializeField] private Sprite validArrowSprite;
    [SerializeField] private Sprite invalidArrowSprite;
    [SerializeField] private Sprite selectedArrowSprite;

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
    public bool ValidTarget { get; private set; }

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
            PositionStatusWindowForEnemy();
        } 
        else
        {
            ReverseSpriteDirection();
            PositionStatusWindowForPlayer();
        }
        ClearSlot();
    }
    public void Setup(Creature newCreature)
    {
        ClearSlot();
        if (newCreature == null)
        {
            return;
        }
        Creature = newCreature;
        newCreature.SetBattleSlot(this);
        UpdateUi();
        UpdateStats();
        PlaySummonCreatureAnimation();
    }

    public void ClearSlot()
    {
        if (Creature != null)
        {
            Creature.SetBattleSlot(null);
        }
        Creature = null;
        spriteHolder.sprite = null;
        spriteHolder.gameObject.SetActive(false);

        hpBar.value = 0;
        energyBar.value = 0;
        xpBar.value = 0;
        hpBar.gameObject.SetActive(false);
        energyBar.gameObject.SetActive(false);
        xpBar.gameObject.SetActive(false);

        ToggleStatusWindow(false);
        UpdateHighlightAura(HighlightAuraState.None);
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

    public void UpdateHighlightAura(HighlightAuraState state)
    {
        switch (state)
        {
            case HighlightAuraState.Active:
                highlightAura.sprite = activeCreatureAura;
                highlightAura.gameObject.SetActive(true);
                ToggleValidTarget(enabled: false);
                break;
            case HighlightAuraState.Negative:
                highlightAura.sprite = negativeTargetAura;
                highlightAura.gameObject.SetActive(true);
                ToggleValidTarget(enabled: true);
                break;
            case HighlightAuraState.Positive:
                highlightAura.sprite = positiveTargetAura;
                highlightAura.gameObject.SetActive(true);
                ToggleValidTarget(enabled: true);
                break;
            case HighlightAuraState.Move:
                highlightAura.sprite = moveTargetAura;
                highlightAura.gameObject.SetActive(true);
                ToggleValidTarget(enabled: true);
                break;
            case HighlightAuraState.None:
                highlightAura.gameObject.SetActive(false); // Hide the image entirely
                ToggleValidTarget(enabled: false);
                break;
        }
    }

    public void UpdateSelectionArrow(SelectionArrowState state)
    {
        switch (state)
        {
            case SelectionArrowState.Valid:
                selectionArrowImage.sprite = validArrowSprite;
                selectionArrowImage.gameObject.SetActive(true); // Ensure the image is visible
                break;
            case SelectionArrowState.Invalid:
                selectionArrowImage.sprite = invalidArrowSprite;
                selectionArrowImage.gameObject.SetActive(true);
                break;
            case SelectionArrowState.Selected:
                selectionArrowImage.sprite = selectedArrowSprite;
                selectionArrowImage.gameObject.SetActive(true);
                break;
            case SelectionArrowState.None:
                selectionArrowImage.gameObject.SetActive(false); // Hide the image entirely
                break;
        }
    }

public void ToggleValidTarget(bool enabled)
    {
        ValidTarget = enabled;
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

    private void PositionStatusWindowForPlayer()
    {
        // Position statusWindow to the right of the sprite for player units
        RectTransform statusRect = statusWindow.GetComponent<RectTransform>();

        // Anchor to the right side of the slot
        statusRect.anchorMin = new Vector2(1f, 0.5f);
        statusRect.anchorMax = new Vector2(1f, 0.5f);
        statusRect.pivot = new Vector2(0f, 0.5f); // Left edge of statusWindow aligns with anchor

        // Position it so it extends to the right
        statusRect.anchoredPosition = new Vector2(0f, 0f);
    }

    private void PositionStatusWindowForEnemy()
    {
        // Position statusWindow to the left of the sprite for enemy units
        RectTransform statusRect = statusWindow.GetComponent<RectTransform>();

        // Anchor to the left side of the slot
        statusRect.anchorMin = new Vector2(0f, 0.5f);
        statusRect.anchorMax = new Vector2(0f, 0.5f);
        statusRect.pivot = new Vector2(1f, 0.5f); // Right edge of statusWindow aligns with anchor

        // Position it so it extends to the left
        statusRect.anchoredPosition = new Vector2(0f, 0f);
    }

    private void SetupStatusWindow()
    {
        AddStatRow("Species", singleValue: true);
        AddStatRow("Type", singleValue: false);
        AddStatRow("Level", singleValue: true);
        AddStatRow("Class", singleValue: true);
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
            statRows[2].GetComponent<StatRow>().UpdateSingleText(Creature.Level.ToString(), Color.black);
            statRows[3].GetComponent<StatRow>().UpdateSingleText(Creature.ClassName, Color.magenta);
            statRows[4].GetComponent<StatRow>().UpdateResource(Creature.MaxHP, Creature.HP, GameConstants.HP_COLOUR);
            statRows[5].GetComponent<StatRow>().UpdateResource(Creature.MaxEnergy, Creature.Energy, GameConstants.ENERGY_COLOUR);
            statRows[6].GetComponent<StatRow>().UpdateResource(100, Creature.XP, GameConstants.XP_COLOUR); // PLACEHOLDER XP REQUIREMENT
            statRows[7].GetComponent<StatRow>().UpdateDoubleText("Base", "Modified");
            statRows[8].GetComponent<StatRow>().UpdateStats(Creature.BaseStrength, Creature.Strength);
            statRows[9].GetComponent<StatRow>().UpdateStats(Creature.BaseMagic, Creature.Magic);
            statRows[10].GetComponent<StatRow>().UpdateStats(Creature.BaseSkill, Creature.Skill);
            statRows[11].GetComponent<StatRow>().UpdateStats(Creature.BaseSpeed, Creature.Speed);
            statRows[12].GetComponent<StatRow>().UpdateStats(Creature.BaseDefense, Creature.Defense);
            statRows[13].GetComponent<StatRow>().UpdateStats(Creature.BaseResistance, Creature.Resistance);
        }
    }

    private void AddStatRow(string statName, bool singleValue)
    {
        GameObject newRow = Instantiate(statRowPrefab, statusWindow.transform);

        StatRow statRow = newRow.GetComponent<StatRow>();

        statRow.SetupRow(statName, singleValue);

        statRows.Add(newRow);
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

    public void HitByHealing(int healing)
    {
        PlayHealAnimation();
        AdjustHP(healing);
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
        
        HPUpdates.Enqueue(MathUtils.NormalizeInt(Creature.HP, Creature.MaxHP));
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
        
        EnergyUpdates.Enqueue(MathUtils.NormalizeInt(Creature.Energy, Creature.MaxEnergy));
        if (!EnergyUpdating)
        {
            StartCoroutine(ProcessSliderUpdates(EnergyUpdates, energyBar, nameof(EnergyUpdating)));
        }
    }

    public void AddXP(int amount)
    {
        Creature.AddXP(amount);
        XPUpdates.Enqueue(MathUtils.NormalizeInt(Creature.XP, 100));
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

    public void PlayHealAnimation()
    {
        var sequence = DOTween.Sequence();
        Vector3 originalScale = image.transform.localScale;

        // Define the healing color (green for a typical healing effect)
        Color healColor = Color.green;

        // Transition to the healing color and then back to the original color
        sequence.Append(image.DOColor(healColor, HEAL_ANIMATION_DURATION / 2));
        sequence.Append(image.DOColor(originalColor, HEAL_ANIMATION_DURATION / 2));

        // Add a scaling effect to emphasize the healing process (optional)
        sequence.Join(image.transform.DOScale(originalScale * 1.05f, HEAL_ANIMATION_DURATION / 2).SetEase(Ease.OutSine));
        sequence.Append(image.transform.DOScale(originalScale, HEAL_ANIMATION_DURATION / 2).SetEase(Ease.InSine));

        // (Optional) Add fade-in/fade-out to the color for a smoother effect
        sequence.Join(image.DOFade(0.8f, HEAL_ANIMATION_DURATION / 2));
        sequence.Append(image.DOFade(1f, HEAL_ANIMATION_DURATION / 2));
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

    public List<BattleSlot> GetAdjacentSlots()
    {
        List<BattleSlot> adjacentSlots = new List<BattleSlot>();
        // TODO : Implement logic to get adjacent slots based on the battle grid
        return adjacentSlots;
    }
}


public enum SelectionArrowState
{
    Valid,      // Arrow for valid targets
    Invalid,    // Arrow for invalid targets
    Selected,   // Arrow for already selected creatures
    None        // No arrow should be displayed
}

public enum HighlightAuraState
{
    Active,     // Aura to highlight the active creature
    Negative,   // Aura to highlight valid targets for offensive actions
    Positive,   // Aura to highlight valid targets for beneficial actions
    Move,       // Aura to highlight valid movement spaces
    None        // No aura should be displayed
}