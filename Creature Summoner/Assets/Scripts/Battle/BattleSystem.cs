using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState
{
    Start,
    NewRound,
    DetermineTurn,
    PlayerActionCategorySelect,
    PlayerCoreActionSelect,
    PlayerEmpoweredActionSelect,
    PlayerMasteryActionSelect,
    PlayerMovementSelect,
    PlayerExamine,
    TargetSelect,
    EnemyAction,
    Busy
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleCreature[] friends;
    [SerializeField] BattleCreature[] enemies;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] GameObject actionCategories;

    public event Action<bool> OnBattleOver;


    //
    bool DEBUG = false;
    //

    const int BATTLE_ROWS = 3; // Number of Rows on each side of the battlefield
    const int BATTLE_COLS = 2; // Number of Columns on each side of the battlefield

    const int BATTLE_MENU_COLS = 2; // Number of Columns in the battle selection menu
    const int CORE_ACTION_COUNT = 3;
    const int EMPOWERED_ACTION_COUNT = 3;
    const int MASTERY_ACTION_COUNT = 1;

    const float ATTACK_DELAY = 1f;
    const KeyCode ACCEPT_KEY = KeyCode.Z; // Keyboard key which accepts options
    const KeyCode BACK_KEY = KeyCode.X; // Keyboard key which indicates cancelling or going back

    public Dictionary<BattleState, (int rows, int cols)> StateChoices { get; set; }

    public BattleField Field { get; set; }

    public BattleCreature activeCreature;

    private BattleCreature highlightedCreature;

    public List<BattleCreature> CreatureTargets { get; set; }
    public List<BattleCreature> InitiativeOrder { get; set; }

    private ActionBase selectedAction;

    Stack<BattleState> stateStack = new Stack<BattleState>();
    Stack<(int y, int x)> positionStack = new Stack<(int y, int x)>();

    BattleState state;
    int selectionPositionX;
    int selectionPositionY;
    int combatRound;
    bool alliedFieldSelected; // Determines if the allied side of the field is selected

    public void StartBattle()
    {
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionCategorySelect(true);
        dialogBox.EnableActionSelect(false);
        dialogBox.EnableActionDetails(false);

        CreatureTargets = new List<BattleCreature>();
        InitiativeOrder = new List<BattleCreature>();

        Field = new BattleField(BATTLE_ROWS, BATTLE_COLS);

        AddCreatures(friends);
        AddCreatures(enemies);

        StateChoices = new Dictionary<BattleState, (int rows, int cols)>
        {
            { BattleState.Start, (0, 0) },
            { BattleState.NewRound, (0, 0) },
            { BattleState.DetermineTurn, (0, 0) },
            { BattleState.PlayerActionCategorySelect, (Mathf.CeilToInt((float)dialogBox.ActionCategoryText.Count/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerCoreActionSelect, (Mathf.CeilToInt((float)CORE_ACTION_COUNT/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerEmpoweredActionSelect, (Mathf.CeilToInt((float)EMPOWERED_ACTION_COUNT/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerMasteryActionSelect, (Mathf.CeilToInt((float)MASTERY_ACTION_COUNT/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerMovementSelect, (2, BATTLE_COLS) },
            { BattleState.PlayerExamine, (BATTLE_ROWS + 1, BATTLE_COLS * 2) },
            { BattleState.TargetSelect, (BATTLE_ROWS + 1, BATTLE_COLS * 2) },
            { BattleState.EnemyAction, (0, 0) },
            { BattleState.Busy, (0, 0) }
        };

        int enemy_num = Field.GetTargets(false, false).Count;
        combatRound = 0;
        alliedFieldSelected = true;
        
        yield return dialogBox.StartTypingDialog($"You have been attacked by {enemy_num} creatures!");

        StartRound();
    }

    private void AddCreatures(BattleCreature[] creatureArray)
    {
        for (int i = 0; i < creatureArray.Length; i++)
        {
            if (creatureArray[i] != null &&
                !creatureArray[i].Ignore &&
                !creatureArray[i].Empty)
            {
                int row = i % BATTLE_ROWS; // Calculate row
                int col = i / BATTLE_ROWS; // Calculate column
                Field.AddCreature(creatureArray[i], row, col);
            }
        }
    }

    void StartRound()
    {
        combatRound++;
        SetupInitiative();
        ToDetermineTurn();
    }

    void SetupInitiative()
    {
        foreach(var creature in Field.FieldCreatures)
        {
            if (creature != null && !creature.Empty)
            creature.RollInitiative();
            InitiativeOrder.Add(creature);
        }
        InitiativeOrder = InitiativeOrder
        .OrderByDescending(c => c.Initiative)   // Primary: Initiative
        .ThenByDescending(c => c.CreatureInstance.Speed)  // Secondary: Speed Stat
        .ThenByDescending(c => c.CreatureInstance.Species.Speed) // Tertiary: Species Base Speed
        .ToList();

        //// DEBUG
        if (DEBUG)
        {
            foreach (var creature in InitiativeOrder)
            {
                Debug.Log($"{creature.CreatureInstance.Nickname} rolled {creature.Initiative}");
            }
        }
        //// DEBUG
    }

    void GetActiveCreature()
    {
        while (InitiativeOrder.Count > 0)
        {
            if (InitiativeOrder[0].IsDefeated)
            {
                // Remove defeated creatures from the Initiative tracker
                InitiativeOrder.RemoveAt(0);
            }
            else
            {
                // Deselect previous creature, if any
                if (activeCreature != null)
                {
                    activeCreature.Select(false);
                }
                // Set next creature's turn
                activeCreature = InitiativeOrder[0];
                activeCreature.Select(true);
                InitiativeOrder.RemoveAt(0);
                ToPlayerActionCategorySelectState();
                return;
            }
        }
        // When Initiative tracker is empty, start a new round
        ToNewRound();
    }

    void ToNewRound()
    {
        state = BattleState.NewRound;
    }

    void ToDetermineTurn()
    {
        state = BattleState.DetermineTurn;
        ResetSelectionPositions();
    }

    void ResetSelectionPositions(int y = 0, int x = 0)
    {
        selectionPositionY = y;
        selectionPositionX = x;
    }

    void ResetSelectedAction()
    {
        selectedAction = null;
    }

    void ToPlayerActionCategorySelectState()
    {
        state = BattleState.PlayerActionCategorySelect;
        dialogBox.StartTypingDialog("Choose which kind of action to take");
        dialogBox.EnableActionCategorySelect(true);
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionSelect(false);
        dialogBox.EnableActionDetails(false);
    }

    void ToPlayerActionSelectState(BattleState battleState)
    {
        PushState();
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
        ResetSelectionPositions();
    }

    void ToPlayerExamineState()
    {
        PushState();
        state = BattleState.PlayerExamine;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableDialogText(true);
        dialogBox.DisableActionOptions(0, 3);
        dialogBox.ResetActionSelection();
        ResetSelectionPositions();
    }

    void ToTargetSelectState()
    {
        PushState();
        state = BattleState.TargetSelect;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableDialogText(false);
        dialogBox.DisableActionOptions(0, 3);
        dialogBox.ResetActionSelection();
        ResetSelectionPositions();
    }

    void ToBusyState()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelect(false);
        dialogBox.EnableActionDetails(false);
        dialogBox.EnableDialogText(true);
    }

    IEnumerator PerformAction(ActionBase action)
    {
        try
        {
            // Check for null values and ensure there's at least one target
            if (selectedAction == null)
            {
                Debug.LogError("Error: selectedAction is null.");
                yield break; // Exit the coroutine if selectedAction is null
            }

            if (activeCreature == null)
            {
                Debug.LogError("Error: activeCreature is null.");
                yield break; // Exit the coroutine if activeCreature is null
            }

            if (CreatureTargets == null || CreatureTargets.Count == 0)
            {
                Debug.LogError("Error: CreatureTargets is empty or null.");
                yield break; // Exit the coroutine if there are no targets
            }

            ToBusyState();

            List<string> battleMessages = new List<string>();

            string targetMessage = $"{activeCreature.CreatureInstance.Nickname} used {action.TalentName}.";

            yield return dialogBox.StartTypingDialog(targetMessage);

            //yield return new WaitForSeconds(ATTACK_DELAY); // Not needed anymore?

            activeCreature.PlayAttackAnimation();

            foreach (var target in CreatureTargets)
            {
                List<string> actionMessages = selectedAction.UseAction(activeCreature, target);
                battleMessages.AddRange(actionMessages);

                // Start both HUD updates simultaneously and yield for both
                StartCoroutine(target.UpdateHud());
                StartCoroutine(activeCreature.UpdateHud());

                // Process the battle messages
                foreach (var message in battleMessages)
                {
                    yield return dialogBox.StartTypingDialog(message);  // Wait for typing to finish
                }

                // After typing is done, ensure both HUD updates have finished
                yield return new WaitUntil(() => !target.Hud.IsUpdating && !activeCreature.Hud.IsUpdating);

                battleMessages.Clear();
            }

        }
        finally
        {
            foreach (var target in CreatureTargets)
            {
                if (target.IsDefeated)
                {
                    Field.RemoveCreature(target);
                }
            }
            ClearStacks();
            ResetTargets();
            ResetSelectedAction();
            if (Field.IsNoEnemies())
            {
                OnBattleOver(true);
            }
            ToDetermineTurn();
        }
    }

    IEnumerator DialogMessage(string message)
    {
        ToBusyState();
        yield return dialogBox.StartTypingDialog(message);
        ToPlayerActionCategorySelectState();
    }

    public void HandleUpdate()
    {
        if (state == BattleState.NewRound)
        {
            StartRound();
        }
        else if (state == BattleState.DetermineTurn) 
        {
            GetActiveCreature();
        }
        else if (state == BattleState.PlayerActionCategorySelect
            || state == BattleState.PlayerCoreActionSelect
            || state == BattleState.PlayerEmpoweredActionSelect
            || state == BattleState.PlayerMasteryActionSelect
            || state == BattleState.PlayerExamine
            || state == BattleState.TargetSelect)
        {
            HandleMenuSelection();
            HandleStateBasedInput();
        }
    }

    void HandleMenuSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (StateChoices[state].rows != 0)
            {
                selectionPositionY = (selectionPositionY + 1) % StateChoices[state].rows;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (StateChoices[state].rows != 0)
            {
                selectionPositionY = (selectionPositionY - 1 + StateChoices[state].rows) % StateChoices[state].rows;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (StateChoices[state].cols != 0)
            {
                selectionPositionX = (selectionPositionX - 1 + StateChoices[state].cols) % StateChoices[state].cols;
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (StateChoices[state].cols != 0)
            {
                selectionPositionX = (selectionPositionX + 1) % StateChoices[state].cols;
            }
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
        else if (state == BattleState.TargetSelect)
        {
            HandleTargetSelect();
        }
    }

    void HandlePlayerActionCategorySelect()
    {
        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            AcceptActionCategorySelection();
        }
        else
        {
            dialogBox.UpdateActionCategorySelection(LinearSelectionPosition());
        }
    }

    void HandlePlayerCoreActionSelect()
    {
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) 
            && selectionPositionX == StateChoices[state].rows - 1
            && selectionPositionY == StateChoices[state].cols - 1))
        {
            GoBackToState();
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
        {
            SelectCoreAction();
        }
        else
        {
            UpdateActionSelection(activeCreature.CreatureInstance.EquippedCoreActions);
        }
    }

    void HandlePlayerEmpoweredActionSelect()
    {
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY)
            && selectionPositionX == StateChoices[state].rows - 1
            && selectionPositionY == StateChoices[state].cols - 1))
        {
            GoBackToState();
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
        {
            SelectEmpoweredAction();
        }
        else
        {
            UpdateActionSelection(activeCreature.CreatureInstance.EquippedEmpoweredActions);
        }
    }

    void HandlePlayerMasteryActionSelect()
    {
        int selection = LinearSelectionPosition();
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY)
            && selectionPositionX == StateChoices[state].rows - 1
            && selectionPositionY == StateChoices[state].cols - 1))
        {
            dialogBox.EnableActionOptions();
            GoBackToState();
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
        {
            SelectMasteryAction();
        }
        else
        {
            UpdateActionSelection(activeCreature.CreatureInstance.EquippedMasteryActions);
            // Highlight 'Back' option if last option is selected
            if (selectionPositionX == StateChoices[state].rows - 1
            && selectionPositionY == StateChoices[state].cols - 1)
            {
                dialogBox.HighlightBackOption();
            }
        }
    }

    void HandlePlayerExamine()
    {
        UnhighlightCreature();

        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPositionY == StateChoices[state].cols - 1))
        {
            dialogBox.EnableActionOptions();
            GoBackToState();
        }
        else
        {
            BattleCreature selectedCreature = CreatureByPosition();
            if (selectedCreature != null)
            {
                // Show panel for the selected creature
                HighlightCreature(selectedCreature, true);
            }
            if (selectionPositionY == StateChoices[state].rows - 1)
            {
                // Highlight the Back button
                dialogBox.HighlightBackOption();
            } 
            else
            {
                // Reset highlighted Back button to black
                dialogBox.ResetActionSelection();
            }
        }
    }

    void HandleTargetSelect()
    {
        UnhighlightCreature();
     
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPositionY == StateChoices[state].cols - 1))
        {
            dialogBox.EnableActionOptions();
            ResetTargets();
            GoBackToState();
        }
        else
        {
            BattleCreature selectedCreature = CreatureByPosition();
            if (selectedCreature != null)
            {
                if (Input.GetKeyDown(ACCEPT_KEY))
                {
                    AddTarget(selectedCreature);
                    if (selectedAction == null)
                    {
                        Debug.Log("Error: No action selected");
                    }
                    else if (CreatureTargets.Count > selectedAction.NumTargets)
                    {
                        Debug.Log("Error: Too many targets for action");
                    }
                    else if (CreatureTargets.Count == selectedAction.NumTargets)
                    {
                        dialogBox.EnableActionOptions();
                        ConfirmAction();
                    }
                }
                else
                {
                    HighlightCreature(selectedCreature);
                }
            }

            if (selectionPositionY == StateChoices[state].rows - 1)
            {
                // Highlight the Back button
                dialogBox.HighlightBackOption();
            }
            else
            {
                // Reset highlighted Back button to black
                dialogBox.ResetActionSelection();
            }
        }
    }

    void AcceptActionCategorySelection()
    {
        int selected = LinearSelectionPosition();
        if (selected == 0)
        {
            ToPlayerActionSelectState(BattleState.PlayerCoreActionSelect);
        }
        else if (selected == 1)
        {
            ToPlayerActionSelectState(BattleState.PlayerEmpoweredActionSelect);
        }
        else if (selected == 2)
        {
            ToPlayerActionSelectState(BattleState.PlayerMasteryActionSelect);
        }
        else if (selected == 3)
        {
            //PlayerActionSelect(BattleState.PlayerMoveSelect);
        }
        else if (selected == 4)
        {
            ToPlayerExamineState();
        }
    }

    void SelectCoreAction()
    {
        int selection = LinearSelectionPosition();
        if (selection < dialogBox.ActionText.Count && dialogBox.ActionText[selection].text != "-")
        {
            ActionBase selected = activeCreature.CreatureInstance.EquippedCoreActions[selection].Action;
            if (selected != null)
            {
                selectedAction = selected;
                ToTargetSelectState();
            } 
            else
            {
                Debug.Log("error: selected Core move does not match active creature's available moves.");
            }
        }
    }

    void ConfirmAction()
    {
        if (selectedAction == null) 
        {
            Debug.Log("Error! No Action selected.");
            ToDetermineTurn();
            return;
        }
        if (selectedAction.Category == ActionCategory.Core)
        {
            activeCreature.AddEnergy(selectedAction.Energy);
        }
        else if (selectedAction.Category == ActionCategory.Empowered)
        {
            activeCreature.RemoveEnergy(selectedAction.Energy);
        }
        else
        {
            Debug.Log($"Error: Action category: {selectedAction.Category}");
        }

        StartCoroutine(PerformAction(selectedAction));
    }

    void SelectEmpoweredAction()
    {
        int selection = LinearSelectionPosition();
        if (selection < dialogBox.ActionText.Count && dialogBox.ActionText[selection].text != "-")
        {
            ActionBase selected = activeCreature.CreatureInstance.EquippedEmpoweredActions[selection].Action;
            if (selected != null)
            {
                if (selected.Energy <= activeCreature.CreatureInstance.Energy) 
                {
                    selectedAction = selected;
                    ToTargetSelectState();
                }
                else
                {
                    StartCoroutine(DialogMessage($"{activeCreature.CreatureInstance.Nickname} does not have enough energy"));
                }
            }
            else
            {
                Debug.Log("error: selected Core move does not match active creature's available moves.");
            }
        }
    }

    void SelectMasteryAction()
    {
        int selection = LinearSelectionPosition();
        if (selection < dialogBox.ActionText.Count && dialogBox.ActionText[selection].text != "-")
        {
            Debug.Log($"{activeCreature.CreatureInstance.Nickname} used {dialogBox.ActionText[selection].text}");//TEMP
        }
    }

    void UpdateActionSelection(CreatureAction[] actions)
    {
        int selection = LinearSelectionPosition();
        ActionBase highlightedAction = null;
        if (selection >= 0 && selection < actions.Length)
        {
            highlightedAction = actions[selection]?.Action;
        }
        dialogBox.UpdateActionSelection(selection, highlightedAction);
    }

    int LinearSelectionPosition()
    {
        return selectionPositionY * StateChoices[state].cols + selectionPositionX;
    }

    private BattleCreature CreatureByPosition()
    {
        int yIndex = selectionPositionY;
        int xIndex = selectionPositionX;
        if (selectionPositionX < BATTLE_COLS)
        {
            alliedFieldSelected = true;
            xIndex = (xIndex + 1) % BATTLE_COLS;
        }
        else
        {
            alliedFieldSelected = false;
            xIndex -= BATTLE_COLS;

        }
        return Field.GetCreature(alliedFieldSelected, yIndex, xIndex);
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

    void PushState()
    {
        stateStack.Push(state);
        positionStack.Push((selectionPositionY, selectionPositionX));
    }

    void GoBackToState()
    {
        if (stateStack.Count > 0)
        {
            BattleState newState = stateStack.Pop();
            if (positionStack.Count > 0)
            {
                (int y, int x) position = positionStack.Pop();
                selectionPositionY = position.y;
                selectionPositionX = position.x;
            }
            HandleStateTransition(newState);
        }
    }

    void HandleStateTransition(BattleState state)
    {
        switch (state)
        {
            case BattleState.PlayerActionCategorySelect:
                ToPlayerActionCategorySelectState();
                break;
            case BattleState.PlayerCoreActionSelect:
                ToPlayerActionSelectState(state);
                break;
            case BattleState.PlayerMovementSelect:
                //ADD LATER
                break;
            default:
                Debug.Log($"Error handling transition to state: {state}");///
                break;
        }
    }
    /*
    string CreateTargetsString()
    {
        List<string> targetNames = new List<string>();

        foreach (var target in CreatureTargets)
        {
            if (target == activeCreature)
            {
                targetNames.Add("itself");
            }
            else
            {
                targetNames.Add(target.CreatureInstance.Nickname);
            }
        }

        string targetsString;
        if (targetNames.Count == 1)
        {
            targetsString = targetNames[0];
        }
        else if (targetNames.Count == 2)
        {
            targetsString = string.Join(" and ", targetNames);
        }
        else
        {
            targetsString = string.Join(", ", targetNames.Take(targetNames.Count - 1)) + " and " + targetNames.Last();
        }

        return targetsString;
    }

    */

    void ClearStacks()
    {
        stateStack.Clear();
        positionStack.Clear();
    }

    void HighlightCreature(BattleCreature creature, bool showHUD = false)
    {
        creature.Hud.EnableSelectionArrow(true);
        if (showHUD)
        {
            creature.Hud.EnableCreatureInfoPanel(true);
        }
        highlightedCreature = creature;
    }

    void UnhighlightCreature()
    {
        if (highlightedCreature != null)
        {
            highlightedCreature.Hud.EnableSelectionArrow(false);
            highlightedCreature.Hud.EnableCreatureInfoPanel(false);
            highlightedCreature = null;
        }
    }

    void AddTarget(BattleCreature target)
    {
        target.Hud.EnableSelectionArrow(true, true);
        CreatureTargets.Add(target);
    }
}

