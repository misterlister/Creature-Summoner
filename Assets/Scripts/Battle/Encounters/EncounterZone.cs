using UnityEngine;

public class EncounterZone : MonoBehaviour
{
    public RandomEncounterRules encounterRules;
    private MapArea parentArea;

    private void Awake()
    {
        // Walk up the hierarchy to find the MapArea
        parentArea = GetComponentInParent<MapArea>();

        if (parentArea == null)
            Debug.LogError($"EncounterZone '{name}' has no MapArea in its parent hierarchy");
    }

    public MapArea GetParentArea() => parentArea;
    public RandomEncounterRules GetEncounterRules() => encounterRules;
}