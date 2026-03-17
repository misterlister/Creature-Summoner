using Game.Statuses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages all battle UI elements including menus, dialogs, and action displays.
/// </summary>
public class BattleUI : MonoBehaviour
{
    [Header("Text Speed")]
    [SerializeField] private int lettersPerSecond = 30;
    [SerializeField] private float textDelayAfterTyping = 0.8f;

    [Header("Menu")]
    [SerializeField] private MenuPanel menuPanel;

    [Header("Dialog System")]
    [SerializeField] private TextMeshProUGUI battleLogText;

    [Header("Action Details Panel")]
    [SerializeField] private GameObject actionDetailsPanel;
    [SerializeField] private TextMeshProUGUI actionElementText;
    [SerializeField] private TextMeshProUGUI actionSourceText;
    [SerializeField] private TextMeshProUGUI actionRangeText;
    [SerializeField] private TextMeshProUGUI actionPowerText;
    [SerializeField] private TextMeshProUGUI adjustedPowerText;
    [SerializeField] private TextMeshProUGUI actionAccuracyText;
    [SerializeField] private TextMeshProUGUI actionEnergyText;
    [SerializeField] private TextMeshProUGUI actionAOEText;
    [SerializeField] private TextMeshProUGUI actionCritText;
    [SerializeField] private TextMeshProUGUI actionDescriptionText;

    [Header("StatPanels")]
    [SerializeField] private CreatureInfoPanel playerInfoPanel;
    [SerializeField] private CreatureInfoPanel enemyInfoPanel;

    [Header("Battle End Screen")]
    [SerializeField] private GameObject battleEndPanel;
    [SerializeField] private TextMeshProUGUI battleResultText;

    // Holds references to text descriptions to justify disabled menu options
    private Dictionary<int, string> disabledReasons = new();

    // Constants
    private const string BACK_TEXT = "Return to the previous menu";
    private const string NULL_ACTION_TEXT = "No available action";

    // State
    private Coroutine typingCoroutine;

    private void Awake()
    {
        HideAllMenus();
    }

    #region Dialog System

