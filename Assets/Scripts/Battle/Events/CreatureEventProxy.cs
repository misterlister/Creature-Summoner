using System;

public class CreatureEventProxy
{
    private Creature owner;
    private BattleEventManager globalEventManager;

    // Creature-specific events

    // Action Events
    public event Action<ActionEventData> OnBeforeIAct;
    public event Action<ActionEventData> OnAfterIAct;
    public event Action<ActionEventData> OnBeforeIAmTargetedByAction;
    public event Action<ActionEventData> OnAfterIAmTargetedByAction;
    public event Action<ActionEventData> OnBeforeIAmTargetedByAOE;
    public event Action<ActionEventData> OnAfterIAmTargetedByAOE;

    // Damage Events
    public event Action<DamageEventData> OnBeforeIReceiveDamage;
    public event Action<DamageEventData> OnAfterIReceiveDamage;
    public event Action<DamageEventData> OnBeforeIDealDamage;
    public event Action<DamageEventData> OnAfterIDealDamage;

    // Healing Events
    public event Action<HealEventData> OnBeforeIAmHealed;
    public event Action<HealEventData> OnAfterIAmHealed;
    public event Action<HealEventData> OnBeforeIHeal;
    public event Action<HealEventData> OnAfterIHeal;

    // Condition Events
    public event Action<ConditionAppliedEventData> OnBeforeConditionAppliedToMe;
    public event Action<ConditionAppliedEventData> OnAfterConditionAppliedToMe;
    public event Action<ConditionAppliedEventData> OnBeforeConditionAppliedByMe;
    public event Action<ConditionAppliedEventData> OnAfterConditionAppliedByMe;
    public event Action<ConditionRemovedEventData> OnConditionRemovedFromMe;

    // Movement Events
    public event Action<MoveEventData> OnBeforeIMove;
    public event Action<MoveEventData> OnAfterIMove;
    public event Action<MoveEventData> OnBeforeIAmForciblyMoved;
    public event Action<MoveEventData> OnAfterIAmForciblyMoved;

    // Turn Events
    public event Action<TurnStartEventData> OnMyTurnStart;
    public event Action<TurnEndEventData> OnMyTurnEnd;

    // Defeat Events
    public event Action<CreatureDefeatEventData> OnIAmDefeated;
    public event Action<CreatureDefeatEventData> OnIDefeatAnother;

    public CreatureEventProxy(Creature owner, BattleEventManager globalEventManager)
    {
        this.owner = owner;
        this.globalEventManager = globalEventManager;

        // Subscribe to all global events
        globalEventManager.OnBeforeAction += FilterBeforeAction;
        globalEventManager.OnAfterAction += FilterAfterAction;
        globalEventManager.OnBeforeDamage += FilterBeforeDamage;
        globalEventManager.OnAfterDamage += FilterAfterDamage;
        globalEventManager.OnBeforeHeal += FilterBeforeHeal;
        globalEventManager.OnAfterHeal += FilterAfterHeal;
        globalEventManager.OnBeforeConditionApplied += FilterBeforeConditionApplied;
        globalEventManager.OnAfterConditionApplied += FilterAfterConditionApplied;
        globalEventManager.OnConditionRemoved += FilterConditionRemoved;
        globalEventManager.OnBeforeMove += FilterBeforeMove;
        globalEventManager.OnAfterMove += FilterAfterMove;
        globalEventManager.OnTurnStart += FilterTurnStart;
        globalEventManager.OnTurnEnd += FilterTurnEnd;
        globalEventManager.OnCreatureDefeated += FilterCreatureDefeated;
    }

    private void FilterBeforeAction(ActionEventData eventData)
    {
        if (eventData.ActingCreature == owner)
        {
            OnBeforeIAct?.Invoke(eventData);
        }
        if (eventData.TargetCreature == owner)
        {
            OnBeforeIAmTargetedByAction?.Invoke(eventData);
        }
        if (eventData.AOETargetCreatures.Contains(owner))
        {
            OnBeforeIAmTargetedByAOE?.Invoke(eventData);
        }
    }

    private void FilterAfterAction(ActionEventData eventData)
    {
        if (eventData.ActingCreature == owner)
        {
            OnAfterIAct?.Invoke(eventData);
        }
        if (eventData.TargetCreature == owner)
        {
            OnAfterIAmTargetedByAction?.Invoke(eventData);
        }
        if (eventData.AOETargetCreatures.Contains(owner))
        {
            OnAfterIAmTargetedByAOE?.Invoke(eventData);
        }
    }

