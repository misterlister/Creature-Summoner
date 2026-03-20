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

    // Current menu position
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
        ResetSelection();
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
        Direction? direction =
            Input.GetKeyDown(DOWN_KEY) ? Direction.Down :
            Input.GetKeyDown(UP_KEY) ? Direction.Up :
            Input.GetKeyDown(RIGHT_KEY) ? Direction.Right :
            Input.GetKeyDown(LEFT_KEY) ? Direction.Left :
            null;

        if (direction == null) return;

        battleUI.NavigateMenuSelection(direction.Value);

        if (currentState is PlayerTurnState.CoreActionSelect
                  or PlayerTurnState.EmpoweredActionSelect
                  or PlayerTurnState.MasteryActionSelect)
        {
            UpdateActionDetails();
        }
    }

    private void UpdateActionDetails()
    {
        IReadOnlyList<CreatureAction> actions = currentState switch
        {
            PlayerTurnState.CoreActionSelect => activeCreature.Actions.EquippedCoreActions,
            PlayerTurnState.EmpoweredActionSelect => activeCreature.Actions.EquippedEmpoweredActions,
            PlayerTurnState.MasteryActionSelect => activeCreature.Actions.EquippedMasteryActions,
            _ => null
        };

        if (actions == null) return;

        if (battleUI.IsBackButtonSelected())
        {
            battleUI.UpdateActionDetails(null);
            return;
        }

        int index = battleUI.CurrentMenuIndex;
        if (index < actions.Count)
            battleUI.UpdateActionDetails(actions[index]?.Action, activeCreature);
    }

    private void HandleGridNavigation()
    {
        
        string direction = "";
        
        if (Input.GetKeyDown(DOWN_KEY))
        {
            direction = "Down";
            cursorRow = Mathf.Clamp(cursorRow + 1, 0, BATTLE_ROWS - 1);
        }
        else if (Input.GetKeyDown(UP_KEY))
        {
            direction = "Up";
            cursorRow = Mathf.Clamp(cursorRow - 1, 0, BATTLE_ROWS - 1);
        }
        else if (Input.GetKeyDown(RIGHT_KEY))
        { 
            direction = "Right";
            cursorCol = Mathf.Clamp(cursorCol + 1, cursorColMin, cursorColMax); 
        }
        else if (Input.GetKeyDown(LEFT_KEY))
        {
            direction = "Left";
            cursorCol = Mathf.Clamp(cursorCol - 1, cursorColMin, cursorColMax); 
        }
        
        if (direction != "")
        {
            Debug.Log($"Row {cursorRow}, Col {cursorCol}");
        }
        
    }

    #endregion

    #region State-Specific Input Handlers

    private void HandleActionCategoryInput()
    {
        if (Input.GetKeyDown(BACK_KEY) || (IsBackButtonSelected() && Input.GetKeyDown(ACCEPT_KEY)))
        { 
            // Can only go back to undo movement from here
            TryPopState();
            return;
        }

        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            int index = battleUI.CurrentMenuIndex;

            if (index >= ActionCategories.Length) return;

            if (battleUI.IsMenuDisabled(index))
            {
                battleUI.ShowMessage(battleUI.GetDisabledReason(index));
                return;
            }

            switch (ActionCategories[index])
            {
                case ActionCategoryMenuOptions.CoreActions:
                    PushState();
                    ResetSelection();
                    TransitionToCoreActionSelect();
                    break;
                case ActionCategoryMenuOptions.EmpoweredActions:
                    PushState();
                    ResetSelection();
                    TransitionToEmpoweredActionSelect();
                    break;
                case ActionCategoryMenuOptions.MasteryActions:
                    PushState();
                    ResetSelection();
                    TransitionToMasteryActionSelect();
                    break;
                case ActionCategoryMenuOptions.Move:
                    PushState();
                    ResetSelection();
                    TransitionToMovementSelect();
                    break;
                case ActionCategoryMenuOptions.Examine:
                    PushState();
                    ResetSelection();
                    TransitionToExamine();
                    break;
                case ActionCategoryMenuOptions.ClassAction:
                    battleUI.ShowMessage("Class actions not implemented yet");
                    break;
                case ActionCategoryMenuOptions.EndTurn:
                    battleManager.EndPlayerTurn();
                    break;
                case ActionCategoryMenuOptions.Flee:
                    battleUI.ShowMessage("Fleeing not implemented yet");
                    break;
            }
        }
    }

    private void HandleCoreActionInput()
    {
        if (Input.GetKeyDown(BACK_KEY) || (IsBackButtonSelected() && Input.GetKeyDown(ACCEPT_KEY)))
        {
            TryPopState();
            return;
        }

        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            int index = battleUI.CurrentMenuIndex;
            if (index < activeCreature.Actions.EquippedCoreActions.Count)
            {
                var action = activeCreature.Actions.EquippedCoreActions[index]?.Action;
                if (action != null)
                {
                    selectedAction = action;
                    PushState();
                    ResetSelection();
                    TransitionToTargetSelect();
                }
            }
        }
    }

    private void HandleEmpoweredActionInput()
    {
        if (Input.GetKeyDown(BACK_KEY) || (IsBackButtonSelected() && Input.GetKeyDown(ACCEPT_KEY)))
        {
            TryPopState();
            return;
        }

        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            int index = battleUI.CurrentMenuIndex;
            if (index < activeCreature.Actions.EquippedEmpoweredActions.Count)
            {
                var action = activeCreature.Actions.EquippedEmpoweredActions[index]?.Action;
                if (action != null)
                {
                    if (action.EnergyValue <= activeCreature.Energy)
                    {
                        selectedAction = action;
                        PushState();
                        ResetSelection();
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
         if (Input.GetKeyDown(BACK_KEY) ||  (IsBackButtonSelected() && Input.GetKeyDown(ACCEPT_KEY)))
        {
            TryPopState();
            return;
        }
        
        if (Input.GetKeyDown(ACCEPT_KEY))
        {
            int index = battleUI.CurrentMenuIndex;
            if (index < activeCreature.Actions.EquippedMasteryActions.Count)
            {
                var action = activeCreature.Actions.EquippedMasteryActions[index]?.Action;
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

        SetHoveredTile(tile, isValid);

        if (Input.GetKeyDown(ACCEPT_KEY) && isValid)
        {
            primaryTarget = tile;

            if (selectedAction.AreaOfEffect != AOE.Single)
            {
                TransitionToAOEConfirm();
            }
            else
            { 
                ConfirmAction(); 
            }
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
        battleUI.ShowActionCategoryMenu(activeCreature, hasMovedThisTurn);
        battleUI.ShowMessage("Choose which kind of action to take");
    }

    private void TransitionToCoreActionSelect()
    {
        currentState = PlayerTurnState.CoreActionSelect;
        battleUI.ShowActionMenu(activeCreature.Actions.EquippedCoreActions);
        UpdateActionDetails();
    }

    private void TransitionToEmpoweredActionSelect()
    {
        currentState = PlayerTurnState.EmpoweredActionSelect;
        battleUI.ShowActionMenu(activeCreature.Actions.EquippedEmpoweredActions);
        UpdateActionDetails();
    }

    private void TransitionToMasteryActionSelect()
    {
        currentState = PlayerTurnState.MasteryActionSelect;
        battleUI.ShowActionMenu(activeCreature.Actions.EquippedMasteryActions);
        UpdateActionDetails();
    }

    private void TransitionToMovementSelect()
    {
        currentState = PlayerTurnState.MovementSelect;

        var creaturePos = battlefield.GetBattlePosition(activeCreature);
        cursorRow = creaturePos.Row;
        cursorCol = creaturePos.GlobalCol;

        activeHighlightType = HighlightType.ValidMove;

        // Player grid only (cols 0-2)
        cursorColMin = 0;
        cursorColMax = GRID_COLS - 1;

        validTargets = battleManager.GetMoveTargets(activeCreature);
        HighlightTiles(validTargets, HighlightType.ValidMove);
        battleUI.ShowMovementMenu();
    }

    private void TransitionToExamine()
    {
        currentState = PlayerTurnState.Examine;

        activeHighlightType = HighlightType.None;

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
        currentState = PlayerTurnState.TargetSelect;

        if (selectedAction.Role == ActionRole.Offensive)
        {
            cursorColMin = ENEMY_COL;           // enemy grid (cols 3-5)
            cursorColMax = BATTLE_COLS - 1;
            activeHighlightType = HighlightType.OffensiveTarget;
        }
        else
        {
            cursorColMin = 0;                   // player grid (cols 0-2)
            cursorColMax = GRID_COLS - 1;
            activeHighlightType = HighlightType.SupportTarget;
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
        currentState = PlayerTurnState.AOEConfirm;

        var grid = battlefield.GetGrid(activeCreature.TeamSide);
        var aoeResult = AOETargetCalculator.GetTargets(primaryTarget, selectedAction.AreaOfEffect, grid, activeCreature.TeamSide);

        HighlightTargetList(aoeResult);

        battleUI.ShowMessage("Confirm area of effect");
    }

    #endregion

    #region Actions

    private void ExecuteMovement(BattleTile targetTile)
    {
        battleManager.ClearActiveCreatureHighlight();
        previousMovePosition = battlefield.GetBattlePosition(activeCreature);
        battlefield.SwapCreatures(activeCreature.CurrentTile, targetTile);
        hasMovedThisTurn = true;
        battleManager.SetActiveCreatureHighlight();

        PushState();
        ResetSelection();
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
        int index = battleUI.CurrentMenuIndex;
        stateStack.Push(new StateSnapshot(currentState, index, cursorRow, cursorCol));
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
        battleUI.SetMenuSelection(snapshot.MenuIndex);
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
                TransitionToCoreActionSelect();
                break;
            case PlayerTurnState.EmpoweredActionSelect:
                TransitionToEmpoweredActionSelect();
                break;
            case PlayerTurnState.MasteryActionSelect:
                TransitionToMasteryActionSelect();
                break;
            case PlayerTurnState.TargetSelect:
                if (selectedAction.Role == ActionRole.Offensive)
                {
                    cursorColMin = ENEMY_COL;
                    cursorColMax = BATTLE_COLS - 1;
                    activeHighlightType = HighlightType.OffensiveTarget;
                }
                else
                {
                    cursorColMin = 0;
                    cursorColMax = GRID_COLS - 1;
                    activeHighlightType = HighlightType.SupportTarget;
                }
                validTargets = selectedAction.GetValidTargets(activeCreature, battlefield);
                HighlightTiles(validTargets, activeHighlightType);
                battleUI.ShowTargetSelectMenu();
                break;
            case PlayerTurnState.MovementSelect:
                if (hasMovedThisTurn && previousMovePosition != null)
                {
                    battleManager.ClearActiveCreatureHighlight();
                    battlefield.SwapCreatures(activeCreature.CurrentTile, battlefield.GetTile(previousMovePosition.Value));
                    hasMovedThisTurn = false;
                    previousMovePosition = null;
                    battleManager.SetActiveCreatureHighlight();
                }
                currentState = PlayerTurnState.MovementSelect;
                cursorColMin = 0;
                cursorColMax = GRID_COLS - 1;
                activeHighlightType = HighlightType.ValidMove;
                validTargets = battleManager.GetMoveTargets(activeCreature);
                HighlightTiles(validTargets, HighlightType.ValidMove);
                battleUI.ShowMovementMenu();
                break;
        }
    }

    #endregion

    #region Helper Methods

    private BattlePosition GetCursorBattlePosition() => new BattlePosition(cursorRow, cursorCol);

    private bool IsBackButtonSelected() => battleUI.IsBackButtonSelected();

    private BattlePosition? GetFirstValidTarget()
    {
        if (validTargets.Count > 0)
            return validTargets[0].BattlefieldPosition;
        return null;
    }

    private void ResetSelection()
    {
        battleUI.ResetMenuSelection();
    }

    private void ResetValidTargets()
    {
        validTargets.Clear();
        battlefield.ClearAllHighlights();
        ClearHoveredTile();
    }

    private void HighlightTiles(List<BattleTile> tiles, HighlightType highlightType)
    {
        battlefield.HighlightTiles(tiles, highlightType);
    }

    private void HighlightTargetList(TargetList targets)
    {
        var primaryType = activeHighlightType switch
        {
            HighlightType.OffensiveTarget => HighlightType.SelectedOffensiveTarget,
            HighlightType.SupportTarget => HighlightType.SelectedSupportTarget,
            _ => HighlightType.None
        };

        var secondaryType = activeHighlightType switch
        {
            HighlightType.OffensiveTarget => HighlightType.OffensiveSplashSecondary,
            HighlightType.SupportTarget => HighlightType.SupportSplashSecondary,
            _ => HighlightType.None
        };

        var tertiaryType = activeHighlightType switch
        {
            HighlightType.OffensiveTarget => HighlightType.OffensiveSplashTertiary,
            HighlightType.SupportTarget => HighlightType.SupportSplashTertiary,
            _ => HighlightType.None
        };

        if (targets.PrimaryTarget != null)
            battlefield.GetTileUI(targets.PrimaryTarget.BattlefieldPosition)
                ?.SetPersistentHighlight(primaryType);

        foreach (var tile in targets.SecondaryTargets)
            battlefield.GetTileUI(tile.BattlefieldPosition)
                ?.SetPersistentHighlight(secondaryType);

        foreach (var tile in targets.TertiaryTargets)
            battlefield.GetTileUI(tile.BattlefieldPosition)
                ?.SetPersistentHighlight(tertiaryType);
    }

    private void SetHoveredTile(BattleTile newTile, bool isValid)
    {
        // No change — do nothing
        if (newTile == hoveredTile) return;

        // Clear previous hover
        if (hoveredTile != null)
        {
            var oldUI = battlefield.GetTileUI(hoveredTile.BattlefieldPosition);
            oldUI?.SetArrow(SelectionArrowState.Hidden);
            oldUI?.SetHighlight(HighlightType.None);
        }

        hoveredTile = newTile;

        if (hoveredTile == null) return;

        var newUI = battlefield.GetTileUI(hoveredTile.BattlefieldPosition);
        newUI?.SetArrow(isValid ? SelectionArrowState.HoveredValid : SelectionArrowState.HoveredInvalid);

        if (isValid)
            newUI?.SetHighlight(GetHoveredHighlightType());

        if (currentState == PlayerTurnState.Examine)
            battleUI.BindExamineCreature(hoveredTile.IsOccupied ? hoveredTile.OccupyingCreature : null);

        if (currentState == PlayerTurnState.TargetSelect)
        { 
            battleUI.UpdateActionDetails(
                selectedAction,
                activeCreature,
                newTile != null && newTile.IsOccupied ? newTile.OccupyingCreature : null);
        }
    }

    private HighlightType GetHoveredHighlightType() => activeHighlightType switch
    {
        HighlightType.OffensiveTarget => HighlightType.HoveredOffensiveTarget,
        HighlightType.SupportTarget => HighlightType.HoveredSupportTarget,
        HighlightType.ValidMove => HighlightType.HoveredMove,
        _ => HighlightType.None
    };

    private void ClearHoveredTile()
    {
        SetHoveredTile(null, false);
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