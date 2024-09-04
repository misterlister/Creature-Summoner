using System.Collections;
using System.Collections.Generic;
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
    PlayerMoveSelect,
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

    const int BATTLE_ROWS = 3;
    const int BATTLE_COLS = 2;
    const float TEXT_DELAY = 0.8f;
    const KeyCode ACCEPT_KEY = KeyCode.Z;
    const KeyCode BACK_KEY = KeyCode.X;

    public Dictionary<BattleState, int> StateChoices { get; set; }

    public BattleField Field { get; set; }

    public BattleCreature activeCreature;

    public List<BattleCreature> CreatureTargets { get; set; }
    public List<BattleCreature> InitiativeOrder { get; set; }

    public IAction selectedAction;
    BattleState state;
    int selectionPosition;
    int round = 0;

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
        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            for (int col = 0; col < BATTLE_COLS; col++)
            {
                int pos = row * BATTLE_COLS + col;
                if (pos < friends.Length && friends[pos].Ignore == false)
                {
                    Field.AddCreature(friends[pos], row, col);
                }
            }
        }

        // Add enemy creatures
        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            for (int col = 0; col < BATTLE_COLS; col++)
            {
                int pos = row * BATTLE_COLS + col;
                if (pos < enemies.Length && enemies[pos].Ignore == false)
                {
                    Field.AddCreature(enemies[pos], row, col);
                }
            }
        }

        StateChoices = new Dictionary<BattleState, int>
        {
            { BattleState.Start, 0 },
            { BattleState.NewRound, 0 },
            { BattleState.DetermineTurn, 0 },
            { BattleState.PlayerActionCategorySelect, dialogBox.ActionCategoryText.Count },
            { BattleState.PlayerCoreActionSelect, dialogBox.ActionText.Count },
            { BattleState.PlayerEmpoweredActionSelect, dialogBox.ActionText.Count },
            { BattleState.PlayerMasteryActionSelect, 2 },
            { BattleState.PlayerMoveSelect, 2 },
            { BattleState.PlayerExamine, Field.FieldCreatures.Count + 1 },
            { BattleState.TargetSelect, Field.FieldCreatures.Count + 1 },
            { BattleState.EnemyAction, 0 },
            { BattleState.Busy, 0 }
        };

        int enemy_num = Field.GetTargets(false, false).Count;
        
        yield return dialogBox.TypeDialog($"You have been attacked by {enemy_num} creatures!");
        yield return new WaitForSeconds(TEXT_DELAY);

        StartRound();
    }

    void StartRound()
    {
        round++;
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
        InitiativeOrder.Sort((c1, c2) => c2.Initiative.CompareTo(c1.Initiative));

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
                // Set next creature's turn
                activeCreature = InitiativeOrder[0];
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
        selectionPosition = 0;
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
        StateChoices[BattleState.PlayerExamine] = Field.FieldCreatures.Count + 1;
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
        StateChoices[BattleState.TargetSelect] = Field.FieldCreatures.Count + 1;
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
        ToDetermineTurn();
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
        else if (state != BattleState.Start
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
        if (Input.GetKeyDown(ACCEPT_KEY))
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
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPosition == StateChoices[state] - 1))
        {
            selectionPosition = 0;
            ToPlayerActionCategorySelectState();
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
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
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPosition == StateChoices[state] - 1))
        {
            selectionPosition = 1;
            ToPlayerActionCategorySelectState();
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
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
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPosition == StateChoices[state] - 1))
        {
            selectionPosition = 2;
            dialogBox.EnableActionOptions();
            ToPlayerActionCategorySelectState();
        }
        else if (Input.GetKeyDown(ACCEPT_KEY))
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
        if (Input.GetKeyDown(BACK_KEY) || (Input.GetKeyDown(ACCEPT_KEY) && selectionPosition == StateChoices[state] - 1))
        {
            // Disable the currently open HUD panel before going back
            if (selectionPosition < StateChoices[state] - 1)
            {
                ResetTargets();
                Field.FieldCreatures[selectionPosition].Hud.EnableCreatureInfoPanel(false); 
            }
            selectionPosition = 4;
            dialogBox.EnableActionOptions();
            ToPlayerActionCategorySelectState();
        }
        else
        {
            ResetTargets();

            if (Field.FieldCreatures.Count > selectionPosition)
            {
                Field.FieldCreatures[selectionPosition].Hud.EnableCreatureInfoPanel(true);
                Field.FieldCreatures[selectionPosition].Hud.EnableSelectionArrow(true);
                CreatureTargets.Add(Field.FieldCreatures[selectionPosition]);
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

