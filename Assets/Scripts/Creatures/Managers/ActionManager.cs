using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameConstants;

namespace Game.Creatures.Managers
{
    public class ActionManager
    {
        private Creature owner;

        private List<CreatureAction> knownActions;

        private Dictionary<ActionSlotType, CreatureAction[]> equippedSlots;

        public ActionManager(Creature owner)
        {
            this.owner = owner;
            knownActions = new List<CreatureAction>();

            equippedSlots = new Dictionary<ActionSlotType, CreatureAction[]>
            {
                { ActionSlotType.Core, new CreatureAction[CORE_SLOTS] },
                { ActionSlotType.Empowered, new CreatureAction[EMPOWERED_SLOTS] },
                { ActionSlotType.Mastery, new CreatureAction[MASTERY_SLOTS] }
            };
        }

        // Get all known actions
        public IReadOnlyList<CreatureAction> AllKnownActions => knownActions;

        // Get known actions by slot type
        public IEnumerable<CreatureAction> GetKnownActions(ActionSlotType slotType)
        {
            return knownActions.Where(a => a.Action.SlotType == slotType);
        }

        public IReadOnlyList<CreatureAction> GetEquippedActions(ActionSlotType slotType)
        {
            return equippedSlots[slotType];
        }

        // Helper method to get all equipped actions (useful for UI display)
        public List<CreatureAction> GetAllEquippedActions()
        {
            List<CreatureAction> allActions = new List<CreatureAction>();

            foreach (var slots in equippedSlots.Values)
            {
                allActions.AddRange(slots.Where(a => a != null));
            }

            return allActions;
        }

        // Convenience accessors
        public IEnumerable<CreatureAction> KnownCoreActions => GetKnownActions(ActionSlotType.Core);
        public IEnumerable<CreatureAction> KnownEmpoweredActions => GetKnownActions(ActionSlotType.Empowered);
        public IEnumerable<CreatureAction> KnownMasteryActions => GetKnownActions(ActionSlotType.Mastery);

        public IReadOnlyList<CreatureAction> EquippedCoreActions => GetEquippedActions(ActionSlotType.Core);
        public IReadOnlyList<CreatureAction> EquippedEmpoweredActions => GetEquippedActions(ActionSlotType.Empowered);
        public IReadOnlyList<CreatureAction> EquippedMasteryActions => GetEquippedActions(ActionSlotType.Mastery);

        public void InitializeKnownActions()
        {
            knownActions.Clear();
            foreach (var learnableAction in owner.Species.LearnableActions)
            {   if (learnableAction.Level <= owner.Level)
                {
                    knownActions.Add(new CreatureAction(learnableAction.Action));
                }
            }
        }

        public void CheckForNewActions(int newLevel)
        {
            foreach(var learnableAction in owner.Species.LearnableActions)
            {
                if (learnableAction.Level == newLevel)
                {
                    LearnAction(learnableAction.Action);
                }
            }
        }

        public bool LearnAction(ActionBase action)
        {
            if (!knownActions.Any(a => a.Action == action))
            {
                knownActions.Add(new CreatureAction(action));
                Debug.Log($"{owner.Nickname} learned new action: {action.ActionName}");
                return true;
            }

            return false;
        }

        public bool TryEquipAction(CreatureAction action, int slotIndex)
        {
            if (!knownActions.Contains(action))
            {
                Debug.LogWarning($"Cannot equip action {action.Action.ActionName} because it is not known by {owner.Nickname}");
                return false;
            }

            ActionSlotType slotType = action.Action.SlotType;
            int maxSlots = GetMaxSlotsForSlotType(slotType);

            if (slotIndex < 0 || slotIndex >= maxSlots)
            {
                Debug.LogWarning($"Invalid slot index {slotIndex} for action slot type {slotType}");
                return false;
            }

            // Validate Core Slot restrictions
            if (slotType == ActionSlotType.Core && !IsValidCoreSlotAction(action, slotIndex))
            {
                Debug.LogWarning($"Action {action.Action.ActionName} cannot be equipped in Core slot {slotIndex}. " +
                                $"Slot requirements not met.");
                return false;
            }

            equippedSlots[slotType][slotIndex] = action;
            Debug.Log($"{owner.Nickname} equipped action {action.Action.ActionName} in {slotType} slot {slotIndex}");
            return true;
        }

        private bool IsValidCoreSlotAction(CreatureAction action, int slotIndex)
        {
            ActionRole role = action.Action.Role;
            ActionSource source = action.Action.Source;

            // Core Slot 0: Physical Attack/Support
            // Core Slot 1: Magical Attack/Support
            // Core Slot 2: Defensive

            return slotIndex switch
            {
                0 => source == ActionSource.Physical && (role == ActionRole.Attack || role == ActionRole.Support),
                1 => source == ActionSource.Magical && (role == ActionRole.Attack || role == ActionRole.Support),
                2 => role == ActionRole.Defensive,
                _ => false,
            };
        }

        public IEnumerable<CreatureAction> GetValidActionsForCoreSlot(int slotIndex)
        {
            return KnownCoreActions.Where(action => IsValidCoreSlotAction(action, slotIndex));
        }

