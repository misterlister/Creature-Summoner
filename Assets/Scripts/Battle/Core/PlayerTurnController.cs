using System;
using System.Collections.Generic;
using UnityEngine;
using static GameConstants;
using static BattleSystemConstants;
using static BattleUIConstants;

/// <summary>
/// Handles all player turn input, state management, and UI interaction.
/// Separated from BattleManager to keep battle orchestration clean.
/// </summary>
public class PlayerTurnController
{
    private BattleManager battleManager;
    private UnifiedBattlefield battlefield;
    private BattleUI battleUI;

    // Active creature
    private Creature activeCreature;

    // Selection state
    private ActionBase selectedAction;
    private BattleTile primaryTarget;
    private int selectionYChoice = 0;
    private bool confirmingAOE = false;


    // Current menu position
    private int menuIndex = 0;
    private int menuCount = 0;
    private int cursorRow = 0;
    private int cursorCol = 0;

    private int MenuRows => Mathf.CeilToInt((float)menuCount / MENU_COLS);

    // Movement tracking
    private bool hasMovedThisTurn = false;
    private BattlePosition? previousMovePosition = null;

    // State tracking - only player-specific substates
    private PlayerTurnState currentState;

    private struct StateSnapshot
    {
        public PlayerTurnState State;
        public int MenuIndex;
        public int CursorRow;
        public int CursorCol;

        public StateSnapshot(PlayerTurnState state, int menuIndex, int cursorRow, int cursorCol)
        {
            State = state;
            MenuIndex = menuIndex;
            CursorRow = cursorRow;
            CursorCol = cursorCol;
        }
    }

    private Stack<StateSnapshot> stateStack = new();

    // Currently highlighted/selected creatures for UI
    private BattleTile hoveredTile;
    private List<BattleTile> validTargets = new List<BattleTile>();

    private static readonly ActionCategoryMenuOptions[] ActionCategories =
    (ActionCategoryMenuOptions[])Enum.GetValues(typeof(ActionCategoryMenuOptions));


    public PlayerTurnController(BattleManager manager, UnifiedBattlefield field, BattleUI ui)
    {
        battleManager = manager;
        battlefield = field;
        battleUI = ui;
    }

    public void StartTurn(Creature creature)
    {
        activeCreature = creature;

        // Reset state
        selectedAction = null;
        primaryTarget = null;
        selectionYChoice = 0;
        hasMovedThisTurn = false;
        previousMovePosition = null;
        confirmingAOE = false;
        ClearStateStack();

        // Start at action category selection
        TransitionToActionCategorySelect();
    }

    #region Input Handling

    public void HandleInput()
    {
        // Only process input during player's turn
        if (battleManager.CurrentState != BattleState.PlayerTurn)
            return;

        // Handle menu navigation
        HandleMenuNavigation();

        // Handle state-specific input
        switch (currentState)
        {
            case PlayerTurnState.ActionCategorySelect:
                HandleActionCategoryInput();
                break;
            case PlayerTurnState.CoreActionSelect:
                HandleCoreActionInput();
                break;
            case PlayerTurnState.EmpoweredActionSelect:
                HandleEmpoweredActionInput();
                break;
            case PlayerTurnState.MasteryActionSelect:
                HandleMasteryActionInput();
                break;
            case PlayerTurnState.MovementSelect:
                HandleMovementInput();
                break;
            case PlayerTurnState.Examine:
                HandleExamineInput();
                break;
            case PlayerTurnState.TargetSelect:
                HandleTargetSelectInput();
                break;
        }
    }

    private void HandleMenuNavigation()
    {
        switch (currentState)
        {
            case PlayerTurnState.ActionCategorySelect:
            case PlayerTurnState.CoreActionSelect:
            case PlayerTurnState.EmpoweredActionSelect:
            case PlayerTurnState.MasteryActionSelect:
                HandleListNavigation();
                break;
            case PlayerTurnState.MovementSelect:
            case PlayerTurnState.TargetSelect:
            case PlayerTurnState.Examine:
                HandleGridNavigation();
                break;
        }
    }

