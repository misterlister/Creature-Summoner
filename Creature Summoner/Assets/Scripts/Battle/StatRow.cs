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

    public void UpdateType(CreatureType type1, CreatureType type2)
    {
        firstValue.text = type1.ToString();
        firstValue.color = GameConstants.TypeColours[type1];

        if (type1 == type2 || type2 == CreatureType.None)
        {
            secondValue.gameObject.SetActive(false);
        }
        else
        {
            secondValue.gameObject.SetActive(true);
            secondValue.text = type2.ToString();
            secondValue.color = GameConstants.TypeColours[type2];
        }
    }

    public void UpdateSpecies(string species)
    {
        singleValue.text = species;
        if (!singleValue)
        {
            Debug.LogWarning("Error. Updating species name on non-single value statRow");
        }
    }
}
