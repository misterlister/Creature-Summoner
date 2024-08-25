using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start,
    PlayerActionCategorySelect,
    PlayerCoreActionSelect,
    PlayerEmpoweredActionSelect,
    PlayerMasteryActionSelect,
    PlayerMoveSelect,
    EnemyAction,
    Busy
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleCreature playerFrontMid;
    [SerializeField] BattleCreature enemyFrontMid;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] GameObject actionCategories;

    KeyCode acceptKey = KeyCode.Z;

    BattleState state;
    int currentAction;

    private void Start()
    {
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerFrontMid.Setup();
        enemyFrontMid.Setup();
        yield return dialogBox.TypeDialog($"You have been attacked by a {enemyFrontMid.CreatureInstance.Species.CreatureName}!");
        yield return new WaitForSeconds(1f);

        PlayerActionCategorySelect();
    }

    void PlayerActionCategorySelect()
    {
        state = BattleState.PlayerActionCategorySelect;
        StartCoroutine(dialogBox.TypeDialog("Choose which kind of action to take"));
        dialogBox.EnableActionCategorySelect(true);
    }

    void PlayerActionSelect(BattleState battleState)
    {
        state = battleState;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelect(true);
    }

    private void Update()
    {
        if (state == BattleState.PlayerActionCategorySelect)
        {
            HandleActionCategorySelection();
        }
    }

    void HandleActionCategorySelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction == 0 || currentAction == 1)
            {
                currentAction += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction == 2 || currentAction == 3)
            {
                currentAction -= 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction == 1 || currentAction == 3)
            {
                currentAction--;
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction == 0 || currentAction == 2)
            {
                currentAction++;
            }
        }
        dialogBox.UpdateActionCategorySelection(currentAction);

        if (Input.GetKeyDown(acceptKey))
        {
            if (currentAction == 0)
            {
                PlayerActionSelect(BattleState.PlayerCoreActionSelect);
            }
            else if (currentAction == 1)
            {
                PlayerActionSelect(BattleState.PlayerEmpoweredActionSelect);
            }
            else if (currentAction == 2)
            {
                PlayerActionSelect(BattleState.PlayerMasteryActionSelect);
            }
            else if (currentAction == 3)
            {
                //PlayerActionSelect(BattleState.PlayerMoveSelect);
            }
        }
    }

}