    private void HandleListNavigation()
    {
        int row = menuIndex / MENU_COLS;
        int col = menuIndex % MENU_COLS;

        if (Input.GetKeyDown(DOWN_KEY))
        {
            int newIndex = menuIndex + MENU_COLS;
            menuIndex = newIndex < menuCount ? newIndex : menuIndex;
        }
        else if (Input.GetKeyDown(UP_KEY))
        {
            int newIndex = menuIndex - MENU_COLS;
            menuIndex = newIndex >= 0 ? newIndex : menuIndex;
        }
        else if (Input.GetKeyDown(RIGHT_KEY))
        {
            int newIndex = menuIndex + 1;
            // Wrap within row only
            menuIndex = col < MENU_COLS - 1 && newIndex < menuCount ? newIndex : menuIndex;
        }
        else if (Input.GetKeyDown(LEFT_KEY))
        {
            int newIndex = menuIndex - 1;
            // Wrap within row only
            menuIndex = col > 0 ? newIndex : menuIndex;
        }
    }

    private void HandleGridNavigation()
    {
        if (Input.GetKeyDown(DOWN_KEY))
            cursorRow = Mathf.Clamp(cursorRow + 1, 0, BATTLE_ROWS - 1);
        else if (Input.GetKeyDown(UP_KEY))
            cursorRow = Mathf.Clamp(cursorRow - 1, 0, BATTLE_ROWS - 1);
        else if (Input.GetKeyDown(RIGHT_KEY))
            cursorCol = Mathf.Clamp(cursorCol + 1, 0, BATTLE_COLS - 1);
        else if (Input.GetKeyDown(LEFT_KEY))
            cursorCol = Mathf.Clamp(cursorCol - 1, 0, BATTLE_COLS - 1);
    }

    private void SetMenuSize(int count)
    {
        menuCount = count + 1; // +1 for Back
        menuIndex = 0;
    }

    #endregion

    #region State-Specific Input Handlers

