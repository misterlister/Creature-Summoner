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

    const int BATTLE_ROWS = 3; // Number of Rows on each side of the battlefield
    const int BATTLE_COLS = 2; // Number of Columns on each side of the battlefield

    const int BATTLE_MENU_COLS = 2; // Number of Columns in the battle selection menu
    const int CORE_ACTION_COUNT = 3;
    const int EMPOWERED_ACTION_COUNT = 3;
    const int MASTERY_ACTION_COUNT = 1;

    const float TEXT_DELAY = 0.8f; // Time delay after text is done printing
    const KeyCode ACCEPT_KEY = KeyCode.Z; // Keyboard key which accepts options
    const KeyCode BACK_KEY = KeyCode.X; // Keyboard key which indicates cancelling or going back

    public Dictionary<BattleState, (int rows, int cols)> StateChoices { get; set; }

    public BattleField Field { get; set; }

    public BattleCreature activeCreature;

    public List<BattleCreature> CreatureTargets { get; set; }
    public List<BattleCreature> InitiativeOrder { get; set; }

    Stack<BattleState> stateStack = new Stack<BattleState>();
    Stack<(int y, int x)> positionStack = new Stack<(int y, int x)>();

    BattleState state;
    int selectionPositionX;
    int selectionPositionY;
    int combatRound;
    bool alliedFieldSelected; // Determines if the allied side of the field is selected

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

        CreatureTargets = new List<BattleCreature>();
        InitiativeOrder = new List<BattleCreature>();

        Field = new BattleField(BATTLE_ROWS, BATTLE_COLS);

        // Add player creatures
        for (int i = 0; i < friends.Length; i++)
        {
            if (friends[i] != null && !friends[i].Ignore)
            {
                int row = i % BATTLE_ROWS; // Calculate row
                int col = i / BATTLE_ROWS; // Calculate column
                Field.AddCreature(friends[i], row, col);
            }
        }

        // Add enemy creatures
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && !enemies[i].Ignore)
            {
                int row = i % BATTLE_ROWS; // Calculate row
                int col = i / BATTLE_ROWS; // Calculate column
                Field.AddCreature(enemies[i], row, col);
            }
        }

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
        yield return new WaitForSeconds(TEXT_DELAY);

        StartRound();
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
            creature.RollInitiative();
            InitiativeOrder.Add(creature);
        }
        InitiativeOrder = InitiativeOrder
        .OrderByDescending(c => c.Initiative)   // Primary: Initiative
        .ThenByDescending(c => c.CreatureInstance.Speed)  // Secondary: Speed Stat
        .ThenByDescending(c => c.CreatureInstance.Species.Speed) // Tertiary: Species Base Speed
        .ToList();

        //// DEBUG
        foreach (var creature in InitiativeOrder)
        {
            Debug.Log($"{creature.CreatureInstance.Nickname} rolled {creature.Initiative}");
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
        resetSelections();
    }

    void resetSelections(int y = 0, int x = 0)
    {
        selectionPositionY = y;
        selectionPositionX = x;
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
        resetSelections();
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
        resetSelections();
    }

    void ToTargetSelectState()
    {
        PushState();
        state = BattleState.TargetSelect;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableDialogText(true);
        dialogBox.DisableActionOptions(0, 3);
        dialogBox.ResetActionSelection();
        resetSelections();
    }

    void ToBusyState()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelect(false);
        dialogBox.EnableActionDetails(false);
        dialogBox.EnableDialogText(true);
    }

    IEnumerator PerformPlayerAction(ActionBase action)
    {
        ToBusyState();
        yield return dialogBox.StartTypingDialog($"{activeCreature.CreatureInstance.Nickname} used {action.TalentName}.");
        yield return new WaitForSeconds(TEXT_DELAY);
        ClearStacks();
        ToDetermineTurn();
    }

    IEnumerator DialogMessage(string message)
    {
        ToBusyState();
        yield return dialogBox.StartTypingDialog(message);
        yield return new WaitForSeconds(TEXT_DELAY);
        ToPlayerActionCategorySelectState();
    }

    private void Update()
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
            //
        }
    }

    void HandlePlayerActionCategorySelect()
    {
        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            AcceptActionCategorySelection();
        }
        else
        {;
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
        ResetTargets();
        BattleCreature selectedCreature = CreatureByPosition();
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPositionY == StateChoices[state].cols - 1))
        {
            dialogBox.EnableActionOptions();
            GoBackToState();
        }
        else
        {
            if (selectedCreature != null)
            {
                // Show panel for the selected creature
                selectedCreature.Hud.EnableCreatureInfoPanel(true);
                selectedCreature.Hud.EnableSelectionArrow(true);
                CreatureTargets.Add(selectedCreature);
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
                activeCreature.AddEnergy(selected.Energy);
                StartCoroutine(PerformPlayerAction(selected));
            } 
            else
            {
                Debug.Log("error: selected Core move does not match active creature's available moves.");
            }
        }
    }

    void ConfirmCoreAction()
    {

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
                    activeCreature.RemoveEnergy(selected.Energy);
                    StartCoroutine(PerformPlayerAction(selected));
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

    void ClearStacks()
    {
        stateStack.Clear();
        positionStack.Clear();
    }
}

