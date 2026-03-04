using UnityEngine;

public class EncounterZoneTrigger : MonoBehaviour
{
    [Tooltip("Must match a ZoneName in the parent MapArea's Encounter Zones list")]
    public string ZoneName;

    private MapArea parentArea;

    private void Awake()
    {
        // Walk up the hierarchy to find the MapArea
        parentArea = GetComponentInParent<MapArea>();

        if (parentArea == null)
            Debug.LogError($"EncounterZoneTrigger '{name}' has no MapArea in its parent hierarchy");
    }

    public MapArea GetParentArea() => parentArea;
    public string GetZoneName() => ZoneName;
}