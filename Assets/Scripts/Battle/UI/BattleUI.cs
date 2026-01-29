using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages all battle UI elements including menus, dialogs, and action displays.
/// Handles visual presentation only - no game logic.
/// </summary>
public class BattleUI : MonoBehaviour
{
    [Header("Text Speed")]
    [SerializeField] private int lettersPerSecond = 30;
    [SerializeField] private float textDelayAfterTyping = 0.8f;

    [Header("Colors")]
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color disabledTextColor = Color.gray;

    [Header("Dialog System")]
    [SerializeField] private TextMeshProUGUI battleLogText;

    [Header("Action Category Menu")]
    [SerializeField] private GameObject actionCategoryPanel;
    [SerializeField] private List<TextMeshProUGUI> actionCategoryTexts;

    [Header("Action Selection Menu")]
    [SerializeField] private GameObject actionSelectPanel;
    [SerializeField] private List<TextMeshProUGUI> actionTexts;

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

    [Header("Battle End Screen")]
    [SerializeField] private GameObject battleEndPanel;
    [SerializeField] private TextMeshProUGUI battleResultText;

    // Constants
    private const string BACK_TEXT = "Return to the previous menu";
    private const string NULL_ACTION_TEXT = "No available action";

    // State
    private Coroutine typingCoroutine;

    public List<TextMeshProUGUI> ActionTexts => actionTexts;
    public List<TextMeshProUGUI> ActionCategoryTexts => actionCategoryTexts;

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

    public void ShowActionCategoryMenu()
    {
        HideAllMenus();
        actionCategoryPanel.SetActive(true);
        EnableDialogText(true);
    }

    public void UpdateActionCategorySelection(int selectedIndex)
    {
        for (int i = 0; i < actionCategoryTexts.Count; i++)
        {
            actionCategoryTexts[i].color = (i == selectedIndex) ? highlightColor : normalTextColor;
        }
    }

    #endregion

    #region Action Selection Menus

    public void ShowCoreActionMenu(IReadOnlyList<CreatureAction> actions)
    {
        ShowActionMenu(actions);
    }

    public void ShowEmpoweredActionMenu(IReadOnlyList<CreatureAction> actions)
    {
        ShowActionMenu(actions);
    }

    public void ShowMasteryActionMenu(IReadOnlyList<CreatureAction> actions)
    {
        ShowActionMenu(actions);
    }

    private void ShowActionMenu(IReadOnlyList<CreatureAction> actions)
    {
        HideAllMenus();
        actionSelectPanel.SetActive(true);
        EnableActionOptions();
        SetActionNames(actions);
    }

    private void SetActionNames(IReadOnlyList<CreatureAction> actions)
    {
        // Set action names, leaving last slot for "Back"
        for (int i = 0; i < actionTexts.Count - 1; i++)
        {
            if (i >= actions.Count || actions[i] == null || actions[i].Action == null)
            {
                actionTexts[i].text = "-";
                actionTexts[i].color = disabledTextColor;
            }
            else
            {
                actionTexts[i].text = actions[i].Action.ActionName;
                actionTexts[i].color = normalTextColor;
            }
        }

        // Last slot is always "Back"
        actionTexts[actionTexts.Count - 1].text = "Back";
        actionTexts[actionTexts.Count - 1].color = normalTextColor;
    }

    public void UpdateActionSelection(int selectedIndex, ActionBase action, Creature attacker)
    {
        // Highlight selected action
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selectedIndex)
            {
                actionTexts[i].color = highlightColor;
            }
            else if (actionTexts[i].text == "-")
            {
                actionTexts[i].color = disabledTextColor;
            }
            else
            {
                actionTexts[i].color = normalTextColor;
            }
        }

        // Show back message or action details
        if (selectedIndex == actionTexts.Count - 1)
        {
            HighlightBackOption();
        }
        else
        {
            UpdateActionDetails(action, attacker);
        }
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
        actionSelectPanel.SetActive(true);
        DisableActionOptions(0, actionTexts.Count - 1); // Show only "Back" button
        ShowMessage("Select a space to move to");
    }

    public void ShowExamineMenu()
    {
        HideAllMenus();
        actionSelectPanel.SetActive(true);
        DisableActionOptions(0, actionTexts.Count - 1); // Show only "Back" button
        ShowMessage("Select a creature to examine");
    }

    public void ShowTargetSelectMenu()
    {
        HideAllMenus();
        actionSelectPanel.SetActive(true);
        DisableActionOptions(0, actionTexts.Count - 1); // Show only "Back" button
        actionDetailsPanel.SetActive(true);
    }

    public void ShowAOESelectMenu()
    {
        HideAllMenus();
        actionSelectPanel.SetActive(true);
        DisableActionOptions(0, actionTexts.Count - 1); // Show only "Back" button
        ShowMessage("Choose AOE positioning");
    }

    public void HighlightBackButton()
    {
        if (actionTexts.Count > 0)
        {
            actionTexts[actionTexts.Count - 1].color = highlightColor;
        }
    }

    public void ResetBackButton()
    {
        if (actionTexts.Count > 0)
        {
            actionTexts[actionTexts.Count - 1].color = normalTextColor;
        }
    }

    private void HighlightBackOption()
    {
        HighlightBackButton();
        ShowMessage(BACK_TEXT);
        actionDetailsPanel.SetActive(false);
    }

    public void ResetActionSelection()
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (actionTexts[i].text != "-")
            {
                actionTexts[i].color = normalTextColor;
            }
            else
            {
                actionTexts[i].color = disabledTextColor;
            }
        }
    }

    public void DisableActionOptions(int start, int count)
    {
        if (start < 0 || start >= actionTexts.Count) return;

        int end = Mathf.Min(start + count, actionTexts.Count);
        for (int i = start; i < end; i++)
        {
            actionTexts[i].gameObject.SetActive(false);
        }
    }

    public void EnableActionOptions()
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            actionTexts[i].gameObject.SetActive(true);
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

    private void HideAllMenus()
    {
        if (actionCategoryPanel != null) actionCategoryPanel.SetActive(false);
        if (actionSelectPanel != null) actionSelectPanel.SetActive(false);
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