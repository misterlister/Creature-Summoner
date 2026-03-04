using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ElementDefinitionLibrary", menuName = "Game/Create Element Definition Library")]
public class ElementDefinitionLibrary : ScriptableObject
{
    [System.Serializable]
    public class ElementDefinition
    {
        public CreatureElement Element;
        public Sprite Icon;
    }

    [SerializeField] private ElementDefinition[] definitions;
    [SerializeField] private Sprite fallbackIcon;
    private Dictionary<CreatureElement, ElementDefinition> lookup;

    private void OnEnable()
    {
        lookup = new();
        foreach (var def in definitions)
            if (def != null) lookup[def.Element] = def;
    }

    public Sprite GetIcon(CreatureElement element)
    {
        if (lookup.TryGetValue(element, out var def) && def.Icon != null)
            return def.Icon;

        if (fallbackIcon != null) return fallbackIcon;

        Debug.LogWarning($"ElementDefinitionLibrary: No icon found for {element} and no fallback set.");
        return null;
    }
}