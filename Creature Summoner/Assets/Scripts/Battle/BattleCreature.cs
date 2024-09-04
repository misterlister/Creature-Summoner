using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCreature : MonoBehaviour
{
    [SerializeField] CreatureBase species; // For Testing
    [SerializeField] int level; // For Testing
    [SerializeField] CreatureHud hud;
    [SerializeField] GameObject creatureSprite;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] bool ignore;

    public Creature CreatureInstance { get; set; } = null;
    public int Initiative { get; private set; }
    public bool IsDefeated { get; private set; } = false;

    public CreatureHud Hud => hud;
    public bool IsPlayerUnit => isPlayerUnit;
    public bool Ignore => ignore;

    private const int DEFAULT_SPRITE_SIZE = 80;
    private const int SIZE_CATEGORY_DIFF = 10;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Setup() // Add parameters for species and level later
    {
        CreatureInstance = new Creature(species, level);

        gameObject.SetActive(true);

        IsDefeated = false;

        Image creatureImage = creatureSprite.GetComponent<Image>();
        creatureImage.sprite = CreatureInstance.Species.FrontSprite;

        // Set sprite size
        RectTransform spriteRectTransform = creatureSprite.GetComponent<RectTransform>();
        SetSpriteSize(spriteRectTransform, CreatureInstance.Species.Size);
        
        if (IsPlayerUnit) 
        {
            ReverseSpriteDirection();
        }

        hud.SetData(CreatureInstance);
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        CreatureInstance = null;
        Initiative = 0;
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
    }
}

