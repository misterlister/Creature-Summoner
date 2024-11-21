using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleCreature : MonoBehaviour
{
    const float ATTACK_ANIMATION_DIST = 40f;
    const float ATTACK_ANIMATION_SPEED = 0.2f;
    const float HIT_ANIMATION_DUR = 0.1f;
    const float SUMMON_ANIMATION_DURATION = 1f;
    const float DISPERSE_ANIMATION_DURATION = 1.5f;

    [SerializeField] CreatureBase species; // For Testing
    [SerializeField] int level; // For Testing
    [SerializeField] CreatureHud hud;
    [SerializeField] GameObject creatureSprite;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] bool ignore;
    [SerializeField] GameObject selectionAura;

    public Creature CreatureInstance { get; set; } = null;
    public int Initiative { get; private set; }
    public bool IsDefeated { get; private set; } = false;

    public CreatureHud Hud => hud;
    public bool IsPlayerUnit => isPlayerUnit;
    public bool Ignore => ignore;

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private const int DEFAULT_SPRITE_SIZE = 75;
    private const int SIZE_CATEGORY_DIFF = 10;
    private const float AURA_SIZE_MOD = 1.5f;

    public bool Empty { get; private set; } = false;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (species == null || ignore)
        {
            Empty = true;
        }
        originalPos = image.transform.localPosition;
        hud.HideBars();
    }

    public void Setup() // Add parameters for species and level later
    {
        CreatureInstance = new Creature(species, level);

        gameObject.SetActive(true);

        hud.ShowBars();

        Select(false);

        IsDefeated = false;

        image = creatureSprite.GetComponent<Image>();
        image.sprite = CreatureInstance.Species.FrontSprite;
        
        originalColor = image.color;

        // Set the sprite to transparent to allow fade-in
        Color fadeInStartColor = originalColor;
        fadeInStartColor.a = 0;
        image.color = fadeInStartColor;


        // Set sprite size
        RectTransform spriteRectTransform = creatureSprite.GetComponent<RectTransform>();
        SetSpriteSize(spriteRectTransform, CreatureInstance.Species.Size);

        if (IsPlayerUnit)
        {
            ReverseSpriteDirection();
        }

        PlaySummonCreatureAnimation();

        hud.SetData(CreatureInstance);
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        CreatureInstance = null;
        Initiative = 0;
        hud.HideBars();
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
        RectTransform auraRectTransform = selectionAura.GetComponent<RectTransform>();
        float auraDimension = dimension * AURA_SIZE_MOD;
        auraRectTransform.sizeDelta = new Vector2(auraDimension, auraDimension);
    }

    private void ReverseSpriteDirection()
    {
        Vector3 newScale = creatureSprite.transform.localScale;
        newScale.x *= -1;
        creatureSprite.transform.localScale = newScale;
    }

    public void RollInitiative()
    {
        Initiative = Random.Range(CreatureInstance.Speed / 2, CreatureInstance.Speed);
    }

    public void Defeated()
    {
        IsDefeated = true;
        PlayDisperseAnimation();
        hud.HideBars();
    }

    public void Select(bool enabled)
    {
        selectionAura.SetActive(enabled);
    }

    public void AddHP(int amount)
    {
        CreatureInstance.AddHP(amount);
    }

    public void RemoveHP(int amount)
    {
        CreatureInstance.RemoveHP(amount);
        if (CreatureInstance.IsDefeated)
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
        CreatureInstance.AddEnergy(amount);
    }

    public void RemoveEnergy(int amount)
    {
        CreatureInstance.RemoveEnergy(amount);
    }

    public IEnumerator UpdateHud()
    {
        yield return hud.UpdateHP();
        yield return hud.UpdateEnergy();
    }

    public void PlaySummonCreatureAnimation()
    {
        image.DOFade(1f, SUMMON_ANIMATION_DURATION).SetEase(Ease.InOutQuad);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit) 
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

