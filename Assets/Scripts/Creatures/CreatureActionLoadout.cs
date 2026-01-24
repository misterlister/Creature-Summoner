using UnityEngine;
using System.Linq;
using Game.Creatures.Managers;

[CreateAssetMenu(fileName = "CreatureActionLoadout", menuName = "Creatures/Create new Creature Action Loadout")]
public class CreatureActionLoadout : ScriptableObject
{
    [System.Serializable]
    public class SlotAssignment
    {
        [Tooltip("The action to equip in this slot")]
        public ActionBase Action;

        [Tooltip("Which slot index to equip it in")]
        [Range(0, 2)]
        public int SlotIndex;
    }

    [Header("Core Actions (3 slots)")]
    [Tooltip("Slot 0: Physical Offensive/Support")]
    public ActionBase CoreSlot0;

    [Tooltip("Slot 1: Magical Offensive/Support")]
    public ActionBase CoreSlot1;

    [Tooltip("Slot 2: Defensive (any source)")]
    public ActionBase CoreSlot2;

    [Header("Empowered Actions (3 slots)")]
    public ActionBase EmpoweredSlot0;
    public ActionBase EmpoweredSlot1;
    public ActionBase EmpoweredSlot2;

    [Header("Mastery Action (1 slot)")]
    public ActionBase MasterySlot0;

    /// <summary>
    /// Apply this loadout to a creature's action manager
    /// Returns the number of slots that failed to equip
    /// </summary>
    public int ApplyToCreature(Creature creature)
    {
        var actions = creature.Actions;
        int failedCount = 0;

        // Equip Core actions
        if (!TryEquipIfNotNull(actions, CoreSlot0, 0)) failedCount++;
        if (!TryEquipIfNotNull(actions, CoreSlot1, 1)) failedCount++;
        if (!TryEquipIfNotNull(actions, CoreSlot2, 2)) failedCount++;

        // Equip Empowered actions
        if (!TryEquipIfNotNull(actions, EmpoweredSlot0, 0)) failedCount++;
        if (!TryEquipIfNotNull(actions, EmpoweredSlot1, 1)) failedCount++;
        if (!TryEquipIfNotNull(actions, EmpoweredSlot2, 2)) failedCount++;

        // Equip Mastery action
        if (!TryEquipIfNotNull(actions, MasterySlot0, 0)) failedCount++;

        return failedCount;
    }

    private bool TryEquipIfNotNull(ActionManager actionManager, ActionBase actionBase, int slotIndex)
    {
        // Null/empty slots are not failures - they're intentional
        if (actionBase == null) return true;

        // Find the action in known actions
        var action = actionManager.AllKnownActions
            .FirstOrDefault(a => a.Action == actionBase);

        if (action != null)
        {
            return actionManager.TryEquipAction(action, slotIndex);
        }
        else
        {
            Debug.LogWarning($"Loadout '{name}' tried to equip unknown action: {actionBase.ActionName}. " +
                           "Creature may not have learned this action yet.");
            return false;
        }
    }

    /// <summary>
    /// Validate this loadout (called in editor)
    /// </summary>
    private void OnValidate()
    {
        ValidateSlot(CoreSlot0, ActionSlotType.Core, "Core Slot 0",
            a => a.Source == ActionSource.Physical &&
                 (a.Role == ActionRole.Offensive || a.Role == ActionRole.Support));

        ValidateSlot(CoreSlot1, ActionSlotType.Core, "Core Slot 1",
            a => a.Source == ActionSource.Magical &&
                 (a.Role == ActionRole.Offensive || a.Role == ActionRole.Support));

        ValidateSlot(CoreSlot2, ActionSlotType.Core, "Core Slot 2",
            a => a.Role == ActionRole.Defensive);

        ValidateSlot(EmpoweredSlot0, ActionSlotType.Empowered, "Empowered Slot 0");
        ValidateSlot(EmpoweredSlot1, ActionSlotType.Empowered, "Empowered Slot 1");
        ValidateSlot(EmpoweredSlot2, ActionSlotType.Empowered, "Empowered Slot 2");

        ValidateSlot(MasterySlot0, ActionSlotType.Mastery, "Mastery Slot 0");
    }

    private void ValidateSlot(ActionBase action, ActionSlotType expectedSlotType, string slotName,
        System.Func<ActionBase, bool> additionalValidation = null)
    {
        if (action == null) return;

        if (action.SlotType != expectedSlotType)
        {
            Debug.LogWarning($"[{name}] {slotName} has wrong SlotType. " +
                           $"Expected {expectedSlotType}, got {action.SlotType}");
        }

        if (additionalValidation != null && !additionalValidation(action))
        {
            Debug.LogWarning($"[{name}] {slotName}: {action.ActionName} doesn't meet slot requirements");
        }
    }
}