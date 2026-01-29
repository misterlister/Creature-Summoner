using System.Collections.Generic;
using UnityEngine;
using static GameConstants;

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
    private List<BattleTile> selectedTargets = new List<BattleTile>();
    private BattleTile primaryTarget;
    private int selectionYChoice = 0;

    // Current menu position
    private int selectionX = 0;
    private int selectionY = 0;

    // Movement tracking
    private bool hasMovedThisTurn = false;
    private BattlePosition? previousMovePosition = null;

    // State tracking - only player-specific substates
    private PlayerTurnState currentState;
    private Stack<PlayerTurnState> stateStack = new Stack<PlayerTurnState>();
    private Stack<(int y, int x)> positionStack = new Stack<(int y, int x)>();

    // Currently highlighted/selected creatures for UI
    private BattleTile hoveredTile;
    private List<BattleTile> validTargets = new List<BattleTile>();

    // State-specific menu dimensions
    private Dictionary<PlayerTurnState, (int rows, int cols)> stateChoices;

    // Input keys
    private const KeyCode ACCEPT_KEY = KeyCode.Z;
    private const KeyCode BACK_KEY = KeyCode.X;

    public PlayerTurnController(BattleManager manager, UnifiedBattlefield battlefield, BattleUI ui)
    {
        this.battleManager = manager;
        this.battlefield = battlefield;
        this.battleUI = ui;

        InitializeStateChoices();
    }

    private void InitializeStateChoices()
    {
        stateChoices = new Dictionary<PlayerTurnState, (int rows, int cols)>
        {
            { PlayerTurnState.ActionCategorySelect, (Mathf.CeilToInt(5f / BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { PlayerTurnState.CoreActionSelect, (Mathf.CeilToInt((float)CORE_SLOTS / BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { PlayerTurnState.EmpoweredActionSelect, (Mathf.CeilToInt((float)EMPOWERED_SLOTS / BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { PlayerTurnState.MasteryActionSelect, (Mathf.CeilToInt((float)MASTERY_SLOTS / BATTLE_MENU_COLS), BATTLE_MENU_COLS) },
            { PlayerTurnState.MovementSelect, (BATTLE_ROWS + 1, BATTLE_COLS / 2) },
            { PlayerTurnState.Examine, (BATTLE_ROWS + 1, BATTLE_COLS) },
            { PlayerTurnState.TargetSelect, (BATTLE_ROWS + 1, BATTLE_COLS) },
            { PlayerTurnState.AOESelect, (0, 0) } // Dynamic based on AOE type
        };
    }

    public void StartTurn(Creature creature)
    {
        activeCreature = creature;

        // Reset state
        selectedAction = null;
        selectedTargets.Clear();
        primaryTarget = null;
        selectionYChoice = 0;
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
            case PlayerTurnState.AOESelect:
                HandleAOESelectInput();
                break;
        }
    }

    private void HandleMenuNavigation()
    {
        if (!stateChoices.ContainsKey(currentState)) return;

        var (rows, cols) = stateChoices[currentState];

        if (Input.GetKeyDown(KeyCode.DownArrow) && rows > 0)
        {
            selectionY = (selectionY + 1) % rows;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && rows > 0)
        {
            selectionY = (selectionY - 1 + rows) % rows;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && cols > 0)
        {
            selectionX = (selectionX - 1 + cols) % cols;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && cols > 0)
        {
            selectionX = (selectionX + 1) % cols;
        }
    }

    #endregion

    #region State-Specific Input Handlers

    private void HandleActionCategoryInput()
    {
        if (Input.GetKeyDown(BACK_KEY) && hasMovedThisTurn)
        {
            // Can go back and undo movement
            TryPopState();
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
        {
            int selection = GetLinearPosition();

            switch (selection)
            {
                case 0: // Core Actions
                    TransitionToCoreActionSelect();
                    break;
                case 1: // Empowered Actions
                    TransitionToEmpoweredActionSelect();
                    break;
                case 2: // Mastery Actions
                    TransitionToMasteryActionSelect();
                    break;
                case 3: // Movement
                    if (hasMovedThisTurn)
                    {
                        battleUI.ShowMessage("Already moved this turn");
                    }
                    else
                    {
                        TransitionToMovementSelect();
                    }
                    break;
                case 4: // Examine
                    TransitionToExamine();
                    break;
            }
        }
        else
        {
            battleUI.UpdateActionCategorySelection(GetLinearPosition());
        }
    }

    private void HandleCoreActionInput()
    {
        if (Input.GetKeyDown(BACK_KEY))
        {
            TryPopState();
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
        {
            int selection = GetLinearPosition();

            if (selection < activeCreature.Actions.EquippedCoreActions.Count)
            {
                var action = activeCreature.Actions.EquippedCoreActions[selection]?.Action;
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
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
        {
            int selection = GetLinearPosition();

            if (selection < activeCreature.Actions.EquippedEmpoweredActions.Count)
            {
                var action = activeCreature.Actions.EquippedEmpoweredActions[selection]?.Action;
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
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
        {
            int selection = GetLinearPosition();

            if (selection < activeCreature.Actions.EquippedMasteryActions.Count)
            {
                var action = activeCreature.Actions.EquippedMasteryActions[selection]?.Action;
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
            battlefield.ClearAllHighlights();
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

            // Get valid targets for selected action
            validTargets = selectedAction.GetValidTargets(activeCreature, battlefield);
            HighlightType highlightType = selectedAction.Role == ActionRole.Offensive
                ? HighlightType.NegativeTarget
                : HighlightType.PositiveTarget;
            HighlightTiles(validTargets, highlightType);

            var cursorPos = GetCursorBattlePosition();
            var tile = battlefield.GetTile(cursorPos);

            bool isValid = validTargets.Contains(tile);

            if (Input.GetKeyDown(ACCEPT_KEY) && isValid)
            {
                primaryTarget = tile;

                if (selectedAction.AreaOfEffect != AOE.Single)
                {
                    TransitionToAOESelect();
                }
                else
                {
                    ConfirmAction();
                }
            }
            else
            {
                HoverTile(tile, isValid);

                // Update action details with target info
                if (tile != null && tile.IsOccupied)
                {
                    battleUI.UpdateActionDetails(selectedAction, activeCreature, tile.OccupyingCreature);
                }
                else
                {
                    battleUI.UpdateActionDetails(selectedAction, activeCreature);
                }
            }
        }
    }

    private void HandleAOESelectInput()
    {
        battlefield.ClearAllHighlights();

        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && IsBackButtonSelected()))
        {
            primaryTarget = null;
            TryPopState();
        }
        else if (IsBackButtonSelected())
        {
            battleUI.HighlightBackButton();
        }
        else
        {
            battleUI.ResetBackButton();

            // Preview AOE at current Y choice
            var aoeTargets = selectedAction.GetAOETargets(
                primaryTarget,
                battlefield,
                selectionY
            );

            HighlightType highlightType = selectedAction.Role == ActionRole.Offensive
                ? HighlightType.NegativeTarget
                : HighlightType.PositiveTarget;
            HighlightTiles(aoeTargets, highlightType);

            if (Input.GetKeyDown(ACCEPT_KEY))
            {
                selectionYChoice = selectionY;
                ConfirmAction();
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
        ResetSelection();
        battleUI.ShowCoreActionMenu(activeCreature.Actions.EquippedCoreActions);
    }

    private void TransitionToEmpoweredActionSelect()
    {
        PushState();
        currentState = PlayerTurnState.EmpoweredActionSelect;
        ResetSelection();
        battleUI.ShowEmpoweredActionMenu(activeCreature.Actions.EquippedEmpoweredActions);
    }

    private void TransitionToMasteryActionSelect()
    {
        PushState();
        currentState = PlayerTurnState.MasteryActionSelect;
        ResetSelection();
        battleUI.ShowMasteryActionMenu(activeCreature.Actions.EquippedMasteryActions);
    }

    private void TransitionToMovementSelect()
    {
        PushState();
        currentState = PlayerTurnState.MovementSelect;

        // Start cursor at active creature's position
        var creaturePos = battlefield.GetBattlePosition(activeCreature);
        selectionY = creaturePos.Row;
        selectionX = creaturePos.LocalCol;

        battleUI.ShowMovementMenu();
    }

    private void TransitionToExamine()
    {
        PushState();
        currentState = PlayerTurnState.Examine;

        // Start cursor at active creature's position
        var creaturePos = battlefield.GetBattlePosition(activeCreature);
        selectionY = creaturePos.Row;
        selectionX = creaturePos.GlobalCol;

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
            selectionY = firstTarget.Value.Row;
            selectionX = firstTarget.Value.GlobalCol;
        }

        battleUI.ShowTargetSelectMenu();
    }

    private void TransitionToAOESelect()
    {
        PushState();
        currentState = PlayerTurnState.AOESelect;
        ResetSelection();

        // Update state choices for this specific AOE
        UpdateAOEStateChoices(selectedAction.AreaOfEffect);

        battleUI.ShowAOESelectMenu();
    }

    #endregion

    #region Actions

    private void ExecuteMovement(BattleTile targetTile)
    {
        var sourcePos = battlefield.GetBattlePosition(activeCreature);
        var targetPos = battlefield.GetBattlePosition(targetTile);

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
        selectedTargets = selectedAction.GetAOETargets(primaryTarget, battlefield, selectionYChoice);

        battlefield.ClearAllHighlights();

        // Tell battle manager to execute
        battleManager.ExecutePlayerAction(selectedAction, selectedTargets, selectionYChoice);
    }

    #endregion

    #region State Stack Management

    private void PushState()
    {
        stateStack.Push(currentState);
        positionStack.Push((selectionY, selectionX));
    }

    private bool TryPopState()
    {
        if (stateStack.Count > 0)
        {
            currentState = stateStack.Pop();
            var (y, x) = positionStack.Pop();
            selectionY = y;
            selectionX = x;

            RestoreStateUI();
            return true;
        }
        return false;
    }

    private void ClearStateStack()
    {
        stateStack.Clear();
        positionStack.Clear();
    }

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

    private int GetLinearPosition()
    {
        if (!stateChoices.ContainsKey(currentState)) return 0;
        var (_, cols) = stateChoices[currentState];
        return selectionY * cols + selectionX;
    }

    private BattlePosition GetCursorBattlePosition()
    {
        return new BattlePosition(selectionY, selectionX);
    }

    private BattlePosition? GetFirstValidTarget()
    {
        var targets = selectedAction.GetValidTargets(activeCreature, battlefield);
        if (targets.Count > 0)
        {
            return battlefield.GetBattlePosition(targets[0]);
        }
        return null;
    }

    private bool IsBackButtonSelected()
    {
        if (!stateChoices.ContainsKey(currentState)) return false;
        var (rows, _) = stateChoices[currentState];
        return selectionY == rows - 1;
    }

    private void ResetSelection()
    {
        selectionX = 0;
        selectionY = 0;
    }

    private void ResetValidTargets()
    {
        validTargets.Clear();
        battlefield.ClearAllHighlights();
    }

    private void UpdateActionHighlight(IReadOnlyList<CreatureAction> actions)
    {
        int selection = GetLinearPosition();
        ActionBase action = null;

        if (selection >= 0 && selection < actions.Count)
        {
            action = actions[selection]?.Action;
        }

        battleUI.UpdateActionSelection(selection, action, activeCreature);
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
        var tileUI = battlefield.GetTileUI(battlefield.GetBattlePosition(tile));
        if (tileUI != null)
        {
            // Show selection arrow or other hover indicator
            if (showStatus)
            {
                tileUI.ShowStatusWindow();
            }
        }
    }

    private void ClearHoveredTile()
    {
        if (hoveredTile != null)
        {
            var tileUI = battlefield.GetTileUI(battlefield.GetBattlePosition(hoveredTile));
            tileUI?.HideStatusWindow();
            hoveredTile = null;
        }
    }

    private void UpdateAOEStateChoices(AOE aoeType)
    {
        // Get AOE options from game constants
        if (GameConstants.AOEOptions.TryGetValue(aoeType, out var options))
        {
            stateChoices[PlayerTurnState.AOESelect] = (options.y + 1, options.x);
        }
        else
        {
            stateChoices[PlayerTurnState.AOESelect] = (1, 1);
        }
    }

    #endregion
}