    private void HandleActionCategoryInput()
    {
        if (Input.GetKeyDown(BACK_KEY) && hasMovedThisTurn)
        {
            // Can go back and undo movement
            TryPopState();
            return;
        }
        
        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            switch (ActionCategories[menuIndex])
            {
                case ActionCategoryMenuOptions.CoreActions:
                    TransitionToCoreActionSelect();
                    break;
                case ActionCategoryMenuOptions.EmpoweredActions:
                    TransitionToEmpoweredActionSelect();
                    break;
                case ActionCategoryMenuOptions.MasteryActions:
                    TransitionToMasteryActionSelect();
                    break;
                case ActionCategoryMenuOptions.Move:
                    if (hasMovedThisTurn) battleUI.ShowMessage("Already moved this turn");
                    else TransitionToMovementSelect();
                    break;
                case ActionCategoryMenuOptions.Examine:
                    TransitionToExamine();
                    break;
                case ActionCategoryMenuOptions.ClassAction:
                    battleUI.ShowMessage("Class actions not implemented yet");
                    break;
                case ActionCategoryMenuOptions.Flee:
                    battleUI.ShowMessage("Fleeing not implemented yet");
                    break;
            }
        }
        else
        {
            battleUI.UpdateActionCategorySelection(menuIndex);
        }
    }

    private void HandleCoreActionInput()
    {
        if (Input.GetKeyDown(BACK_KEY))
        {
            TryPopState();
            return;
        }

        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            if (menuIndex < activeCreature.Actions.EquippedCoreActions.Count)
            {
                var action = activeCreature.Actions.EquippedCoreActions[menuIndex]?.Action;
                if (action != null)
                {
                    selectedAction = action;
                    TransitionToTargetSelect();
                }
            }
        }
        else
        {
            UpdateActionHighlight(activeCreature.Actions.EquippedCoreActions);
        }
    }

    private void HandleEmpoweredActionInput()
    {
        if (Input.GetKeyDown(BACK_KEY))
        {
            TryPopState();
            return;
        }

        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            if (menuIndex < activeCreature.Actions.EquippedEmpoweredActions.Count)
            {
                var action = activeCreature.Actions.EquippedEmpoweredActions[menuIndex]?.Action;
                if (action != null)
                {
                    if (action.EnergyValue <= activeCreature.Energy)
                    {
                        selectedAction = action;
                        TransitionToTargetSelect();
                    }
                    else
                    {
                        battleUI.ShowMessage($"{activeCreature.Nickname} does not have enough energy");
                    }
                }
            }
        }
        else
        {
            UpdateActionHighlight(activeCreature.Actions.EquippedEmpoweredActions);
        }
    }

    private void HandleMasteryActionInput()
    {
        if (Input.GetKeyDown(BACK_KEY))
        {
            TryPopState();
            return;
        }
        
        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            if (menuIndex < activeCreature.Actions.EquippedMasteryActions.Count)
            {
                var action = activeCreature.Actions.EquippedMasteryActions[menuIndex]?.Action;
                if (action != null)
                {
                    // TODO: Implement mastery action logic
                    Debug.Log($"Mastery action: {action.ActionName}");
                }
            }
        }
        else
        {
            UpdateActionHighlight(activeCreature.Actions.EquippedMasteryActions);
        }
    }

    private void HandleMovementInput()
    {
        ClearHoveredTile();

        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && IsBackButtonSelected()))
        {
            ResetValidTargets();
            TryPopState();
        }
        else if (IsBackButtonSelected())
        {
            battleUI.HighlightBackButton();
        }
        else
        {
            battleUI.ResetBackButton();

            // Get move targets
            var moveTargets = battleManager.GetMoveTargets(activeCreature);
            validTargets = moveTargets;
            HighlightTiles(validTargets, HighlightType.MoveTarget);

            // Get tile at cursor
            var cursorPos = GetCursorBattlePosition();
            var tile = battlefield.GetTile(cursorPos);

            bool isValid = validTargets.Contains(tile);

            if (Input.GetKeyDown(ACCEPT_KEY) && isValid)
            {
                ExecuteMovement(tile);
            }
            else
            {
                HoverTile(tile, isValid);
            }
        }
    }

    private void HandleExamineInput()
    {
        ClearHoveredTile();

        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && IsBackButtonSelected()))
        {
            TryPopState();
        }
        else if (IsBackButtonSelected())
        {
            battleUI.HighlightBackButton();
        }
        else
        {
            battleUI.ResetBackButton();

            var cursorPos = GetCursorBattlePosition();
            var tile = battlefield.GetTile(cursorPos);

            if (tile != null && tile.IsOccupied)
            {
                HoverTile(tile, true, showStatus: true);
            }
            else
            {
                HoverTile(tile, false);
            }
        }
    }

    private void HandleTargetSelectInput()
    {
        ClearHoveredTile();

        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && IsBackButtonSelected()))
        {
            if (confirmingAOE)
            {
                // Step back to primary target selection
                confirmingAOE = false;
                battlefield.ClearAllHighlights();
            }
            else
            {
                battlefield.ClearAllHighlights();
                ResetValidTargets();
                TryPopState();
            }
            return;
        }

        if (IsBackButtonSelected())
        {
            battleUI.HighlightBackButton();
            return;
        }
        
        battleUI.ResetBackButton();

        HighlightType highlightType = selectedAction.Role == ActionRole.Offensive
            ? HighlightType.NegativeTarget
            : HighlightType.PositiveTarget;
        HighlightTiles(validTargets, highlightType);

        if (confirmingAOE)
        {
            battleUI.ShowMessage("Confirm area of effect");

            var grid = battlefield.GetGrid(activeCreature.TeamSide);
            var aoeResult = AOETargetCalculator.GetTargets(
                primaryTarget,
                selectedAction.AreaOfEffect,
                grid,
                activeCreature.TeamSide);

            battlefield.ClearAllHighlights();
            HighlightTiles(aoeResult.AllTargets(), highlightType);

            if (Input.GetKeyDown(ACCEPT_KEY))
            {
                ConfirmAction();
            }
        }
        else
        {
            battleUI.ShowMessage("Choose target");

            // Get valid targets for selected action
            validTargets = selectedAction.GetValidTargets(activeCreature, battlefield);
            var cursorPos = GetCursorBattlePosition();
            var tile = battlefield.GetTile(cursorPos);
            bool isValid = validTargets.Contains(tile);

            // Show AOE preview on hover, valid targets otherwise
            if (tile != null && isValid && selectedAction.AreaOfEffect != AOE.Single)
            {
                var grid = battlefield.GetGrid(activeCreature.TeamSide);
                var aoeResult = AOETargetCalculator.GetTargets(
                    tile,
                    selectedAction.AreaOfEffect,
                    grid,
                    activeCreature.TeamSide);

                battlefield.ClearAllHighlights();
                HighlightTiles(aoeResult.AllTargets(), highlightType);
            }
            else
            {
                HighlightTiles(validTargets, highlightType);
            }

            if (Input.GetKeyDown(ACCEPT_KEY) && isValid)
            {
                primaryTarget = tile;

                if (selectedAction.AreaOfEffect != AOE.Single)
                    confirmingAOE = true;
                else
                    ConfirmAction();
            }
            else
            {
                HoverTile(tile, isValid);

                if (tile != null && tile.IsOccupied)
                    battleUI.UpdateActionDetails(selectedAction, activeCreature, tile.OccupyingCreature);
                else
                    battleUI.UpdateActionDetails(selectedAction, activeCreature);
            }
        }
    }

    #endregion

    #region State Transitions

    private void TransitionToActionCategorySelect()
    {
        currentState = PlayerTurnState.ActionCategorySelect;
        ResetSelection();
        battleUI.ShowActionCategoryMenu();
        battleUI.ShowMessage("Choose which kind of action to take");
    }

    private void TransitionToCoreActionSelect()
    {
        PushState();
        currentState = PlayerTurnState.CoreActionSelect;
        SetMenuSize(activeCreature.Actions.EquippedCoreActions.Count);
        battleUI.ShowCoreActionMenu(activeCreature.Actions.EquippedCoreActions);
    }

    private void TransitionToEmpoweredActionSelect()
    {
        PushState();
        currentState = PlayerTurnState.EmpoweredActionSelect;
        SetMenuSize(activeCreature.Actions.EquippedEmpoweredActions.Count);
        battleUI.ShowEmpoweredActionMenu(activeCreature.Actions.EquippedEmpoweredActions);
    }

    private void TransitionToMasteryActionSelect()
    {
        PushState();
        currentState = PlayerTurnState.MasteryActionSelect;
        SetMenuSize(activeCreature.Actions.EquippedMasteryActions.Count);
        battleUI.ShowMasteryActionMenu(activeCreature.Actions.EquippedMasteryActions);
    }

    private void TransitionToMovementSelect()
    {
        PushState();
        currentState = PlayerTurnState.MovementSelect;

        // Start cursor at active creature's position
        var creaturePos = battlefield.GetBattlePosition(activeCreature);
        cursorRow = creaturePos.Row;
        cursorCol = creaturePos.LocalCol;

        battleUI.ShowMovementMenu();
    }

    private void TransitionToExamine()
    {
        PushState();
        currentState = PlayerTurnState.Examine;

        // Start cursor at active creature's position
        var creaturePos = battlefield.GetBattlePosition(activeCreature);
        cursorRow = creaturePos.Row;
        cursorCol = creaturePos.GlobalCol;

        battleUI.ShowExamineMenu();
    }

    private void TransitionToTargetSelect()
    {
        PushState();
        currentState = PlayerTurnState.TargetSelect;

        // Start cursor at first valid target
        var firstTarget = GetFirstValidTarget();
        if (firstTarget.HasValue)
        {
            cursorRow = firstTarget.Value.Row;
            cursorCol = firstTarget.Value.GlobalCol;
        }

        battleUI.ShowTargetSelectMenu();
    }

    #endregion

    #region Actions

    private void ExecuteMovement(BattleTile targetTile)
    {
        var sourcePos = battlefield.GetBattlePosition(activeCreature);
        var targetPos = targetTile.BattlefieldPosition;

        previousMovePosition = sourcePos;

        // Move creature
        battlefield.SwapCreatures(activeCreature, targetTile.OccupyingCreature);

        hasMovedThisTurn = true;
        battlefield.ClearAllHighlights();

        TransitionToActionCategorySelect();
    }

    private void ConfirmAction()
    {
        // Get final target list
        var grid = battlefield.GetGrid(activeCreature.TeamSide);
        var selectedTargets = AOETargetCalculator.GetTargets(
        primaryTarget,
        selectedAction.AreaOfEffect,
        grid,
        activeCreature.TeamSide);

        battlefield.ClearAllHighlights();

        // Tell battle manager to execute
        battleManager.ExecutePlayerAction(selectedAction, selectedTargets);
    }

    #endregion

    #region State Stack Management
    private void PushState()
    {
        stateStack.Push(new StateSnapshot(currentState, menuIndex, cursorRow, cursorCol));
    }

    private bool TryPopState()
    {
        if (stateStack.Count == 0) return false;
        var snapshot = stateStack.Pop();
        currentState = snapshot.State;
        menuIndex = snapshot.MenuIndex;
        cursorRow = snapshot.CursorRow;
        cursorCol = snapshot.CursorCol;
        confirmingAOE = false;
        RestoreStateUI();
        return true;
    }

    private void ClearStateStack() => stateStack.Clear();

    private void RestoreStateUI()
    {
        // Restore UI based on state we're returning to
        switch (currentState)
        {
            case PlayerTurnState.ActionCategorySelect:
                TransitionToActionCategorySelect();
                break;
            case PlayerTurnState.CoreActionSelect:
                battleUI.ShowCoreActionMenu(activeCreature.Actions.EquippedCoreActions);
                break;
            case PlayerTurnState.EmpoweredActionSelect:
                battleUI.ShowEmpoweredActionMenu(activeCreature.Actions.EquippedEmpoweredActions);
                break;
            case PlayerTurnState.MasteryActionSelect:
                battleUI.ShowMasteryActionMenu(activeCreature.Actions.EquippedMasteryActions);
                break;
            case PlayerTurnState.TargetSelect:
                battleUI.ShowTargetSelectMenu();
                break;
            case PlayerTurnState.MovementSelect:
                // Undo movement
                if (hasMovedThisTurn && previousMovePosition != null)
                {
                    // TODO: Implement movement undo
                    hasMovedThisTurn = false;
                }
                battleUI.ShowMovementMenu();
                break;
        }
    }

    #endregion

    #region Helper Methods

    private int GetCurrentMenuSize() => currentState switch
    {
        PlayerTurnState.ActionCategorySelect => ActionCategories.Length,
        PlayerTurnState.CoreActionSelect => activeCreature.Actions.EquippedCoreActions.Count + 1,
        PlayerTurnState.EmpoweredActionSelect => activeCreature.Actions.EquippedEmpoweredActions.Count + 1,
        PlayerTurnState.MasteryActionSelect => activeCreature.Actions.EquippedMasteryActions.Count + 1,
        _ => 0
    };

    private BattlePosition GetCursorBattlePosition() => new BattlePosition(cursorRow, cursorCol);

    private bool IsBackButtonSelected() => menuIndex == GetCurrentMenuSize() - 1;

    private BattlePosition? GetFirstValidTarget()
    {
        var targets = selectedAction.GetValidTargets(activeCreature, battlefield);
        if (targets.Count > 0)
        {
            return targets[0].BattlefieldPosition;
        }
        return null;
    }

    private void ResetSelection()
    {
        menuIndex = 0;
        menuCount = 0;
    }

    private void ResetValidTargets()
    {
        validTargets.Clear();
        battlefield.ClearAllHighlights();
    }

    private void UpdateActionHighlight(IReadOnlyList<CreatureAction> actions)
    {
        ActionBase action = null;

        if (menuIndex >= 0 && menuIndex < actions.Count)
        {
            action = actions[menuIndex]?.Action;
        }

        battleUI.UpdateActionSelection(menuIndex, action, activeCreature);
    }

    private void HighlightTiles(List<BattleTile> tiles, HighlightType highlightType)
    {
        battlefield.HighlightTiles(tiles, highlightType);
    }

    private void HoverTile(BattleTile tile, bool isValid, bool showStatus = false)
    {
        if (tile == null) return;

        hoveredTile = tile;

        // Update UI to show hover state
        var tileUI = battlefield.GetTileUI(tile.BattlefieldPosition);
        tileUI?.SetHighlight(isValid ? HighlightType.ValidTarget : HighlightType.InvalidTarget);

        if (tile.IsOccupied)
            battleUI.BindExamineCreature(tile.OccupyingCreature);
        else
            battleUI.BindExamineCreature(null);
    }

    private void ClearHoveredTile()
    {
        if (hoveredTile != null)
        {
            var tileUI = battlefield.GetTileUI(hoveredTile.BattlefieldPosition);
            tileUI?.ClearHighlight();
            hoveredTile = null;
        }

        battleUI.BindExamineCreature(null);
    }

    #endregion
}