        public void SetupDefaultEquippedActions()
        {
            ClearAllEquippedActions();

            AutoEquipCoreSlots();
            AutoEquipSlotType(ActionSlotType.Empowered);
            AutoEquipSlotType(ActionSlotType.Mastery);
        }
        /*
        public bool LoadActionLoadout(CreatureActionLoadout loadout, bool fillEmptySlots = true)
        {
            if (loadout == null)
            {
                Debug.LogWarning($"Attempted to load null action loadout for {owner.Nickname}");

                if (fillEmptySlots)
                {
                    SetupDefaultEquippedActions();
                }

                return false;
            }

            ClearAllEquippedActions();

            int failedSlots = loadout.ApplyToCreature(owner);

            if (failedSlots > 0 && fillEmptySlots)
            {
                if (failedSlots == CORE_SLOTS + EMPOWERED_SLOTS + MASTERY_SLOTS)
                {
                    // Complete failure - use full default setup
                    Debug.LogWarning($"Loadout '{loadout.name}' failed to equip any actions for {owner.Nickname}. Using defaults.");
                    SetupDefaultEquippedActions();
                }
                else
                {
                    // Partial failure - fill only the empty slots
                    Debug.Log($"Filling {failedSlots} empty slot(s) for {owner.Nickname} with defaults");
                    FillEmptySlots();
                }
            }
            return failedSlots == 0;
        }
        */
        // Fill any empty slots with the most recently learned actions
        private void FillEmptySlots()
        {
            // Fill Core slots
            AutoEquipCoreSlots();

            // Fill Empowered slots
            AutoEquipSlotType(ActionSlotType.Empowered);

            // Fill Mastery slots
            AutoEquipSlotType(ActionSlotType.Mastery);
        }

        private void AutoEquipCoreSlots()
        {
            var coreActions = KnownCoreActions.ToList();
            if (coreActions.Count == 0) return;

            var slots = equippedSlots[ActionSlotType.Core];

            for (int i = coreActions.Count - 1; i >= 0; i--)
            {
                CreatureAction currentAction = coreActions[i];

                if (slots.All(s => s != null))
                {
                    break; // All slots filled
                }

                for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
                {
                    if (slots[slotIndex] == null && IsValidCoreSlotAction(currentAction, slotIndex))
                    {
                        slots[slotIndex] = currentAction;
                        break; // Move to next action
                    }
                }
            }
        }

        private void AutoEquipSlotType(ActionSlotType slotType)
        {
            var actions = GetKnownActions(slotType).ToList();

            if (actions.Count == 0)
            {
                return;
            }

            var slots = equippedSlots[slotType];
            int maxSlots = GetMaxSlotsForSlotType(slotType);

            int slotIndex = 0;

            for (int i = actions.Count - 1; i >= 0; i--)
            {
                if (slots.Contains(actions[i]))
                {
                    continue; // Already equipped
                }

                while (slotIndex < maxSlots && slots[slotIndex] != null)
                {
                    slotIndex++; // Find next empty slot
                }

                if (slotIndex < maxSlots)
                {
                    slots[slotIndex] = actions[i];
                    slotIndex++;
                }
            }
        }


        public void ClearAllEquippedActions()
        {
            foreach (var slots in equippedSlots.Values)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = null;
                }
            }
        }

        // Helper to check if an action is equipped
        public bool IsActionEquipped(CreatureAction action)
        {
            return equippedSlots.Values.Any(slots => slots.Contains(action));
        }


        public bool CanAffordAction(CreatureAction action)
        {
            return action.Action.SlotType switch
            {
                ActionSlotType.Core => true, // Core actions always affordable (they generate energy)
                ActionSlotType.Empowered => owner.Energy >= action.Action.EnergyValue,
                ActionSlotType.Mastery => CheckMasteryResource(action.Action.EnergyValue),
                _ => false
            };
        }

        private bool CheckMasteryResource(int cost)
        {
            // TODO: Implement mastery resource checking
            return true;
        }

        private int GetMaxSlotsForSlotType(ActionSlotType slotType)
        {
            return slotType switch
            {
                ActionSlotType.Core => CORE_SLOTS,
                ActionSlotType.Empowered => EMPOWERED_SLOTS,
                ActionSlotType.Mastery => MASTERY_SLOTS,
                _ => 0
            };
        }

        public IEnumerable<CreatureAction> FilterActions(
            ActionSlotType? slotType = null,
            ActionRole? role = null,
            ActionSource? source = null,
            CreatureElement? element = null)
        {
            var filtered = knownActions.AsEnumerable();

            if (slotType.HasValue)
                filtered = filtered.Where(a => a.Action.SlotType == slotType.Value);

            if (role.HasValue)
                filtered = filtered.Where(a => a.Action.Role == role.Value);

            if (source.HasValue)
                filtered = filtered.Where(a => a.Action.Source == source.Value);

            if (element.HasValue)
                filtered = filtered.Where(a => a.Action.Element == element.Value);

            return filtered;
        }
    }
}
