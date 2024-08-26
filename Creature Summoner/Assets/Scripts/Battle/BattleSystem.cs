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

    public BattleCreature activeCreature;

    KeyCode acceptKey = KeyCode.Z;
    KeyCode backKey = KeyCode.X;

    BattleState state;
    int selectionPosition;

    private void Start()
    {
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerFrontMid.Setup();
        enemyFrontMid.Setup();

        activeCreature = playerFrontMid;

        yield return dialogBox.TypeDialog($"You have been attacked by a {enemyFrontMid.CreatureInstance.Species.CreatureName}!");
        yield return new WaitForSeconds(1f);

        PlayerActionCategorySelect();
    }

    void PlayerActionCategorySelect()
    {
        state = BattleState.PlayerActionCategorySelect;
        StartCoroutine(dialogBox.TypeDialog("Choose which kind of action to take"));
        dialogBox.EnableActionCategorySelect(true);
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionSelect(false);
        selectionPosition = 0;
    }

    void PlayerActionSelect(BattleState battleState)
    {
        state = battleState;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelect(true);
        if (battleState == BattleState.PlayerCoreActionSelect)
        {
            dialogBox.SetCoreActionNames(activeCreature.CreatureInstance);
        }
        else if (battleState == BattleState.PlayerEmpoweredActionSelect)
        {
            dialogBox.SetEmpoweredActionNames(activeCreature.CreatureInstance);
        }
        selectionPosition = 0;
    }

    private void Update()
    {
        if (state == BattleState.PlayerActionCategorySelect 
            || state == BattleState.PlayerCoreActionSelect
            || state == BattleState.PlayerEmpoweredActionSelect)
        {
            HandleMenuSelection();
        }
    }

    void HandleMenuSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (selectionPosition == 0 || selectionPosition == 1)
            {
                selectionPosition += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (selectionPosition == 2 || selectionPosition == 3)
            {
                selectionPosition -= 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selectionPosition == 1 || selectionPosition == 3)
            {
                selectionPosition--;
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (selectionPosition == 0 || selectionPosition == 2)
            {
                selectionPosition++;
            }
        }
        
        if (state == BattleState.PlayerActionCategorySelect)
        {
            dialogBox.UpdateActionCategorySelection(selectionPosition);
            if (Input.GetKeyDown(acceptKey))
            {
                AcceptActionCategorySelection();
            }
        }
        else if (state == BattleState.PlayerCoreActionSelect)
        {
            dialogBox.UpdateActionSelection(selectionPosition);
            if (Input.GetKeyDown(acceptKey))
            {
                AcceptCoreActionSelection();
            }
            if (Input.GetKeyDown(backKey))
            {
                PlayerActionCategorySelect();
            }
        }
    }

    void AcceptActionCategorySelection()
    {
        if (selectionPosition == 0)
        {
            PlayerActionSelect(BattleState.PlayerCoreActionSelect);
        }
        else if (selectionPosition == 1)
        {
            PlayerActionSelect(BattleState.PlayerEmpoweredActionSelect);
        }
        else if (selectionPosition == 2)
        {
            PlayerActionSelect(BattleState.PlayerMasteryActionSelect);
        }
        else if (selectionPosition == 3)
        {
            //PlayerActionSelect(BattleState.PlayerMoveSelect);
        }
    }

    void AcceptCoreActionSelection()
    {
        if (selectionPosition == 3)
        {
            PlayerActionCategorySelect();
        }
        else if (selectionPosition < dialogBox.ActionText.Count && dialogBox.ActionText[selectionPosition].text != "-")
        {
            Debug.Log($"{activeCreature.CreatureInstance.Nickname} used {dialogBox.ActionText[selectionPosition].text}");
        }
    }
}
