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
    [SerializeField] GameObject actionTypeSelect;
    [SerializeField] GameObject actionSelect;
    [SerializeField] GameObject actionDetails;

    [SerializeField] List<TextMeshProUGUI> actionTypeText;
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
    }

    public void EnableDialogText(bool enabled)
    {
        battleLogText.enabled = enabled;
    }

    public void EnableActionTypeSelect(bool enabled)
    {
        actionTypeSelect.SetActive(enabled);
    }

    public void EnableActionSelect(bool enabled)
    {
        actionSelect.SetActive(enabled);
        actionDetails.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
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
    }

    public void UpdateActionTypeSelection(int selectedAction)
    {
        for (int i = 0; i < actionTypeText.Count; i++)
        {
            if (i == selectedAction)
            {
                actionTypeText[i].color = highlightColour;
            }
            else
            {
                actionTypeText[i].color = Color.black;
            }
        }
    }
}
