using TMPro;
using UnityEngine;

public class StatRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statName;
    [SerializeField] private TextMeshProUGUI singleValue;
    [SerializeField] private TextMeshProUGUI firstValue;
    [SerializeField] private TextMeshProUGUI secondValue;

    private bool singleEntry;

    public void SetupRow(string name, bool singleField)
    {
        statName.text = name;
        singleEntry = singleField;

        if (singleField)
        {
            singleValue.gameObject.SetActive(true);
            secondValue.gameObject.SetActive(false);
            firstValue.gameObject.SetActive(false);
        }
        else
        {
            singleValue.gameObject.SetActive(false);
            secondValue.gameObject.SetActive(true);
            firstValue.gameObject.SetActive(true);
        }

    }

    public void UpdateStats(int statValue, int modifiedValue)
    {

        firstValue.text = statValue.ToString();

        if (modifiedValue == statValue)
        {
            this.secondValue.gameObject.SetActive(false);
        }
        else
        {
            this.secondValue.gameObject.SetActive(true); // Show currentValue
            this.secondValue.text = modifiedValue.ToString();

            // Change the color of currentValue based on comparison
            if (modifiedValue < statValue)
            {
                this.secondValue.color = Color.red; // Stat decreased
            }
            else if (modifiedValue > statValue)
            {
                this.secondValue.color = Color.green; // Stat increased
            }
            else
            {
                this.secondValue.color = Color.black; // Stat unchanged
            }
        }
    }

    public void UpdateElement(CreatureElement element1, CreatureElement element2)
    {
        firstValue.text = element1.ToString();
        firstValue.color = GameConstants.ElementColours[element1];

        if (element1 == element2 || element2 == CreatureElement.None)
        {
            secondValue.gameObject.SetActive(false);
        }
        else
        {
            secondValue.gameObject.SetActive(true);
            secondValue.text = element2.ToString();
            secondValue.color = GameConstants.ElementColours[element2];
        }
    }

    public void UpdateSingleText(string text, Color colour = default)
    {
        if (colour == default)
        {
            colour = Color.black;
        }
        singleValue.text = text;
        singleValue.color = colour;
        if (!singleEntry)
        {
            Debug.LogWarning("Error: Updating single text on double value statRow");
        }
    }

    public void UpdateDoubleText(string text, string text2, Color colour = default)
    {
        if (colour == default)
        {
            colour = Color.black;
        }
        firstValue.text = text;
        firstValue.color = colour;
        secondValue.text = text2;
        secondValue.color = colour;

        if (singleEntry)
        {
            Debug.LogWarning($"Error: Updating double text on single value. Values: {text}, {text2}");
        }
    }

    public void UpdateResource(int max, int current, Color colour = default)
    {
        if (colour == default)
        {
            colour = Color.black;
        }
        singleValue.text = $"{current}/{max}";
        singleValue.color = colour;
        if (!singleEntry)
        {
            Debug.LogWarning("Error: Updating single text on double value statRow");
        }
    }
}
