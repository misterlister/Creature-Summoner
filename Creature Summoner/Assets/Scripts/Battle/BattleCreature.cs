using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCreature : MonoBehaviour
{
    [SerializeField] CreatureBase species;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] CreatureHud hud;
    [SerializeField] GameObject creatureSprite;

    public Creature CreatureInstance { get; set; }
    public CreatureHud Hud => hud;

    private const int DEFAULT_SPRITE_SIZE = 80;
    private const int SIZE_CATEGORY_DIFF = 10;

    public void Setup()
    {
        CreatureInstance = new Creature(species, level);
        Image creatureImage = creatureSprite.GetComponent<Image>();
        creatureImage.sprite = CreatureInstance.Species.FrontSprite;

        // Set sprite size
        RectTransform spriteRectTransform = creatureSprite.GetComponent<RectTransform>();
        SetSpriteSize(spriteRectTransform, CreatureInstance.Species.Size);
        
        if (isPlayerUnit) 
        {
            ReverseSpriteDirection();
        }

        hud.SetData(CreatureInstance);
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
}

