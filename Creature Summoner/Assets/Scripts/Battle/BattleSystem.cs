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
    TargetSelect,
    EnemyAction,
    Busy
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleCreature playerFrontMid;
    [SerializeField] BattleCreature enemyFrontMid;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] GameObject actionCategories;

    const float TEXT_DELAY = 0.8f;
    const KeyCode acceptKey = KeyCode.Z;
    const KeyCode backKey = KeyCode.X;

    public Dictionary<BattleState, int> StateChoices { get; set; }

    public List<BattleCreature> FieldCreatures { get; set; }

    public BattleCreature activeCreature;

    public List<BattleCreature> CreatureTargets { get; set; }
    public IAction selectedAction;
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
        dialogBox.EnableActionDetails(false);

        FieldCreatures = new List<BattleCreature>();
        CreatureTargets = new List<BattleCreature>();

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
            { BattleState.PlayerExamine, FieldCreatures.Count + 1},
            { BattleState.TargetSelect, FieldCreatures.Count + 1},
            { BattleState.EnemyAction, 0 },
            { BattleState.Busy, 0 }
        };

        activeCreature = playerFrontMid;

        yield return dialogBox.TypeDialog($"You have been attacked by a {enemyFrontMid.CreatureInstance.Species.CreatureName}!");
        yield return new WaitForSeconds(TEXT_DELAY);

        ToPlayerActionCategorySelectState();
    }

    void ToPlayerActionCategorySelectState()
    {
        state = BattleState.PlayerActionCategorySelect;
        StartCoroutine(dialogBox.TypeDialog("Choose which kind of action to take"));
        dialogBox.EnableActionCategorySelect(true);
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionSelect(false);
        dialogBox.EnableActionDetails(false);
    }

    void ToPlayerActionSelectState(BattleState battleState)
    {
        state = battleState;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableActionDetails(true);
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
            dialogBox.DisableActionOptions(1, 2);
        }
        selectionPosition = 0;
    }

    void ToPlayerExamineState()
    {
        state = BattleState.PlayerExamine;
        // Update current number of active Creatures
        StateChoices[BattleState.PlayerExamine] = FieldCreatures.Count + 1;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableDialogText(true);
        dialogBox.DisableActionOptions(0, 3);
        dialogBox.ResetActionSelection();
        selectionPosition = 0;
    }

    void ToTargetSelectState()
    {
        state = BattleState.TargetSelect;
        StateChoices[BattleState.TargetSelect] = FieldCreatures.Count + 1;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableDialogText(true);
        dialogBox.DisableActionOptions(0, 3);
        dialogBox.ResetActionSelection();
        selectionPosition = 0;
    }

    void ToBusyState()
    {
        state = BattleState.Busy;
    }

    IEnumerator PerformPlayerAction(IAction action)
    {
        yield return dialogBox.TypeDialog($"{activeCreature.CreatureInstance.Nickname} used {action.BaseAction.TalentName}.");
        yield return new WaitForSeconds(TEXT_DELAY);
        //yield ToPlayerActionCategorySelectState();
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
        if (Input.GetKeyDown(backKey) || (Input.GetKeyDown(acceptKey) && selectionPosition == StateChoices[state] - 1))
        {
            selectionPosition = 0;
            ToPlayerActionCategorySelectState();
        }
        else if (Input.GetKeyDown(acceptKey))
        {
            AcceptCoreActionSelection();
        }
        else
        {
            UpdateActionSelection(activeCreature.CreatureInstance.EquippedCoreActions);
        }
    }

    void HandlePlayerEmpoweredActionSelect()
    {
        if (Input.GetKeyDown(backKey) || (Input.GetKeyDown(acceptKey) && selectionPosition == StateChoices[state] - 1))
        {
            selectionPosition = 1;
            ToPlayerActionCategorySelectState();
        }
        else if (Input.GetKeyDown(acceptKey))
        {
            AcceptEmpoweredActionSelection();
        }
        else
        {
            UpdateActionSelection(activeCreature.CreatureInstance.EquippedEmpoweredActions);
        }
    }

    void HandlePlayerMasteryActionSelect()
    {
        if (Input.GetKeyDown(backKey) || (Input.GetKeyDown(acceptKey) && selectionPosition == StateChoices[state] - 1))
        {
            selectionPosition = 2;
            dialogBox.EnableActionOptions();
            ToPlayerActionCategorySelectState();
        }
        else if (Input.GetKeyDown(acceptKey))
        {
            AcceptMasteryActionSelection();
        }
        else
        {
            UpdateActionSelection(activeCreature.CreatureInstance.EquippedMasteryActions);
            // Highlight 'Back' option if last option is selected
            if (selectionPosition == StateChoices[state] - 1)
            {
                dialogBox.HighlightBackOption();
            }
        }
    }

    void HandlePlayerExamine()
    {
        if (Input.GetKeyDown(backKey) || (Input.GetKeyDown(acceptKey) && selectionPosition == StateChoices[state] - 1))
        {
            // Disable the currently open HUD panel before going back
            if (selectionPosition < StateChoices[state] - 1)
            {
                ResetTargets();
                FieldCreatures[selectionPosition].Hud.EnableCreatureInfoPanel(false); 
            }
            selectionPosition = 4;
            dialogBox.EnableActionOptions();
            ToPlayerActionCategorySelectState();
        }
        else
        {
            ResetTargets();

            if (FieldCreatures.Count > selectionPosition)
            {
                FieldCreatures[selectionPosition].Hud.EnableCreatureInfoPanel(true);
                FieldCreatures[selectionPosition].Hud.EnableSelectionArrow(true);
                CreatureTargets.Add(FieldCreatures[selectionPosition]);
            }
            // Makes the action menu highlight the Back option when it reaches the end
            if (selectionPosition == StateChoices[state] - 1)
            {
                dialogBox.HighlightBackOption();
            } 
            else
            {
                dialogBox.ResetActionSelection();
            }
        }
    }

    void AcceptActionCategorySelection()
    {
        if (selectionPosition == 0)
        {
            ToPlayerActionSelectState(BattleState.PlayerCoreActionSelect);
        }
        else if (selectionPosition == 1)
        {
            ToPlayerActionSelectState(BattleState.PlayerEmpoweredActionSelect);
        }
        else if (selectionPosition == 2)
        {
            ToPlayerActionSelectState(BattleState.PlayerMasteryActionSelect);
        }
        else if (selectionPosition == 3)
        {
            //PlayerActionSelect(BattleState.PlayerMoveSelect);
        }
        else if (selectionPosition == 4)
        {
            ToPlayerExamineState();
        }
    }

    void AcceptCoreActionSelection()
    {
        if (selectionPosition < dialogBox.ActionText.Count && dialogBox.ActionText[selectionPosition].text != "-")
        {
            if (activeCreature.CreatureInstance.EquippedCoreActions[selectionPosition] != null)
            {
                dialogBox.EnableActionSelect(false);
                dialogBox.EnableActionDetails(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(PerformPlayerAction(activeCreature.CreatureInstance.EquippedCoreActions[selectionPosition]));
            } 
            else
            {
                Debug.Log("error: selected Core move does not match active creature's available moves.");
            }
        }
    }

    void AcceptEmpoweredActionSelection()
    {
        if (selectionPosition < dialogBox.ActionText.Count && dialogBox.ActionText[selectionPosition].text != "-")
        {
            Debug.Log($"{activeCreature.CreatureInstance.Nickname} used {dialogBox.ActionText[selectionPosition].text}");//TEMP
        }
    }

    void AcceptMasteryActionSelection()
    {
        if (selectionPosition < dialogBox.ActionText.Count && dialogBox.ActionText[selectionPosition].text != "-")
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

    void ResetTargets()
    {
        while (CreatureTargets.Count > 0)
        {
            CreatureTargets[0].Hud.EnableCreatureInfoPanel(false);
            CreatureTargets[0].Hud.EnableSelectionArrow(false);
            CreatureTargets.RemoveAt(0);
        }
    }
}

