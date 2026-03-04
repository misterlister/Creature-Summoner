using UnityEngine;
using Game.Statuses;

[CreateAssetMenu(fileName = "StatusDefinition", menuName = "Status/Create Status Definition")]
public class StatusDefinition : ScriptableObject
{
    [Header("Identity")]
    public StatusType Type;
    public string DisplayName;

    [Header("Icons")]
    [Tooltip("Index 0 = base icon, index 1+ = intensity levels")]
    public Sprite[] Icons;

    public Sprite GetIcon(int intensity)
    {
        if (Icons == null || Icons.Length == 0) return null;
        int index = Mathf.Clamp(intensity - 1, 0, Icons.Length - 1);
        return Icons[index];
    }
}