    /// <summary>
    /// Type a message into the dialog box with typewriter effect.
    /// </summary>
    public IEnumerator TypeMessage(string message)
    {
        EnableDialogText(true);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(message));
        yield return typingCoroutine;
        typingCoroutine = null;
    }

    private IEnumerator TypeText(string message)
    {
        battleLogText.text = "";
        foreach (char letter in message)
        {
            battleLogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        yield return new WaitForSeconds(textDelayAfterTyping);
    }

    /// <summary>
    /// Show a message instantly without typing effect.
    /// </summary>
    public void ShowMessage(string message)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        battleLogText.text = message;
        EnableDialogText(true);
    }

    /// <summary>
    /// Display multiple messages from an action result.
    /// </summary>
    public void DisplayActionMessages(List<string> messages)
    {
        StartCoroutine(DisplayMessagesSequence(messages));
    }

    private IEnumerator DisplayMessagesSequence(List<string> messages)
    {
        foreach (var message in messages)
        {
            yield return TypeMessage(message);
        }
    }

    public void EnableDialogText(bool enabled)
    {
        battleLogText.gameObject.SetActive(enabled);
    }

    #endregion

    #region Action Category Menu

    public void ShowActionCategoryMenu(Creature creature, bool hasMovedThisTurn)
    {
        HideAllMenus();
        EnableDialogText(true);

        disabledReasons.Clear();
        var disabled = new HashSet<int>();

        CheckCategoryDisabled(creature, hasMovedThisTurn, disabled);

        var labels = Enum.GetValues(typeof(ActionCategoryMenuOptions))
            .Cast<ActionCategoryMenuOptions>()
            .Select(o => o.ToDisplayName())
            .ToList();

        menuPanel.Build(labels, disabled);
    }

    private void CheckCategoryDisabled(Creature creature, bool hasMovedThisTurn, HashSet<int> disabled)
    {
        int moveIndex = (int)ActionCategoryMenuOptions.Move;
        if (hasMovedThisTurn)
        {
            disabled.Add(moveIndex);
            disabledReasons[moveIndex] = "Already moved this turn";
        }
        else if (creature.HasStatusEffect(StatusType.Rooted))
        {
            disabled.Add(moveIndex);
            disabledReasons[moveIndex] = "This creature is rooted";
        }

        int empoweredIndex = (int)ActionCategoryMenuOptions.EmpoweredActions;
        if (creature.HasStatusEffect(StatusType.Stunned)) //////////////// IMPLEMENT BETTER LATER ////////////////
        {
            disabled.Add(empoweredIndex);
            disabledReasons[empoweredIndex] = "This creature is stunned";
        }
        else if (!creature.HasEmpoweredActions)
        {
            disabled.Add(empoweredIndex);
            disabledReasons[empoweredIndex] = "No empowered actions known";
        }

        int masteryIndex = (int)ActionCategoryMenuOptions.MasteryActions;
        if (creature.HasStatusEffect(StatusType.Stunned)) //////////////// IMPLEMENT BETTER LATER ////////////////
        {
            disabled.Add(masteryIndex);
            disabledReasons[masteryIndex] = "This creature is stunned";
        }
        else if (!creature.HasMasteryActions)
        {
            disabled.Add(masteryIndex);
            disabledReasons[masteryIndex] = "No mastery actions known";
        }
    }

    #endregion

    #region Action Selection Menus

    public void ShowActionMenu(IReadOnlyList<CreatureAction> actions)
    {
        HideAllMenus();

        var labels = actions
            .Select(a => a?.Action?.ActionName ?? "-")
            .Append("Back")
            .ToList();

        var disabled = new HashSet<int>();
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i]?.Action == null)
                disabled.Add(i);
        }

        menuPanel.Build(labels, disabled);
    }

    public void UpdateActionDetails(ActionBase action, Creature attacker = null, Creature defender = null)
    {
        if (action == null)
        {
            ShowMessage(NULL_ACTION_TEXT);
            actionDetailsPanel.SetActive(false);
        }
        else
        {
            EnableDialogText(false);
            actionDetailsPanel.SetActive(true);

            // Element and source
            actionElementText.text = $"Element: {action.Element}";
            actionSourceText.text = $"{action.Source}";
            actionRangeText.text = action.Range.ToString();

            // Power (base and adjusted)
            actionPowerText.text = $"Power: {action.Power}";

            if (attacker != null)
            {
                float powerRatio = 1f;
                if (action.Source == ActionSource.Physical)
                {
                    powerRatio = attacker.Compare_stat_to_average(StatType.Strength);
                }
                else if (action.Source == ActionSource.Magical)
                {
                    powerRatio = attacker.Compare_stat_to_average(StatType.Magic);
                }
                int adjPower = (int)(powerRatio * action.Power);
                adjustedPowerText.text = $"Adjusted: {adjPower}";
            }
            else
            {
                adjustedPowerText.text = "";
            }

            // Accuracy
            if (attacker != null && defender != null)
            {
                int accuracy = CombatCalculator.CalculateAccuracy(action, attacker, defender);
                actionAccuracyText.text = $"Accuracy: {accuracy}%";
            }
            else
            {
                actionAccuracyText.text = $"Accuracy: {action.Accuracy}%";
            }

            // Energy cost/gain
            if (action.SlotType == ActionSlotType.Core)
            {
                actionEnergyText.text = $"Energy Gain: {action.EnergyValue}";
            }
            else if (action.SlotType == ActionSlotType.Empowered)
            {
                actionEnergyText.text = $"Energy Cost: {action.EnergyValue}";
            }
            else if (action.SlotType == ActionSlotType.Mastery)
            {
                actionEnergyText.text = $"Mastery Cost: {action.EnergyValue}";
            }
            else
            {
                actionEnergyText.text = "";
            }

            // AOE
            actionAOEText.text = $"AOE: {action.AreaOfEffect}";

            // Crit chance
            if (attacker != null && defender != null)
            {
                int critChance = CombatCalculator.CalculateCritChance(action, attacker, defender);
                actionCritText.text = $"Crit Chance: {critChance}%";
            }
            else
            {
                actionCritText.text = $"Crit Chance: {action.BaseCrit}%";
            }

            // Description
            actionDescriptionText.text = action.Description;
        }
    }

    #endregion

    #region Movement/Examine Menus

    public void ShowMovementMenu()
    {
        HideAllMenus();
        menuPanel.Build(new List<string> { "Back" });
        ShowMessage("Select a space to move to");
    }

    public void ShowExamineMenu()
    {
        HideAllMenus();
        menuPanel.Build(new List<string> { "Back" });
        ShowMessage("Select a creature to examine");
    }

    public void HideExaminePanels()
    {
        playerInfoPanel.SetActive(false);
        enemyInfoPanel.SetActive(false);
    }

    public void ShowTargetSelectMenu()
    {
        HideAllMenus();
        menuPanel.Build(new List<string> { "Back" });
        actionDetailsPanel.SetActive(true);
        ShowMessage("Choose target");
    }

    public void BindExamineCreature(Creature creature)
    {
        if (creature == null)
        {
            playerInfoPanel.SetActive(false);
            enemyInfoPanel.SetActive(false);
            return;
        }

        if (creature.TeamSide == TeamSide.Player)
        {
            enemyInfoPanel.SetActive(false);
            playerInfoPanel.SetActive(true);
            playerInfoPanel.Bind(creature);
        }
        else
        {
            playerInfoPanel.SetActive(false);
            enemyInfoPanel.SetActive(true);
            enemyInfoPanel.Bind(creature);
        }
    }

    #endregion

    #region Battle End

    public void ShowBattleEndScreen(TeamSide winner)
    {
        HideAllMenus();
        battleEndPanel.SetActive(true);

        if (winner == TeamSide.Player)
        {
            battleResultText.text = "Victory!";
            battleResultText.color = Color.green;
        }
        else
        {
            battleResultText.text = "Defeat...";
            battleResultText.color = Color.red;
        }
    }

    #endregion

    #region Helper Methods

    public int MenuPanelActiveCount => menuPanel.ActiveCount;
    public bool IsMenuDisabled(int index) => menuPanel.IsDisabled(index);

    public void UpdateMenuSelection(int index)
    {
        menuPanel.SetSelection(index);
    }

    public string GetDisabledReason(int index)
    {
        return disabledReasons.TryGetValue(index, out var reason) ? reason : "Not available";
    }

    private void HideAllMenus()
    {
        menuPanel.Clear();
        if (actionDetailsPanel != null) actionDetailsPanel.SetActive(false);
        if (battleEndPanel != null) battleEndPanel.SetActive(false);
    }

    public void SetTextSpeed(int lettersPerSec)
    {
        lettersPerSecond = lettersPerSec;
    }

    public void SetTextDelay(float delay)
    {
        textDelayAfterTyping = delay;
    }

    #endregion
}