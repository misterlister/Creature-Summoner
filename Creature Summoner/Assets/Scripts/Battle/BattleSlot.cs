using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

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
    
    [SerializeField] private Image spriteHolder;
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Slider xpBar;
    [SerializeField] private Image highlightAura;
    [SerializeField] private GameObject selectionArrowValid;
    [SerializeField] private GameObject selectionArrowInvalid;
    [SerializeField] private GameObject selectionArrowSelected;
    [SerializeField] private GameObject statusWindow;
    [SerializeField] private GameObject namePanel;
    [SerializeField] private GameObject horizontalArrangement;
    public Creature Creature { get; private set; }
    public bool IsPlayerSlot { get; private set; }
    public bool IsEmpty { get; private set; } = true;

    public int Initiative { get; private set; }
    public bool IsDefeated { get; private set; } = false;

    Image image;
    Vector3 originalPos;
    Color originalColor;

    public void Initialize(bool playerSlot)
    {
        IsPlayerSlot = playerSlot;
        if (!playerSlot)
        {
            ReverseSpriteDirection();
            ReverseSlotArrangement();
        }
    }
    public void Setup(Creature newCreature)
    {
        Creature = newCreature;
        UpdateUi();
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

        IsEmpty = true;
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

            hpBar.value = Creature.HP / (float)Creature.MaxHP;
            energyBar.value = Creature.Energy / (float)Creature.MaxEnergy;
            xpBar.value = Creature.XP / 100; // TEMP SIMPLIFIED VALUE
            spriteHolder.gameObject.SetActive(true);
            hpBar.gameObject.SetActive(true);
            if (IsPlayerSlot)
            {
                energyBar.gameObject.SetActive(true);
                xpBar.gameObject.SetActive(true);
            }
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
        statusWindow.SetActive(enabled);
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
        HorizontalLayoutGroup horizontalLayoutGroup = horizontalArrangement.GetComponent<HorizontalLayoutGroup>();
        horizontalLayoutGroup.reverseArrangement = false;
    }



    public void RollInitiative()
    {
        Initiative = Random.Range(Creature.Speed / 2, Creature.Speed);
    }


    public void Defeated()
    {
        IsDefeated = true;
        PlayDisperseAnimation();
        //hud.HideBars();
    }

    public void AddHP(int amount)
    {
        Creature.AddHP(amount);
    }

    public void RemoveHP(int amount)
    {
        Creature.RemoveHP(amount);
        if (Creature.IsDefeated)
        {
            Defeated();
        }
    }

    public void HitByAttack(int damage)
    {
        PlayHitAnimation();
        RemoveHP(damage);
    }

    public void AddEnergy(int amount)
    {
        Creature.AddEnergy(amount);
    }

    public void RemoveEnergy(int amount)
    {
        Creature.RemoveEnergy(amount);
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



public void Reset()
{
    creatureSprite.gameObject.SetActive(false);
    Empty = true;
    Creature = null;
    Initiative = 0;
    hud.HideBars();
}

*/