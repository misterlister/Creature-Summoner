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

    public List<TextMeshProUGUI> ActionText => actionText;

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

    public void EnableActionCategorySelect(bool enabled)
    {
        actionCategorySelect.SetActive(enabled);
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

    public void SetCoreActionNames(Creature creature)
    {
        if (creature.PhysicalCore == null)
        {
            actionText[0].text = "-";
        }
        else
        {
            actionText[0].text = creature.PhysicalCore.Action.TalentName;
        }
        if (creature.MagicalCore == null)
        {
            actionText[1].text = "-";
        }
        else
        {
            actionText[1].text = creature.MagicalCore.Action.TalentName;
        }
        if (creature.DefensiveCore == null)
        {
            actionText[2].text = "-";
        }
        else
        {
            actionText[2].text = creature.DefensiveCore.Action.TalentName;
        }
    }

    public void SetEmpoweredActionNames(Creature creature)
    {
        for (int i = 0; i < actionText.Count; i++)
        {
            if (i < creature.EquippedEmpoweredActions.Count)
            {
                actionText[i].text = creature.EquippedEmpoweredActions[i].Action.TalentName;
            } 
            else
            {
                actionText[i].text = "-";
            }
        }
    }
}