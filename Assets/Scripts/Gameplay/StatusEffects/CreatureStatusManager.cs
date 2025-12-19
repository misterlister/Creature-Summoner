
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Statuses
{
    public class CreatureStatusManager : MonoBehaviour
    {
        private Creature creature;
        private Dictionary<StatusType, StatusEffect> activeStatuses;

        void Awake()
        {
            creature = GetComponent<Creature>();
            activeStatuses = new Dictionary<StatusType, StatusEffect>();
        }

        public void ApplyStatus(StatusType type, int amount, bool isCrit, Creature source)
        {
            if (activeStatuses.ContainsKey(type))
            {
                var existing = activeStatuses[type];

                switch (existing.Category)
                {
                    case StatusCategory.OverTime:
                        ((OverTimeStatus)existing).AddToSchedule(amount, isCrit);
                        break;
                    case StatusCategory.Persistent:
                        ((PersistentStatus)existing).RefreshDuration(isCrit);
                        break;
                    case StatusCategory.CrowdControl:
                        ((CrowdControlStatus)existing).AddStacks(amount, isCrit);
                        break;
                    case StatusCategory.Triggered:
                        ((TriggeredStatus)existing).AddStacks(amount, isCrit);
                        break;
                    case StatusCategory.StatModifier:
                        ((StatModifierStatus)existing).AddStacks(amount, isCrit);
                        break;
                }
            }
            else
            {
                StatusEffect newStatus = CreateStatus(type, amount, isCrit, source);
                activeStatuses[type] = newStatus;
                newStatus.OnApply(creature);
            }

            OnStatusChanged?.Invoke();
        }

        private StatusEffect CreateStatus(StatusType type, int amount, bool isCrit, Creature source)
        {
            var category = StatusRules.GetCategory(type);

            switch (category)
            {
                case StatusCategory.OverTime:
                    return new OverTimeStatus(type, amount, isCrit, source);
                case StatusCategory.Persistent:
                    return new PersistentStatus(type, isCrit, source);
                case StatusCategory.CrowdControl:
                    return new CrowdControlStatus(type, amount, isCrit, source);
                case StatusCategory.Triggered:
                    return new TriggeredStatus(type, amount, isCrit, source);
                case StatusCategory.StatModifier:
                    return new StatModifierStatus(type, amount, isCrit, source);
                default:
                    throw new System.Exception($"Can't apply status. Unknown category for {type}");
            }
        }

        public void TickStatuses()
        {
            var expiredStatuses = new List<StatusType>();

            foreach (var kvp in activeStatuses)
            {
                kvp.Value.OnTick(creature);

                if (kvp.Value.ShouldExpire())
                {
                    expiredStatuses.Add(kvp.Key);
                }
            }

            foreach (var type in expiredStatuses)
            {
                RemoveStatus(type);
            }

            OnStatusChanged?.Invoke();
        }

        public void RemoveStatus(StatusType type)
        {
            if (activeStatuses.ContainsKey(type))
            {
                activeStatuses[type].OnRemove(creature);
                activeStatuses.Remove(type);
                OnStatusChanged?.Invoke();
            }
        }

        public void PurgeBoons()
        {
            var toRemove = activeStatuses.Where(kvp => kvp.Value.IsBoon).Select(kvp => kvp.Key).ToList();
            foreach (var type in toRemove)
            {
                RemoveStatus(type);
            }
        }

        public void CleanseBanes()
        {
            var toRemove = activeStatuses.Where(kvp => !kvp.Value.IsBoon).Select(kvp => kvp.Key).ToList();
            foreach (var type in toRemove)
            {
                RemoveStatus(type);
            }
        }

        public bool HasStatus(StatusType type)
        {
            return activeStatuses.ContainsKey(type);
        }

        public StatusEffect GetStatus(StatusType type)
        {
            return activeStatuses.ContainsKey(type) ? activeStatuses[type] : null;
        }

        public IEnumerable<StatusEffect> GetAllBoons()
        {
            return activeStatuses.Values.Where(s => s.IsBoon);
        }

        public IEnumerable<StatusEffect> GetAllBanes()
        {
            return activeStatuses.Values.Where(s => !s.IsBoon);
        }

        public float GetStatModifier(StatType stat)
        {
            float multiplier = 1.0f;

            foreach (var status in activeStatuses.Values.OfType<StatModifierStatus>())
            {
                if (status.Stat == stat)
                {
                    multiplier *= status.GetMultiplier();
                }
            }

            return multiplier;
        }

        public event System.Action OnStatusChanged;
    }
}