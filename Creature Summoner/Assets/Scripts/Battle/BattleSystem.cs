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

    public List<BattleCreature> FieldCreatures { get; set; }

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

        FieldCreatures = new List<BattleCreature>();

        playerFrontMid.Setup();
        FieldCreatures.Add(playerFrontMid);
        enemyFrontMid.Setup();
        FieldCreatures.Add(enemyFrontMid);

        StateChoices = new Dictionary<BattleState, int>
        {
            { BattleState.Start, 0 },
            { BattleState.PlayerActionCategorySelect, dialogBox.ActionCategoryText.Count },
            { BattleState.PlayerCoreActionSelect, dialogBox.ActionText.Count },
            { BattleState.PlayerEmpoweredActionSelect, dialogBox.ActionText.Count },
            { BattleState.PlayerMasteryActionSelect, 2 },
            { BattleState.PlayerMoveSelect, 2 },
            { BattleState.PlayerExamine, FieldCreatures.Count },
            { BattleState.EnemyAction, 0 },
            { BattleState.Busy, 0 }
        };

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
            dialogBox.SetActionNames(activeCreature.CreatureInstance.EquippedCoreActions);
        }
        else if (battleState == BattleState.PlayerEmpoweredActionSelect)
        {
            dialogBox.SetActionNames(activeCreature.CreatureInstance.EquippedEmpoweredActions);
        }
        else if (battleState == BattleState.PlayerMasteryActionSelect)
        {
            dialogBox.SetActionNames(activeCreature.CreatureInstance.EquippedMasteryActions);
        }
        selectionPosition = 0;
    }
    /*
    IEnumerator PerformPlayerAction()
    {

    }
    */
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
            HandlePlayerActionCategorySelect();
        }
        else if (state == BattleState.PlayerCoreActionSelect)
        {
            HandlePlayerCoreActionSelect();
        }
        else if (state == BattleState.PlayerEmpoweredActionSelect)
        {
            HandlePlayerEmpoweredActionSelect();
        }
        else if (state == BattleState.PlayerMasteryActionSelect)
        {
            HandlePlayerMasteryActionSelect();
        }
        else if (state == BattleState.PlayerExamine)
        {
            HandlePlayerExamine();
        }
    }

    void HandlePlayerActionCategorySelect()
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

    void HandlePlayerCoreActionSelect()
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
            UpdateActionSelection(activeCreature.CreatureInstance.EquippedCoreActions);
        }
    }

    void HandlePlayerEmpoweredActionSelect()
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
            UpdateActionSelection(activeCreature.CreatureInstance.EquippedEmpoweredActions);
        }
    }

    void HandlePlayerMasteryActionSelect()
    {
        if (Input.GetKeyDown(acceptKey))
        {
            AcceptMasteryActionSelection();
        }
        else if (Input.GetKeyDown(backKey))
        {
            selectionPosition = 2;
            PlayerActionCategorySelect();
        }
        else
        {
            //UpdateMasteryActionSelection();
        }
    }

    void HandlePlayerExamine()
    {
        if (Input.GetKeyDown(backKey))
        {
            FieldCreatures[selectionPosition].Hud.EnableCreatureInfoPanel(false);
            selectionPosition = 4;
            PlayerActionCategorySelect();
        }
        else
        {
            for (int i = 0; i < FieldCreatures.Count; i++)
            {
                if (i == selectionPosition)
                {
                    FieldCreatures[i].Hud.EnableCreatureInfoPanel(true);
                }
                else
                {
                    FieldCreatures[i].Hud.EnableCreatureInfoPanel(false);
                }
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
        else if (selectionPosition == 4)
        {
            PlayerActionSelect(BattleState.PlayerExamine);
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
            dialogBox.EnableActionSelect(false);
            dialogBox.EnableDialogText(true);
            //StartCoroutine(PerformPlayerAction());
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
            Debug.Log($"{activeCreature.CreatureInstance.Nickname} used {dialogBox.ActionText[selectionPosition].text}");//TEMP
        }
    }

    void AcceptMasteryActionSelection()
    {
        if (selectionPosition == 3)
        {
            selectionPosition = 2;
            PlayerActionCategorySelect();
        }
        else if (selectionPosition < dialogBox.ActionText.Count && dialogBox.ActionText[selectionPosition].text != "-")
        {
            Debug.Log($"{activeCreature.CreatureInstance.Nickname} used {dialogBox.ActionText[selectionPosition].text}");//TEMP
        }
    }

    void UpdateActionSelection(IAction[] actions)
    {
        ActionBase selectedAction = null;
        if (selectionPosition >= 0 && selectionPosition < actions.Length)
        {
            selectedAction = actions[selectionPosition]?.BaseAction;
        }
        dialogBox.UpdateActionSelection(selectionPosition, selectedAction);
    }
}
