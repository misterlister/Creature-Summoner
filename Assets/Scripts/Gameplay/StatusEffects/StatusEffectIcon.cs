using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Statuses;

public class StatusEffectIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI turnText;

    public void Setup(StatusEffect status, StatusDefinitionLibrary library)
    {
        iconImage.sprite = library.GetIcon(status.Type, status.Intensity);
        turnText.text = status.GetDisplayValue().ToString();
    }
}