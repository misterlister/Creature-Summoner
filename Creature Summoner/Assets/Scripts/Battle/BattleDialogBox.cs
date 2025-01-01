using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int LettersPerSecond;
    [SerializeField] Color highlightColour;

    [SerializeField] TextMeshProUGUI battleLogText;
    [SerializeField] GameObject actionCategorySelect;
    [SerializeField] GameObject actionSelect;
    [SerializeField] GameObject actionDetails;

    [SerializeField] List<TextMeshProUGUI> actionCategoryText;
    [SerializeField] List<TextMeshProUGUI> actionText;

    [SerializeField] TextMeshProUGUI actionType;
    [SerializeField] TextMeshProUGUI actionSource;
    [SerializeField] TextMeshProUGUI actionRange;
    [SerializeField] TextMeshProUGUI actionPower;
    [SerializeField] TextMeshProUGUI actionAccuracy;
    [SerializeField] TextMeshProUGUI actionEnergy;
    [SerializeField] TextMeshProUGUI actionTargets;
    [SerializeField] TextMeshProUGUI actionPrep;
    [SerializeField] TextMeshProUGUI actionDescription;

    const string backText = "Return to the previous menu";
    const string nullText = "No available move";

    const float TEXT_DELAY = 0.8f; // Time delay after text is done printing

    public List<TextMeshProUGUI> ActionText => actionText;
    public List<TextMeshProUGUI> ActionCategoryText => actionCategoryText;

    private Coroutine typingCoroutine;

    public void SetDialog(string dialog)
    {
        battleLogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        battleLogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            battleLogText.text += letter;
            yield return new WaitForSeconds(1f/LettersPerSecond);
        }
        yield return new WaitForSeconds(TEXT_DELAY);
        typingCoroutine = null; // Reset coroutine reference when complete
    }

    public Coroutine StartTypingDialog(string dialog)
    {
        // Stop any active typing coroutine before starting another
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeDialog(dialog));
        
        return typingCoroutine;
    }

    public void EnableDialogText(bool enabled)
    {
        battleLogText.gameObject.SetActive(enabled);
        battleLogText.enabled = enabled;
    }

    public void EnableActionCategorySelect(bool enabled)
    {
        actionCategorySelect.SetActive(enabled);
    }

    public void EnableActionSelect(bool enabled)
    {
        actionSelect.SetActive(enabled);
    }

    public void EnableActionDetails(bool enabled)
    {
        actionDetails.SetActive(enabled);
    }

    public void ResetActionSelection()
    {
        for (int i = 0; i < actionText.Count; i++)
        {
            actionText[i].color = Color.black;
        }
    }

    public void HighlightBackOption()
    {
        actionText[actionText.Count-1].color = highlightColour;
        battleLogText.text = backText;
        actionDetails.SetActive(false);
        EnableDialogText(true);
    }

    public void UpdateActionSelection(int selectedAction, ActionBase action)
    {
        for (int i = 0; i < actionText.Count; i++)
        {
            if (i == selectedAction)
            {
                actionText[i].color = highlightColour;
            }
            else
            {
                actionText[i].color = Color.black;
            }
        }
        if (selectedAction == actionText.Count - 1)
        {
            HighlightBackOption();
        }
        else if (action == null)
        {
            battleLogText.text = nullText;
            actionDetails.SetActive(false);
            EnableDialogText(true);
        }
        else
        {
            EnableDialogText(false);
            actionDetails.SetActive(true);
            actionType.text = $"{action.Type}";
            actionSource.text = $"{action.Category}";
            actionRange.text = action.Range.ToString();
            actionPower.text = $"Power: {action.Power}";
            actionAccuracy.text = $"Accuracy: {action.Accuracy}%";
            if (action.Category == ActionCategory.Core)
            {
                actionEnergy.text = $"Energy Gain: {action.EnergyGain}";
            }
            else if (action.Category == ActionCategory.Empowered)
            {
                actionEnergy.text = $"Energy Cost: {action.EnergyCost}";
            }
            else if (action.Category == ActionCategory.Mastery)
            {
                actionEnergy.text = $"Mastery Cost: {action.EnergyCost}";
            }
            else
            {
                actionEnergy.text = "";
            }
            actionTargets.text = $"Targets: {action.AreaOfEffect.ToString()}";
            actionPrep.text = action.Preparation ? "Prepared Action" : "";
            actionDescription.text = $"{action.Description}";
        }
    }

    public void UpdateActionCategorySelection(int selectedAction)
    {
        for (int i = 0; i < actionCategoryText.Count; i++)
        {
            if (i == selectedAction)
            {
                actionCategoryText[i].color = highlightColour;
            }
            else
            {
                actionCategoryText[i].color = Color.black;
            }
        }
    }

    public void SetActionNames(CreatureAction[] actions)
    {
        for (int i = 0; i < actionText.Count - 1; i++)
        {
            if (i >= actions.Length || actions[i] == null)
            {
                actionText[i].text = "-";
            }
            else
            {
                actionText[i].text = actions[i].Action.TalentName;
            }
        }
    }

    public void DisableActionOptions(int start, int total)
    {   if (start < actionText.Count && start + total < actionText.Count)
        {
            for (int i = start; i < start + total; i++)
            {
                actionText[i].gameObject.SetActive(false);
            }
        }
    }
    
    public void EnableActionOptions()
    {
        for (int i = 0; i < actionText.Count; i++) {
            actionText[i].gameObject.SetActive(true);
        }
    }
}