    private void FilterBeforeDamage(DamageEventData eventData)
    {
        if (eventData.Defender == owner)
            OnBeforeIReceiveDamage?.Invoke(eventData);
        if (eventData.Attacker == owner)
            OnBeforeIDealDamage?.Invoke(eventData);
    }

    private void FilterAfterDamage(DamageEventData eventData)
    {
        if (eventData.Defender == owner)
            OnAfterIReceiveDamage?.Invoke(eventData);
        if (eventData.Attacker == owner)
            OnAfterIDealDamage?.Invoke(eventData);
    }

    private void FilterBeforeHeal(HealEventData eventData)
    {
        if (eventData.HealedCreature == owner)
            OnBeforeIAmHealed?.Invoke(eventData);
        if (eventData.HealingCreature == owner)
            OnBeforeIHeal?.Invoke(eventData);
    }

    private void FilterAfterHeal(HealEventData eventData)
    {
        if (eventData.HealedCreature == owner)
            OnAfterIAmHealed?.Invoke(eventData);
        if (eventData.HealingCreature == owner)
            OnAfterIHeal?.Invoke(eventData);
    }

    private void FilterBeforeConditionApplied(ConditionAppliedEventData eventData)
    {
        if (eventData.TargetCreature == owner)
            OnBeforeConditionAppliedToMe?.Invoke(eventData);
        if (eventData.ApplyingCreature == owner)
            OnBeforeConditionAppliedByMe?.Invoke(eventData);
    }

    private void FilterAfterConditionApplied(ConditionAppliedEventData eventData)
    {
        if (eventData.TargetCreature == owner)
            OnAfterConditionAppliedToMe?.Invoke(eventData);
        if (eventData.ApplyingCreature == owner)
            OnAfterConditionAppliedByMe?.Invoke(eventData);
    }

    public void FilterConditionRemoved(ConditionRemovedEventData eventData)
    {
        if (eventData.AffectedCreature == owner)
            OnConditionRemovedFromMe?.Invoke(eventData);
    }

    private void FilterBeforeMove(MoveEventData eventData)
    {
        if (eventData.MovingCreature == owner)
        {
            if (eventData.MoveSource != null)
                OnBeforeIAmForciblyMoved?.Invoke(eventData);
            else
                OnBeforeIMove?.Invoke(eventData);
        }
    }

    private void FilterAfterMove(MoveEventData eventData)
    {
        if (eventData.MovingCreature == owner)
        {
            if (eventData.MoveSource != null)
                OnAfterIAmForciblyMoved?.Invoke(eventData);
            else
                OnAfterIMove?.Invoke(eventData);
        }
    }

    private void FilterTurnStart(TurnStartEventData eventData)
    {
        if (eventData.CurrentCreature == owner)
            OnMyTurnStart?.Invoke(eventData);
    }

    private void FilterTurnEnd(TurnEndEventData eventData)
    {
        if (eventData.CurrentCreature == owner)
            OnMyTurnEnd?.Invoke(eventData);
    }

    private void FilterCreatureDefeated(CreatureDefeatEventData eventData)
    {
        if (eventData.DefeatedCreature == owner)
            OnIAmDefeated?.Invoke(eventData);
        if (eventData.VictoriousCreature == owner)
            OnIDefeatAnother?.Invoke(eventData);
    }

    public void Cleanup()
    {
        globalEventManager.OnBeforeAction -= FilterBeforeAction;
        globalEventManager.OnAfterAction -= FilterAfterAction;
        globalEventManager.OnBeforeDamage -= FilterBeforeDamage;
        globalEventManager.OnAfterDamage -= FilterAfterDamage;
        globalEventManager.OnBeforeHeal -= FilterBeforeHeal;
        globalEventManager.OnAfterHeal -= FilterAfterHeal;
        globalEventManager.OnBeforeConditionApplied -= FilterBeforeConditionApplied;
        globalEventManager.OnAfterConditionApplied -= FilterAfterConditionApplied;
        globalEventManager.OnConditionRemoved -= FilterConditionRemoved;
        globalEventManager.OnBeforeMove -= FilterBeforeMove;
        globalEventManager.OnAfterMove -= FilterAfterMove;
        globalEventManager.OnTurnStart -= FilterTurnStart;
        globalEventManager.OnTurnEnd -= FilterTurnEnd;
        globalEventManager.OnCreatureDefeated -= FilterCreatureDefeated;
    }
}
