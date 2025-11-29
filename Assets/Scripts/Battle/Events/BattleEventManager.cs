using System;

public class BattleEventManager
{
    // Action Events
    public event Action<ActionEventData> OnBeforeAction;
    public event Action<ActionEventData> OnAfterAction;

    // Damage Events
    public event Action<DamageEventData> OnBeforeDamage;
    public event Action<DamageEventData> OnAfterDamage;

    // Healing Events
    public event Action<HealEventData> OnBeforeHeal;
    public event Action<HealEventData> OnAfterHeal;

    // Condition Events
    public event Action<ConditionAppliedEventData> OnBeforeConditionApplied;
    public event Action<ConditionAppliedEventData> OnAfterConditionApplied;
    public event Action<ConditionRemovedEventData> OnConditionRemoved;

    // Movement Events
    public event Action<MoveEventData> OnBeforeMove;
    public event Action<MoveEventData> OnAfterMove;

    // Turn Events
    public event Action<TurnStartEventData> OnTurnStart;
    public event Action<TurnEndEventData> OnTurnEnd;

    // Defeat Events
    public event Action<CreatureDefeatEventData> OnCreatureDefeated;


    // Trigger Methods for Damage

    public void TriggerBeforeDamage(DamageEventData eventData)
    {
        OnBeforeDamage?.Invoke(eventData);
    }

    public void TriggerAfterDamage(DamageEventData eventData)
    {
        OnAfterDamage?.Invoke(eventData);
    }

    // Trigger Methods for Healing

    public void TriggerBeforeHeal(HealEventData eventData)
    {
        OnBeforeHeal?.Invoke(eventData);
    }

    public void TriggerAfterHeal(HealEventData eventData)
    {
        OnAfterHeal?.Invoke(eventData);
    }

    // Trigger Methods for Actions

    public void TriggerBeforeAction(ActionEventData eventData)
    {
        OnBeforeAction?.Invoke(eventData);
    }

    public void TriggerAfterAction(ActionEventData eventData)
    {
        OnAfterAction?.Invoke(eventData);
    }

    // Trigger Methods for Conditions

    public void TriggerBeforeConditionApplied(ConditionAppliedEventData eventData)
    {
        OnBeforeConditionApplied?.Invoke(eventData);
    }

    public void TriggerAfterConditionApplied(ConditionAppliedEventData eventData)
    {
        OnAfterConditionApplied?.Invoke(eventData);
    }

    public void TriggerConditionRemoved(ConditionRemovedEventData eventData)
    {
        OnConditionRemoved?.Invoke(eventData);
    }

    // Trigger Methods for Turns

    public void TriggerTurnStart(TurnStartEventData eventData)
    {
        OnTurnStart?.Invoke(eventData);
    }

    public void TriggerTurnEnd(TurnEndEventData eventData)
    {
        OnTurnEnd?.Invoke(eventData);
    }

    // Trigger Methods for Defeat

    public void TriggerCreatureDefeated(CreatureDefeatEventData eventData)
    {
        OnCreatureDefeated?.Invoke(eventData);
    }

    // Trigger Methods for Movement

    public void TriggerBeforeMove(MoveEventData eventData)
    {
        OnBeforeMove?.Invoke(eventData);
    }

    public void TriggerAfterMove(MoveEventData eventData)
    {
        OnAfterMove?.Invoke(eventData);
    }

    // Clear all subscriptions
    public void ClearAllSubscriptions()
    {
        OnBeforeDamage = null;
        OnAfterDamage = null;
        OnBeforeHeal = null;
        OnAfterHeal = null;
        OnBeforeAction = null;
        OnAfterAction = null;
        OnTurnStart = null;
        OnTurnEnd = null;
        OnBeforeMove = null;
        OnAfterMove = null;
        OnBeforeConditionApplied = null;
        OnAfterConditionApplied = null;
        OnConditionRemoved = null;
        OnCreatureDefeated = null;
    }
}
