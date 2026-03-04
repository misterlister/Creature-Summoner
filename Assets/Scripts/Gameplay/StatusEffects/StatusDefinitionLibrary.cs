using System.Collections.Generic;
using UnityEngine;
using Game.Statuses;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "StatusDefinitionLibrary", menuName = "Status/Create Status Definition Library")]
public class StatusDefinitionLibrary : ScriptableObject
{
    [SerializeField] private StatusDefinition[] definitions;
    [SerializeField] private Sprite fallbackIcon;

    private Dictionary<StatusType, StatusDefinition> lookup;

    private void OnEnable()
    {
        lookup = new();
        foreach (var def in definitions)
        {
            if (def != null) lookup[def.Type] = def;
        }
    }

    public Sprite GetIcon(StatusType type, int intensity = 1)
    {
        if (lookup.TryGetValue(type, out var def))
        {
            Sprite icon = def.GetIcon(intensity);
            if (icon != null) return icon;
        }

        if (fallbackIcon != null) return fallbackIcon;

        Debug.LogWarning($"StatusDefinitionLibrary: No icon found for {type} at intensity {intensity} and no fallback set.");
        return null;
    }

    [ContextMenu("Auto-populate from Project")]
    private void AutoPopulate()
    {
    #if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:StatusDefinition", new[] { "Assets/Game/Resources/Libraries/Statuses/Definitions" });
        definitions = new StatusDefinition[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            definitions[i] = AssetDatabase.LoadAssetAtPath<StatusDefinition>(path);
        }
        EditorUtility.SetDirty(this);
        Debug.Log($"StatusDefinitionLibrary: Auto-populated {definitions.Length} definitions.");
    #endif
    }
}