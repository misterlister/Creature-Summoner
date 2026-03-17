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
    private BattleTile lastTargetTile = null;

    // Current menu position
    private int menuIndex = 0;
    private int cursorRow = 0;
    private int cursorCol = 0;
    private int cursorColMin = 0;
    private int cursorColMax = BATTLE_COLS * 2 - 1;

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
    private BattleTile pinnedTargetTile = null;
    private List<BattleTile> validTargets = new List<BattleTile>();
    private HighlightType activeHighlightType;

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
        hasMovedThisTurn = false;
        previousMovePosition = null;
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
            case PlayerTurnState.AOEConfirm:
                HandleAOEConfirmInput();
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
            case PlayerTurnState.AOEConfirm:
                // no navigation — this step involves confirmation only
                break;
        }
    }

    private void HandleListNavigation()
    {
        int previousIndex = menuIndex;
        int col = menuIndex % MENU_COLS;

        if (Input.GetKeyDown(DOWN_KEY))
        {
            int newIndex = menuIndex + MENU_COLS;
            menuIndex = newIndex < battleUI.MenuPanelActiveCount ? newIndex : menuIndex;
        }
        else if (Input.GetKeyDown(UP_KEY))
        {
            int newIndex = menuIndex - MENU_COLS;
            menuIndex = newIndex >= 0 ? newIndex : menuIndex;
        }
        else if (Input.GetKeyDown(RIGHT_KEY))
        {
            int newIndex = menuIndex + 1;
            menuIndex = col < MENU_COLS - 1 && newIndex < battleUI.MenuPanelActiveCount ? newIndex : menuIndex;
        }
        else if (Input.GetKeyDown(LEFT_KEY))
        {
            int newIndex = menuIndex - 1;
            menuIndex = col > 0 ? newIndex : menuIndex;
        }

        if (menuIndex != previousIndex)
        { 
            battleUI.UpdateMenuSelection(menuIndex);
            OnMenuIndexChanged();
        }
    }

    private void OnMenuIndexChanged()
    {
        switch (currentState)
        {
            case PlayerTurnState.CoreActionSelect:
                if (!IsBackButtonSelected() && menuIndex < activeCreature.Actions.EquippedCoreActions.Count)
                    battleUI.UpdateActionDetails(activeCreature.Actions.EquippedCoreActions[menuIndex]?.Action, activeCreature);
                break;
            case PlayerTurnState.EmpoweredActionSelect:
                if (!IsBackButtonSelected() && menuIndex < activeCreature.Actions.EquippedEmpoweredActions.Count)
                    battleUI.UpdateActionDetails(activeCreature.Actions.EquippedEmpoweredActions[menuIndex]?.Action, activeCreature);
                break;
            case PlayerTurnState.MasteryActionSelect:
                if (!IsBackButtonSelected() && menuIndex < activeCreature.Actions.EquippedMasteryActions.Count)
                    battleUI.UpdateActionDetails(activeCreature.Actions.EquippedMasteryActions[menuIndex]?.Action, activeCreature);
                break;
        }
    }

    private void HandleGridNavigation()
    {
        /*
        string direction = "";
        */
        if (Input.GetKeyDown(DOWN_KEY))
        {
            //direction = "Down";
            cursorRow = Mathf.Clamp(cursorRow + 1, 0, BATTLE_ROWS - 1);
        }
        else if (Input.GetKeyDown(UP_KEY))
        {
            //direction = "Up";
            cursorRow = Mathf.Clamp(cursorRow - 1, 0, BATTLE_ROWS - 1);
        }
        else if (Input.GetKeyDown(RIGHT_KEY))
        { 
            //direction = "Right";
            cursorCol = Mathf.Clamp(cursorCol + 1, cursorColMin, cursorColMax); 
        }
        else if (Input.GetKeyDown(LEFT_KEY))
        {
            //direction = "Left";
            cursorCol = Mathf.Clamp(cursorCol - 1, cursorColMin, cursorColMax); 
        }
        /*
        if (direction != "")
        {
            Debug.Log($"Row {cursorRow}, Col {cursorCol}");
        }
        */
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
            if (IsBackButtonSelected())
            {
                TryPopState();
                return;
            }

            if (battleUI.IsMenuDisabled(menuIndex))
            {
                battleUI.ShowMessage(battleUI.GetDisabledReason(menuIndex));
                return;
            }

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
                    TransitionToMovementSelect();
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
    }

    private void HandleMovementInput()
    {
        if (Input.GetKeyDown(BACK_KEY))
        {
            ResetValidTargets();
            TryPopState();
            return;
        }

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
            SetHoveredTile(tile, isValid);
        }
    }

    private void HandleExamineInput()
    {
        if (Input.GetKeyDown(BACK_KEY))
        {
            TryPopState();
            return;
        }

        var cursorPos = GetCursorBattlePosition();
        var tile = battlefield.GetTile(cursorPos);
        SetHoveredTile(tile, tile != null && tile.IsOccupied);
    }

    private void HandleTargetSelectInput()
    {
        if (Input.GetKeyDown(BACK_KEY))
        {
            ResetValidTargets();
            TryPopState();
            return;
        }

        var cursorPos = GetCursorBattlePosition();
        var tile = battlefield.GetTile(cursorPos);
        bool isValid = validTargets.Contains(tile);

        if (tile != lastTargetTile)
        {
            lastTargetTile = tile;
            RefreshTargetHighlights(tile, isValid);
            battleUI.UpdateActionDetails(
                selectedAction,
                activeCreature,
                tile != null && tile.IsOccupied ? tile.OccupyingCreature : null);
        }

        if (Input.GetKeyDown(ACCEPT_KEY) && isValid)
        {
            primaryTarget = tile;
            lastTargetTile = null;
            if (selectedAction.AreaOfEffect != AOE.Single)
            {
                PinTargetTile(tile);
                TransitionToAOEConfirm();
            }
            else
                ConfirmAction();
        }
        else
        {
            SetHoveredTile(tile, isValid);
        }
    }

    private void HandleAOEConfirmInput()
    {
        if (Input.GetKeyDown(BACK_KEY))
        {
            ResetValidTargets();
            TryPopState();
            return;
        }

        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            ConfirmAction();
        }
    }

    #endregion

    #region State Transitions

    private void TransitionToActionCategorySelect()
    {
        currentState = PlayerTurnState.ActionCategorySelect;
        ResetSelection();
        battleUI.ShowActionCategoryMenu(activeCreature, hasMovedThisTurn);
        battleUI.ShowMessage("Choose which kind of action to take");
    }

    private void TransitionToCoreActionSelect()
    {
        PushState();
        currentState = PlayerTurnState.CoreActionSelect;
        battleUI.ShowActionMenu(activeCreature.Actions.EquippedCoreActions);
    }

    private void TransitionToEmpoweredActionSelect()
    {
        PushState();
        currentState = PlayerTurnState.EmpoweredActionSelect;
        battleUI.ShowActionMenu(activeCreature.Actions.EquippedEmpoweredActions);
    }

    private void TransitionToMasteryActionSelect()
    {
        PushState();
        currentState = PlayerTurnState.MasteryActionSelect;
        battleUI.ShowActionMenu(activeCreature.Actions.EquippedMasteryActions);
    }

    private void TransitionToMovementSelect()
    {
        PushState();
        currentState = PlayerTurnState.MovementSelect;

        var creaturePos = battlefield.GetBattlePosition(activeCreature);
        cursorRow = creaturePos.Row;
        cursorCol = creaturePos.GlobalCol;

        // Player grid only (cols 0-2)
        cursorColMin = 0;
        cursorColMax = GRID_COLS - 1;

        validTargets = battleManager.GetMoveTargets(activeCreature);
        HighlightTiles(validTargets, HighlightType.MoveTarget);
        battleUI.ShowMovementMenu();
    }

    private void TransitionToExamine()
    {
        PushState();
        currentState = PlayerTurnState.Examine;

        var creaturePos = battlefield.GetBattlePosition(activeCreature);
        cursorRow = creaturePos.Row;
        cursorCol = creaturePos.GlobalCol;

        // Both grids (cols 0-5)
        cursorColMin = 0;
        cursorColMax = BATTLE_COLS - 1;

        battleUI.ShowExamineMenu();
    }

    private void TransitionToTargetSelect()
    {
        PushState();
        currentState = PlayerTurnState.TargetSelect;

        if (selectedAction.Role == ActionRole.Offensive)
        {
            cursorColMin = ENEMY_COL;           // enemy grid (cols 3-5)
            cursorColMax = BATTLE_COLS - 1;
            activeHighlightType = HighlightType.NegativeTarget;
        }
        else
        {
            cursorColMin = 0;                   // player grid (cols 0-2)
            cursorColMax = GRID_COLS - 1;
            activeHighlightType = HighlightType.PositiveTarget;
        }

        validTargets = selectedAction.GetValidTargets(activeCreature, battlefield);
        HighlightTiles(validTargets, activeHighlightType);

        var firstTarget = GetFirstValidTarget();
        if (firstTarget.HasValue)
        {
            cursorRow = firstTarget.Value.Row;
            cursorCol = firstTarget.Value.GlobalCol;
        }

        battleUI.ShowTargetSelectMenu();
    }

    private void TransitionToAOEConfirm()
    {
        PushState();
        currentState = PlayerTurnState.AOEConfirm;

        var grid = battlefield.GetGrid(activeCreature.TeamSide);
        var aoeResult = AOETargetCalculator.GetTargets(primaryTarget, selectedAction.AreaOfEffect, grid, activeCreature.TeamSide);
        battlefield.ClearAllHighlights();
        HighlightTiles(aoeResult.AllTargets(), activeHighlightType);

        battleUI.ShowMessage("Confirm area of effect");
    }

    #endregion

    #region Actions

    private void ExecuteMovement(BattleTile targetTile)
    {
        battleManager.ClearActiveCreatureHighlight();

        previousMovePosition = battlefield.GetBattlePosition(activeCreature);

        // Move creature
        battlefield.SwapCreatures(activeCreature.CurrentTile, targetTile);

        hasMovedThisTurn = true;
        battlefield.ClearAllHighlights();

        battleManager.SetActiveCreatureHighlight();

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

        ClearHoveredTile();

        // Clean up current state before leaving
        if (currentState == PlayerTurnState.Examine)
            battleUI.HideExaminePanels();

        var snapshot = stateStack.Pop();
        currentState = snapshot.State;
        menuIndex = snapshot.MenuIndex;
        cursorRow = snapshot.CursorRow;
        cursorCol = snapshot.CursorCol;
        RestoreStateUI();

        if (IsGridNavigationState(currentState))
        {
            var tile = battlefield.GetTile(GetCursorBattlePosition());
            bool isValid = validTargets.Contains(tile);
            SetHoveredTile(tile, isValid);
        }

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
                battleUI.ShowActionMenu(activeCreature.Actions.EquippedCoreActions);
                break;
            case PlayerTurnState.EmpoweredActionSelect:
                battleUI.ShowActionMenu(activeCreature.Actions.EquippedEmpoweredActions);
                break;
            case PlayerTurnState.MasteryActionSelect:
                battleUI.ShowActionMenu(activeCreature.Actions.EquippedMasteryActions);
                break;
            case PlayerTurnState.TargetSelect:
                battleUI.ShowTargetSelectMenu();
                break;
            case PlayerTurnState.MovementSelect:
                if (hasMovedThisTurn && previousMovePosition != null)
                {
                    hasMovedThisTurn = false;
                }
                validTargets = battleManager.GetMoveTargets(activeCreature);
                HighlightTiles(validTargets, HighlightType.MoveTarget);
                battleUI.ShowMovementMenu();
                break;
        }
    }

    #endregion

    #region Helper Methods

    private int GetCurrentMenuSize() => battleUI.MenuPanelActiveCount;

    private BattlePosition GetCursorBattlePosition() => new BattlePosition(cursorRow, cursorCol);

    private bool IsBackButtonSelected() => menuIndex == GetCurrentMenuSize() - 1;

    private BattlePosition? GetFirstValidTarget()
    {
        if (validTargets.Count > 0)
            return validTargets[0].BattlefieldPosition;
        return null;
    }

    private void ResetSelection()
    {
        menuIndex = 0;
    }

    private void ResetValidTargets()
    {
        validTargets.Clear();
        PinTargetTile(null);
        battlefield.ClearAllHighlights();
    }
    private void RefreshTargetHighlights(BattleTile tile, bool isValid)
    {
        battlefield.ClearAllHighlights();
        HighlightTiles(validTargets, activeHighlightType);

        if (tile != null && isValid && selectedAction.AreaOfEffect != AOE.Single)
        {
            var grid = battlefield.GetGrid(activeCreature.TeamSide);
            var aoeResult = AOETargetCalculator.GetTargets(tile, selectedAction.AreaOfEffect, grid, activeCreature.TeamSide);
            HighlightTiles(aoeResult.AllTargets(), activeHighlightType);
        }
    }

    private void HighlightTiles(List<BattleTile> tiles, HighlightType highlightType)
    {
        battlefield.HighlightTiles(tiles, highlightType);
    }

    private void SetHoveredTile(BattleTile newTile, bool isValid)
    {
        // No change — do nothing
        if (newTile == hoveredTile) return;

        // Clear previous hover
        if (hoveredTile != null)
        {
            var oldUI = battlefield.GetTileUI(hoveredTile.BattlefieldPosition);
            oldUI?.ClearHoverHighlight();
        }

        hoveredTile = newTile;

        if (hoveredTile == null) return;

        // Apply new hover
        var newUI = battlefield.GetTileUI(hoveredTile.BattlefieldPosition);
        newUI?.SetHoverHighlight(isValid ? HighlightType.ValidTarget : HighlightType.InvalidTarget);

        if (currentState == PlayerTurnState.Examine)
            battleUI.BindExamineCreature(hoveredTile.IsOccupied ? hoveredTile.OccupyingCreature : null);
    }

    private void ClearHoveredTile()
    {
        SetHoveredTile(null, false);
    }

    private void PinTargetTile(BattleTile tile)
    {
        if (pinnedTargetTile != null)
        {
            var oldUI = battlefield.GetTileUI(pinnedTargetTile.BattlefieldPosition);
            oldUI?.ClearPinnedHighlight();
        }
        pinnedTargetTile = tile;
        if (tile != null)
        {
            var tileUI = battlefield.GetTileUI(tile.BattlefieldPosition);
            tileUI?.SetPinnedHighlight(activeHighlightType);
        }
    }

    private bool IsGridNavigationState(PlayerTurnState state) => state switch
    {
        PlayerTurnState.MovementSelect => true,
        PlayerTurnState.TargetSelect => true,
        PlayerTurnState.Examine => true,
        _ => false
    };

    #endregion
}