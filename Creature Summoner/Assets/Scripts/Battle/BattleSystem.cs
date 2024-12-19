using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameConstants;

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
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] GameObject actionCategories;
    [SerializeField] BattleField Field;

    public event Action<bool> OnBattleOver;

    CreatureTeam playerTeam;
    CreatureTeam enemyTeam;

    //
    bool DEBUG = false;
    //

    const float ATTACK_DELAY = 1f;
    const KeyCode ACCEPT_KEY = KeyCode.Z; // Keyboard key which accepts options
    const KeyCode BACK_KEY = KeyCode.X; // Keyboard key which indicates cancelling or going back

    public Dictionary<BattleState, (int rows, int cols)> StateChoices { get; set; }

    public BattleSlot activeCreature;

    private BattleSlot highlightedCreature;

    public List<BattleSlot> CreatureTargets { get; set; }
    public List<BattleSlot> InitiativeOrder { get; set; }

    private ActionBase selectedAction;

    Stack<BattleState> stateStack = new Stack<BattleState>();
    Stack<(int y, int x)> positionStack = new Stack<(int y, int x)>();

    BattleState state;
    int selectionPositionX;
    int selectionPositionY;
    int combatRound;
    bool alliedFieldSelected; // Determines if the allied side of the field is selected

    private void Awake()
    {
        CreatureTargets = new List<BattleSlot>();
        InitiativeOrder = new List<BattleSlot>();

        StateChoices = new Dictionary<BattleState, (int rows, int cols)>
        {
            { BattleState.Start, (0, 0) },
            { BattleState.NewRound, (0, 0) },
            { BattleState.DetermineTurn, (0, 0) },
            { BattleState.PlayerActionCategorySelect, (Mathf.CeilToInt((float)dialogBox.ActionCategoryText.Count/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerCoreActionSelect, (Mathf.CeilToInt((float)CORE_SLOTS/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerEmpoweredActionSelect, (Mathf.CeilToInt((float)EMPOWERED_SLOTS/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerMasteryActionSelect, (Mathf.CeilToInt((float)MASTERY_SLOTS/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerMovementSelect, (2, BATTLE_COLS) },
            { BattleState.PlayerExamine, (BATTLE_ROWS + 1, BATTLE_COLS * 2) },
            { BattleState.TargetSelect, (BATTLE_ROWS + 1, BATTLE_COLS * 2) },
            { BattleState.EnemyAction, (0, 0) },
            { BattleState.Busy, (0, 0) }
        };
    }

    public void StartBattle(CreatureTeam playerTeam, CreatureTeam enemyTeam)
    {
        this.playerTeam = playerTeam;
        this.enemyTeam = enemyTeam;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionCategorySelect(true);
        dialogBox.EnableActionSelect(false);
        dialogBox.EnableActionDetails(false);

        CreatureTargets.Clear();
        InitiativeOrder.Clear();

        AddCreatures(playerTeam, true);
        AddCreatures(enemyTeam, false);

        int enemy_num = Field.EnemyCount();
        combatRound = 0;
        alliedFieldSelected = true;
        
        yield return dialogBox.StartTypingDialog($"You have been attacked by {enemy_num} creatures!");

        StartRound();
    }

    private void AddCreatures(CreatureTeam creatureTeam, bool isPlayerTeam)
    {
        List<Creature> creatureArray = creatureTeam.Creatures;

        for (int i = 0; i < creatureArray.Count; i++)
        {
            if (creatureArray[i] != null)
            {
                int row = i % BATTLE_ROWS; // Calculate row
                int col = i / BATTLE_ROWS; // Calculate column

                Field.AddCreature(creatureArray[i], row, col, isPlayerTeam);
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
            if (creature != null && !creature.IsEmpty)
            creature.RollInitiative();
            InitiativeOrder.Add(creature);
        }
        InitiativeOrder = InitiativeOrder
        .OrderByDescending(c => c.Initiative)   // Primary: Initiative
        .ThenByDescending(c => c.Creature.Speed)  // Secondary: Speed Stat
        .ThenByDescending(c => c.Creature.Species.Speed) // Tertiary: Species Base Speed
        .ToList();

        //// DEBUG
        if (DEBUG)
        {
            foreach (var creature in InitiativeOrder)
            {
                Debug.Log($"{creature.Creature.Nickname} rolled {creature.Initiative}");
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
                    activeCreature.ToggleHighlightAura(false);
                }
                // Set next creature's turn
                activeCreature = InitiativeOrder[0];
                activeCreature.ToggleHighlightAura(true);
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
            dialogBox.SetActionNames(activeCreature.Creature.EquippedCoreActions);
        }
        else if (battleState == BattleState.PlayerEmpoweredActionSelect)
        {
            dialogBox.SetActionNames(activeCreature.Creature.EquippedEmpoweredActions);
        }
        else if (battleState == BattleState.PlayerMasteryActionSelect)
        {
            dialogBox.SetActionNames(activeCreature.Creature.EquippedMasteryActions);
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

            string targetMessage = $"{activeCreature.Creature.Nickname} used {action.TalentName}.";

            yield return dialogBox.StartTypingDialog(targetMessage);

            //yield return new WaitForSeconds(ATTACK_DELAY); // Not needed anymore?

            activeCreature.PlayAttackAnimation();

            foreach (var target in CreatureTargets)
            {
                List<string> actionMessages = selectedAction.UseAction(activeCreature, target);
                battleMessages.AddRange(actionMessages);

                // Process the battle messages
                foreach (var message in battleMessages)
                {
                    yield return dialogBox.StartTypingDialog(message);  // Wait for typing to finish
                }

                // After typing is done, ensure both HUD updates have finished
                while (activeCreature.IsUpdating() || target.IsUpdating())
                {
                    yield return null; // Wait until all updates are finished
                }
                if (target.IsDefeated)
                {
                    target.ClearSlot();
                }

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
            if (Field.EnemyCount() == 0)
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
            UpdateActionSelection(activeCreature.Creature.EquippedCoreActions);
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
            UpdateActionSelection(activeCreature.Creature.EquippedEmpoweredActions);
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
            UpdateActionSelection(activeCreature.Creature.EquippedMasteryActions);
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
            BattleSlot selectedCreature = CreatureByPosition();
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
            BattleSlot selectedCreature = CreatureByPosition();
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
            ActionBase selected = activeCreature.Creature.EquippedCoreActions[selection].Action;
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

        StartCoroutine(PerformAction(selectedAction));
    }

    void SelectEmpoweredAction()
    {
        int selection = LinearSelectionPosition();
        if (selection < dialogBox.ActionText.Count && dialogBox.ActionText[selection].text != "-")
        {
            ActionBase selected = activeCreature.Creature.EquippedEmpoweredActions[selection].Action;
            if (selected != null)
            {
                if (selected.EnergyCost <= activeCreature.Creature.Energy) 
                {
                    selectedAction = selected;
                    ToTargetSelectState();
                }
                else
                {
                    StartCoroutine(DialogMessage($"{activeCreature.Creature.Nickname} does not have enough energy"));
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
            Debug.Log($"{activeCreature.Creature.Nickname} used {dialogBox.ActionText[selection].text}");//TEMP
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

    private BattleSlot CreatureByPosition()
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
            CreatureTargets[0].ToggleStatusWindow(false);
            CreatureTargets[0].UpdateSelectionArrow(SelectionArrowState.None);
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

    void HighlightCreature(BattleSlot creature, bool showHUD = false)
    {
        creature.UpdateSelectionArrow(SelectionArrowState.Valid);
        if (showHUD)
        {
            creature.ToggleStatusWindow(true);
        }
        highlightedCreature = creature;
    }

    void UnhighlightCreature()
    {
        if (highlightedCreature != null)
        {
            highlightedCreature.UpdateSelectionArrow(SelectionArrowState.None);
            highlightedCreature.ToggleStatusWindow(false);
            highlightedCreature = null;
        }
    }

    void AddTarget(BattleSlot target)
    {
        target.UpdateSelectionArrow(SelectionArrowState.Selected);
        CreatureTargets.Add(target);
    }
}

