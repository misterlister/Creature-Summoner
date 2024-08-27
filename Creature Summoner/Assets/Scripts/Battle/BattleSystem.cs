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
    PlayerExamine,
    EnemyAction,
    Busy
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleCreature playerFrontMid;
    [SerializeField] BattleCreature enemyFrontMid;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] GameObject actionCategories;

    public Dictionary<BattleState, int> StateChoices { get; set; }

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
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionCategorySelect(true);
        dialogBox.EnableActionSelect(false);

        StateChoices = new Dictionary<BattleState, int>
        {
            { BattleState.Start, 0 },
            { BattleState.PlayerActionCategorySelect, dialogBox.ActionCategoryText.Count },
            { BattleState.PlayerCoreActionSelect, dialogBox.ActionText.Count },
            { BattleState.PlayerEmpoweredActionSelect, dialogBox.ActionText.Count },
            { BattleState.PlayerMasteryActionSelect, 2 },
            { BattleState.PlayerMoveSelect, 2 },
            { BattleState.PlayerExamine, 2 },
            { BattleState.EnemyAction, 0 },
            { BattleState.Busy, 0 }
        };

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
        if (state != BattleState.Start 
            && state != BattleState.EnemyAction
            && state != BattleState.Busy)
        {
            HandleMenuSelection(StateChoices[state]);
            HandleStateBasedInput();
        }
    }

    void HandleMenuSelection(int optionCount)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectionPosition = (selectionPosition + 2) % optionCount;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectionPosition = (selectionPosition - 2 + optionCount) % optionCount;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectionPosition = (selectionPosition - 1 + optionCount) % optionCount;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectionPosition = (selectionPosition + 1) % optionCount;
        }
    }

    void HandleStateBasedInput()
    {
        if (state == BattleState.PlayerActionCategorySelect)
        {
            if (Input.GetKeyDown(acceptKey))
            {
                AcceptActionCategorySelection();
            } 
            else
            {
                dialogBox.UpdateActionCategorySelection(selectionPosition);
            }
        }
        else if (state == BattleState.PlayerCoreActionSelect)
        {
            if (Input.GetKeyDown(acceptKey))
            {
                AcceptCoreActionSelection();
            }
            else if (Input.GetKeyDown(backKey))
            {
                selectionPosition = 0;
                PlayerActionCategorySelect();
            }
            else
            {
                UpdateCoreActionSelection();
            }
        }
        else if (state == BattleState.PlayerEmpoweredActionSelect)
        {
            if (Input.GetKeyDown(acceptKey))
            {
                AcceptEmpoweredActionSelection();
            }
            else if (Input.GetKeyDown(backKey))
            {
                selectionPosition = 1;
                PlayerActionCategorySelect();
            }
            else
            {
                UpdateEmpoweredActionSelection();
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
            selectionPosition = 0;
            PlayerActionCategorySelect();
        }
        else if (selectionPosition < dialogBox.ActionText.Count && dialogBox.ActionText[selectionPosition].text != "-")
        {
            Debug.Log($"{activeCreature.CreatureInstance.Nickname} used {dialogBox.ActionText[selectionPosition].text}");
        }
    }

    void AcceptEmpoweredActionSelection()
    {
        if (selectionPosition == 3)
        {
            selectionPosition = 1;
            PlayerActionCategorySelect();
        }
        else if (selectionPosition < dialogBox.ActionText.Count && dialogBox.ActionText[selectionPosition].text != "-")
        {
            Debug.Log($"{activeCreature.CreatureInstance.Nickname} used {dialogBox.ActionText[selectionPosition].text}");
        }
    }

    void UpdateCoreActionSelection()
    {
        ActionBase selectedAction = null;
        if (selectionPosition == 0)
        {
            selectedAction = activeCreature.CreatureInstance.PhysicalCore?.Action;
        }
        else if (selectionPosition == 1)
        {
            selectedAction = activeCreature.CreatureInstance.MagicalCore?.Action;
        }
        else if (selectionPosition == 2)
        {
            selectedAction = activeCreature.CreatureInstance.DefensiveCore?.Action;
        }
        dialogBox.UpdateActionSelection(selectionPosition, selectedAction);
    }

    void UpdateEmpoweredActionSelection()
    {
        ActionBase selectedAction = null;
        if (selectionPosition < activeCreature.CreatureInstance.EquippedEmpoweredActions.Count)
        {
            selectedAction = activeCreature.CreatureInstance.EquippedEmpoweredActions[selectionPosition]?.Action;
        }
        dialogBox.UpdateActionSelection(selectionPosition, selectedAction);
    }
}
