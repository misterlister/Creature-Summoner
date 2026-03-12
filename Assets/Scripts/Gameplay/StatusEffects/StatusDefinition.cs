using UnityEngine;
using Game.Statuses;

[CreateAssetMenu(fileName = "StatusDefinition", menuName = "Status/Create Status Definition")]
public class StatusDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private StatusType type;
    [SerializeField] private string displayName;

    [Header("Icons")]
    [Tooltip("Index 0 = base icon, index 1+ = intensity levels")]
    [SerializeField] private Sprite[] icons;

    [Header("Description")]
    [TextArea]
    [SerializeField] private string description;

    public Sprite GetIcon(int intensity)
    {
        if (icons == null || icons.Length == 0) return null;
        int index = Mathf.Clamp(intensity - 1, 0, icons.Length - 1);
        return icons[index];
    }

    public string Description => description;
    public string DisplayName => displayName;
    public StatusType Type => type;
}