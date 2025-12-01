using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static GameConstants;
using static UnityEngine.GraphicsBuffer;

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
    AOESelect,
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

    public BattleSlot activeCreature;
    private BattleSlot selectedCreature;
    private BattleSlot TargetedCreature;
    private ActionBase selectedAction;

    private Dictionary<BattleState, (int rows, int cols)> StateChoices;
    private List<BattleSlot> CreatureTargets;
    private List<BattleSlot> SelectableTargets;
    private List<Creature> InitiativeOrder;

    private bool movementChosen = false;
    private BattleSlot previousMovementSpace = null;

    private BattleEventManager eventManager;
    private BattleContext battleContext;

    Stack<BattleState> stateStack = new Stack<BattleState>();
    Stack<(int y, int x)> positionStack = new Stack<(int y, int x)>();

    BattleState state;
    int selectionPositionX;
    int selectionPositionY;
    int combatRound;

    private void Awake()
    {
        CreatureTargets = new List<BattleSlot>();
        TargetedCreature = null;
        InitiativeOrder = new List<Creature>();

        StateChoices = new Dictionary<BattleState, (int rows, int cols)>
        {
            { BattleState.Start, (0, 0) },
            { BattleState.NewRound, (0, 0) },
            { BattleState.DetermineTurn, (0, 0) },
            { BattleState.PlayerActionCategorySelect, (Mathf.CeilToInt((float)dialogBox.ActionCategoryText.Count/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerCoreActionSelect, (Mathf.CeilToInt((float)CORE_SLOTS/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerEmpoweredActionSelect, (Mathf.CeilToInt((float)EMPOWERED_SLOTS/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerMasteryActionSelect, (Mathf.CeilToInt((float)MASTERY_SLOTS/BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { BattleState.PlayerMovementSelect, (BATTLE_ROWS + 1, BATTLE_COLS/2) },
            { BattleState.PlayerExamine, (BATTLE_ROWS + 1, BATTLE_COLS) },
            { BattleState.TargetSelect, (BATTLE_ROWS + 1, BATTLE_COLS) },
            { BattleState.AOESelect, (0, 0) },
            { BattleState.EnemyAction, (0, 0) },
            { BattleState.Busy, (0, 0) }
        };
    }

    public void StartBattle(CreatureTeam playerTeam, CreatureTeam enemyTeam)
    {
        eventManager = new BattleEventManager();

        foreach (var creature in playerTeam.Creatures)
        {
            creature.InitializeBattle(eventManager);
        }

        foreach (var creature in enemyTeam.Creatures)
        {
            creature.InitializeBattle(eventManager);
        }

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
        
        yield return dialogBox.StartTypingDialog($"You have been attacked by {enemy_num} creatures!");

        StartRound();
    }

    private void AddCreatures(CreatureTeam creatureTeam, bool isPlayerTeam)
    {
        for (int i = 0; i < creatureTeam.Creatures.Count; i++)
        {
            if (creatureTeam.Creatures[i] != null)
            {
                int row = i % BATTLE_ROWS; // Calculate row
                int col;
                if (isPlayerTeam)
                {
                    col = (i < BATTLE_ROWS)? 1 : 0;
                }
                else
                {
                    col = (i < BATTLE_ROWS) ? ENEMY_COL : BATTLE_COLS -1;
                }

                Field.AddCreature(creatureTeam.Creatures[i], row, col, isPlayerTeam);
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
            if (creature != null && !creature.IsDefeated)
            creature.RollInitiative();
            InitiativeOrder.Add(creature);
        }
        InitiativeOrder = InitiativeOrder
        .OrderByDescending(c => c.Initiative)   // Primary: Initiative
        .ThenByDescending(c => c.Speed)  // Secondary: Speed Stat
        .ThenByDescending(c => c.Species.Speed) // Tertiary: Species Base Speed
        .ToList();

        //// DEBUG
        if (DEBUG)
        {
            foreach (var creature in InitiativeOrder)
            {
                Debug.Log($"{creature.Nickname} rolled {creature.Initiative}");
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
                    activeCreature.UpdateHighlightAura(HighlightAuraState.None);
                }
                // Set next creature's turn
                activeCreature = InitiativeOrder[0].BattleSlot;
                activeCreature.UpdateHighlightAura(HighlightAuraState.Active);
                InitiativeOrder.RemoveAt(0);
                ToPlayerActionCategorySelectState(true);
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

    void ResetSelectionPositions((int, int)? coords = null)
    {
        int row = 0;
        int col = 0;
        if (coords != null)
        {
            row = coords.Value.Item1;
            col = coords.Value.Item2;
        }
        selectionPositionY = row;
        selectionPositionX = col;
    }

    void ResetSelectedAction()
    {
        selectedAction = null;
    }

    void ToPlayerActionCategorySelectState(bool back_transition = false)
    {
        if (!back_transition)
        {
            PushState();
        }
        state = BattleState.PlayerActionCategorySelect;
        dialogBox.StartTypingDialog("Choose which kind of action to take");
        dialogBox.EnableActionCategorySelect(true);
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionSelect(false);
        dialogBox.EnableActionDetails(false);
        ResetSelectionPositions();
    }

    void ToPlayerActionSelectState(BattleState battleState, bool back_transition = false)
    {
        if (!back_transition)
        {
            PushState();
        }
        state = battleState;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableActionDetails(true);
        dialogBox.EnableActionOptions();
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

    void ToMoveSelectState(bool back_transition = false)
    {
        if (!back_transition)
        {
            PushState();
        }
        state = BattleState.PlayerMovementSelect;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableDialogText(true);
        dialogBox.DisableActionOptions(0, 3);
        dialogBox.ResetActionSelection();
        ResetSelectionPositions(GetTargetSelf(activeCreature));
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
        ResetSelectionPositions(GetTargetSelf(activeCreature));
    }

    void ToTargetSelectState(bool back_transition = false)
    {
        if (!back_transition)
        {
            PushState();
        }
        state = BattleState.TargetSelect;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableDialogText(false);
        dialogBox.DisableActionOptions(0, 3);
        dialogBox.ResetActionSelection();
        UpdateSelectableTargets(activeCreature, selectedAction);
        ResetSelectionPositions(GetStartingTarget());
    }

    void ToAOESelectState(bool back_transition = false)
    {
        if (!back_transition)
        {
            PushState();
        }
        state = BattleState.AOESelect;
        dialogBox.EnableActionCategorySelect(false);
        dialogBox.EnableActionSelect(true);
        dialogBox.EnableDialogText(false);
        dialogBox.DisableActionOptions(0, 3);
        dialogBox.ResetActionSelection();
        SetAOEStateChoices(selectedAction);
        Field.ResetTargetHighlights(activeCreature);
        HighlightAuraState highlightAura = (selectedAction.ActionClass == ActionClass.Attack) ? HighlightAuraState.Negative : HighlightAuraState.Positive;
        CreatureTargets = Field.GetAOETargets(TargetedCreature, selectedAction, selectionPositionY, isPlayer: activeCreature.IsPlayerSlot);
        HighlightUnitList(CreatureTargets, highlightAura);
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

            string targetMessage = $"{activeCreature.Creature.Nickname} used {action.ActionName}.";

            yield return dialogBox.StartTypingDialog(targetMessage);

            //yield return new WaitForSeconds(ATTACK_DELAY); // Not needed anymore?

            selectedAction.PayEnergyCost(activeCreature);

            if (selectedAction.Range != ActionRange.Self)
            {
                activeCreature.PlayAttackAnimation();
            }

            foreach (var target in CreatureTargets)
            {
                if (!target.IsEmpty)
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
            selectedAction.GenerateEnergy(activeCreature);
            EndTurn();
        }
    }

    IEnumerator DialogMessage(string message)
    {
        ToBusyState();
        yield return dialogBox.StartTypingDialog(message);
        ToPlayerActionCategorySelectState(true);
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
            || state == BattleState.PlayerMovementSelect
            || state == BattleState.PlayerExamine
            || state == BattleState.TargetSelect
            || state == BattleState.AOESelect)
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
        switch (state)
        {
            case BattleState.PlayerActionCategorySelect:
                HandlePlayerActionCategorySelect();
                break;
            case BattleState.PlayerCoreActionSelect:
                HandlePlayerCoreActionSelect();
                break;
            case BattleState.PlayerEmpoweredActionSelect:
                HandlePlayerEmpoweredActionSelect();
                break;
            case BattleState.PlayerMasteryActionSelect:
                HandlePlayerMasteryActionSelect();
                break;
            case BattleState.PlayerMovementSelect:
                HandlePlayerMovementSelect();
                break;
            case BattleState.PlayerExamine:
                HandlePlayerExamine();
                break;
            case BattleState.TargetSelect:
                HandleTargetSelect();
                break;
            case BattleState.AOESelect:
                HandleAOESelect();
                break;
        }
    }

    void HandlePlayerActionCategorySelect()
    {
        if (Input.GetKeyDown(BACK_KEY) && movementChosen)
        {
            GoBackToState();
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
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

    void HandlePlayerMovementSelect()
    {
        DeselectCreature();
        Field.ResetTargetHighlights(activeCreature);

        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPositionY == StateChoices[state].rows - 1))
        {
            ResetSelectableTargets();
            GoBackToState();
        }
        else
        {
            if (selectionPositionY == StateChoices[state].rows - 1)
            {
                // Highlight the Back button
                dialogBox.HighlightBackOption();
            }
            else
            {
                // Reset highlighted Back button to black
                dialogBox.ResetActionSelection();

                SelectableTargets = Field.GetMoveTargets(activeCreature);
                HighlightUnitList(SelectableTargets, HighlightAuraState.Move);

                // Get a reference for the currently selected creature
                int row = selectionPositionY;
                int col = selectionPositionX + ((activeCreature.IsPlayerSlot)? 0 : ENEMY_COL);
                BattleSlot pointerLocation = Field.GetCreature(row, col);

                bool valid_target = SelectableTargets.Contains(pointerLocation);

                if (Input.GetKeyDown(ACCEPT_KEY) && valid_target)
                {
                    Field.SwapSlots(activeCreature, pointerLocation);
                    previousMovementSpace = activeCreature;
                    activeCreature = pointerLocation;
                    UntargetCreature();
                    Field.ResetTargetHighlights(activeCreature);
                    SetMovementChosen(true);
                    ToPlayerActionCategorySelectState(false);
                }
                else
                {
                    SelectCreature(pointerLocation, valid_target);
                }
            }
        }
    }

    void HandlePlayerExamine()
    {
        DeselectCreature();

        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPositionY == StateChoices[state].rows - 1))
        {
            dialogBox.EnableActionOptions();
            GoBackToState();
        }
        else
        {
            if (selectionPositionY == StateChoices[state].rows - 1)
            {
                // Highlight the Back button
                dialogBox.HighlightBackOption();
            } 
            else
            {
                // Reset highlighted Back button to black
                dialogBox.ResetActionSelection();
                BattleSlot pointerLocation = Field.GetCreature(selectionPositionY, selectionPositionX);

                // Show panel for the selected creature
                if (pointerLocation.IsEmpty) 
                {
                    SelectCreature(pointerLocation, false);
                }
                else
                {
                    SelectCreature(pointerLocation, true, true);
                }
            }
        }
    }

    void HandleTargetSelect()
    {
        DeselectCreature();
     
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPositionY == StateChoices[state].rows - 1))
        {
            dialogBox.EnableActionOptions();
            dialogBox.UpdateActionDetails(selectedAction);
            Field.ResetTargetHighlights(activeCreature);
            ResetSelectableTargets();
            GoBackToState();
        }
        else
        {
            if (selectionPositionY == StateChoices[state].rows - 1)
            {
                // Highlight the Back button
                dialogBox.HighlightBackOption();
            }
            else
            {
                // Reset highlighted Back button to black
                dialogBox.ResetActionSelection();
                UpdateSelectableTargets(activeCreature, selectedAction);
                BattleSlot pointerLocation = Field.GetCreature(selectionPositionY, selectionPositionX);


                if (pointerLocation == null)
                {
                    dialogBox.UpdateActionDetails(selectedAction);
                }
                else
                {
                    dialogBox.UpdateActionDetails(selectedAction, activeCreature.Creature, pointerLocation.Creature);
                    (int, int)? coords = GetTargetSelf(pointerLocation);
                    if (DEBUG)
                    {
                        Debug.Log($"Row: {coords.Value.Item1} Col: {coords.Value.Item2}");
                        Debug.Log($"SelectionX: {selectionPositionX}, SelectionY: {selectionPositionY}");
                    }

                    // If the accept key isn't selected, simply highlight the creature's position
                    bool valid_target = SelectableTargets.Contains(pointerLocation);
                    if (Input.GetKeyDown(ACCEPT_KEY) && valid_target)
                    {
                        TargetCreature(pointerLocation);
                        if (selectedAction == null)
                        {
                            Debug.Log("Error: No action selected");
                        }
                        else if (selectedAction.AreaOfEffect != AOE.Single)
                        {
                            ToAOESelectState();
                        }
                        else
                        {
                            Field.ResetTargetHighlights(activeCreature);
                            HighlightTargets();
                            dialogBox.EnableActionOptions();
                            ConfirmAction();
                        }
                    }
                    else
                    {
                        SelectCreature(pointerLocation, valid_target);
                    }
                }
            }
        }
    }

    void HandleAOESelect()
    {
        Field.ResetTargetHighlights(activeCreature);
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPositionY == StateChoices[state].rows - 1))
        {
            ResetCreatureTargets();
            UntargetCreature();
            GoBackToState();
        }
        else
        {
            if (selectionPositionY == StateChoices[state].rows - 1)
            {
                // Highlight the Back button
                dialogBox.HighlightBackOption();
            }
            else
            {
                // Reset highlighted Back button to black
                dialogBox.ResetActionSelection();
                HighlightAuraState highlightAura = (selectedAction.ActionClass == ActionClass.Attack) ? HighlightAuraState.Negative : HighlightAuraState.Positive;
                CreatureTargets = Field.GetAOETargets(TargetedCreature, selectedAction, selectionPositionY, isPlayer: activeCreature.IsPlayerSlot);
                HighlightUnitList(CreatureTargets, highlightAura);

                if (!Input.GetKeyDown(ACCEPT_KEY))
                {
                    // Just show highlighting?
                }
                else
                {
                    if (selectedAction == null)
                    {
                        Debug.Log("Error: No action selected");
                    }
                    else
                    {
                        Field.ResetTargetHighlights(activeCreature);
                        HighlightTargets();
                        dialogBox.EnableActionOptions();
                        ConfirmAction();
                    }
                }
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
            if (movementChosen == true)
            {
                dialogBox.StartTypingDialog("Move action already chosen");
            }
            else
            {
                ToMoveSelectState();
            }
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
        dialogBox.UpdateActionSelection(selection, highlightedAction, activeCreature.Creature);
    }

    int LinearSelectionPosition()
    {
        return selectionPositionY * StateChoices[state].cols + selectionPositionX;
    }

    void ResetCreatureTargets()
    {
        while (CreatureTargets.Count > 0)
        {
            RemoveTarget();
        }
    }

    void RemoveTarget()
    {
        int count = CreatureTargets.Count;
        if (count > 0)
        {
            CreatureTargets[count-1].ToggleStatusWindow(false);
            CreatureTargets[count-1].UpdateSelectionArrow(SelectionArrowState.None);
            CreatureTargets.RemoveAt(count-1);
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
                ToPlayerActionCategorySelectState(true);
                break;
            case BattleState.PlayerCoreActionSelect:
                ToPlayerActionSelectState(state, true);
                break;
            case BattleState.PlayerEmpoweredActionSelect:
                ToPlayerActionSelectState(state, true);
                break;
            case BattleState.PlayerMasteryActionSelect:
                ToPlayerActionSelectState(state, true);
                break;
            case BattleState.TargetSelect:
                ToTargetSelectState(true);
                break;
            case BattleState.PlayerMovementSelect:
                Field.SwapSlots(previousMovementSpace, activeCreature);
                activeCreature = previousMovementSpace;
                ResetMovementSpace();
                SetMovementChosen(false);
                ToMoveSelectState(true);
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

    void ResetMovementSpace()
    {
        previousMovementSpace = null;
    }

    void SetMovementChosen(bool set)
    {
        movementChosen = set;
    }

    void SelectCreature(BattleSlot creature, bool valid, bool showHUD = false)
    {
        if (creature == null) return;

        // Change arrow if the creature isn't already selected
        if (creature != TargetedCreature)
        {
            if (valid)
            {
                creature.UpdateSelectionArrow(SelectionArrowState.Valid);
            }
            else
            {
                creature.UpdateSelectionArrow(SelectionArrowState.Invalid);
            }
        }

        if (showHUD)
        {
            creature.ToggleStatusWindow(true);
        }
        selectedCreature = creature;
    }

    void DeselectCreature()
    {
        if (selectedCreature != null)
        {
            selectedCreature.UpdateSelectionArrow(SelectionArrowState.None);
            selectedCreature.ToggleStatusWindow(false);
            selectedCreature = null;
        }
    }

    void TargetCreature(BattleSlot creature)
    {
        TargetedCreature = creature;
        AddTarget(creature);
    }

    void UntargetCreature()
    {
        if (TargetedCreature != null)
        {
            TargetedCreature.UpdateSelectionArrow(SelectionArrowState.None);
            TargetedCreature = null;
        }
    }

    void HighlightTargets()
    {
        if (selectedAction != null)
        {
            HighlightAuraState highlightAura = (selectedAction.ActionClass == ActionClass.Attack) ? HighlightAuraState.Negative : HighlightAuraState.Positive;
            foreach (var target in CreatureTargets)
            {
                target.UpdateHighlightAura(highlightAura);
            }
        }
    }

    void AddTarget(BattleSlot target)
    {
        target.UpdateSelectionArrow(SelectionArrowState.Selected);
        CreatureTargets.Add(target);
    }

    public (int, int)? GetTargetSelf(BattleSlot creature)
    {
        return (creature.Row, creature.Col);
    }

    public (int, int)? GetStartingTarget()
    {
        bool playerTarget = false;
        if (selectedAction == null || activeCreature == null)
        {
            return null;
        }
        // If this is a support action by the player, or an attack action by an enemy, target the player side
        if (activeCreature.IsPlayerSlot && selectedAction.ActionClass == ActionClass.Support 
            || !activeCreature.IsPlayerSlot && selectedAction.ActionClass == ActionClass.Attack)
        {
            playerTarget = true;
        }
        foreach (var creature in Field.FieldCreatures) 
        {
            BattleSlot slot = creature.BattleSlot;
            if (slot.IsPlayerSlot == playerTarget && slot.ValidTarget)
            {
                return (slot.Row, slot.Col);
            }
        }
        return null;
    }

    public void UpdateSelectableTargets(BattleSlot selectedCreature, ActionBase selectedAction)
    {
        HighlightAuraState highlightAuraState = (selectedAction.ActionClass == ActionClass.Attack) ? HighlightAuraState.Negative : HighlightAuraState.Positive;
        switch (selectedAction.Range)
        {
            case ActionRange.Self:
                SelectableTargets.Clear();
                SelectableTargets.Add(selectedCreature);
                break;
            case ActionRange.Melee:
                SelectableTargets = Field.GetMeleeTargets(selectedCreature);
                break;
            case ActionRange.ShortRanged:
                SelectableTargets = Field.GetRangedTargets(selectedCreature);
                break;
        }
        HighlightUnitList(SelectableTargets, highlightAuraState);
    }

    public void HighlightUnitList(List<BattleSlot> battleSlots, HighlightAuraState highlightAuraState)
    {
        foreach (BattleSlot slot in battleSlots)
        {
            slot.UpdateHighlightAura(highlightAuraState);
        }
    }

    public void SetAOEStateChoices(ActionBase action = null)
    {
        if (action == null)
        {
            StateChoices[BattleState.AOESelect] = (0, 0);
            return;
        }
        if (AOEOptions.TryGetValue(action.AreaOfEffect, out var aoeOption))
        {
            // Modify the y value and assign it to StateChoices
            StateChoices[BattleState.AOESelect] = (rows: aoeOption.y + 1, cols: aoeOption.x);
        }
        else
        {
            Debug.LogError($"Invalid AOE type: {action.AreaOfEffect}");
        }
    }

    private void EndTurn()
    {
        ClearStacks();
        ResetCreatureTargets();
        ResetSelectableTargets();
        ResetMovementSpace();
        SetMovementChosen(false);
        UntargetCreature();
        ResetMovementSpace();
        Field.ResetTargetHighlights();
        ResetSelectedAction();
        if (Field.EnemyCount() == 0)
        {
            EndBattle();
        }
        ToDetermineTurn();
    }

    private void ResetSelectableTargets()
    {
        SelectableTargets.Clear();
    }

    public void EndBattle()
    {
        OnBattleOver(true);

        foreach (var creature in playerTeam.Creatures)
        {
            creature.CleanupBattle();
        }

        foreach (var creature in enemyTeam.Creatures)
        {
            creature.CleanupBattle();
        }

        eventManager.ClearAllSubscriptions();
    }
}

