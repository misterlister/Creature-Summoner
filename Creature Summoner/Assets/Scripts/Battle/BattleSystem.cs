using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleCreature playerFrontMid;
    [SerializeField] BattleCreature enemyFrontMid;

    private void Start()
    {
        SetupBattle();
    }

    public void SetupBattle()
    {
        playerFrontMid.Setup();
        enemyFrontMid.Setup();
    }

}
