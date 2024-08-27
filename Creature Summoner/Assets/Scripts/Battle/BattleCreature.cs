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

    public Creature CreatureInstance { get; set; }
    public CreatureHud Hud => hud;

    public void Setup()
    {
        CreatureInstance = new Creature(species, level);
        GetComponent<Image>().sprite = CreatureInstance.Species.FrontSprite;
        if (isPlayerUnit) 
        {
            //reverse direction
        }
        hud.SetData(CreatureInstance);
    }
}
