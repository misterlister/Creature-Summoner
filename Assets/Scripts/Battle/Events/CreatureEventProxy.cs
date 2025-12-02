using System;

public class CreatureEventProxy
{
    private Creature owner;
    private BattleEventManager globalEventManager;
    public BattleEventManager GlobalEventManager => globalEventManager;

    // Creature-specific events

    // Action Events
    public event Action<ActionEventData> OnBeforeIAct;
    public event Action<ActionEventData> OnAfterIAct;
    public event Action<ActionEventData> OnBeforeIAmTargetedByAction;
    public event Action<ActionEventData> OnAfterIAmTargetedByAction;
    public event Action<ActionEventData> OnBeforeIAmTargetedByAOE;
    public event Action<ActionEventData> OnAfterIAmTargetedByAOE;
    public event Action<ActionEventData> OnBeforeAllyActs;
    public event Action<ActionEventData> OnAfterAllyActs;
    public event Action<ActionEventData> OnBeforeOpponentActs;
    public event Action<ActionEventData> OnAfterOpponentActs;

    // Damage Events
    public event Action<DamageEventData> OnBeforeIReceiveDamage;
    public event Action<DamageEventData> OnAfterIReceiveDamage;
    public event Action<DamageEventData> OnBeforeIDealDamage;
    public event Action<DamageEventData> OnAfterIDealDamage;
    public event Action<DamageEventData> OnBeforeAllyReceivesDamage;
    public event Action<DamageEventData> OnAfterAllyReceivesDamage;
    public event Action<DamageEventData> OnBeforeOpponentReceivesDamage;
    public event Action<DamageEventData> OnAfterOpponentReceivesDamage;

    // Healing Events
    public event Action<HealEventData> OnBeforeIAmHealed;
    public event Action<HealEventData> OnAfterIAmHealed;
    public event Action<HealEventData> OnBeforeIHeal;
    public event Action<HealEventData> OnAfterIHeal;
    public event Action<HealEventData> OnBeforeAllyHeals;
    public event Action<HealEventData> OnAfterAllyHeals;
    public event Action<HealEventData> OnBeforeAllyIsHealed;
    public event Action<HealEventData> OnAfterAllyIsHealed;
    public event Action<HealEventData> OnBeforeOpponentHeals;
    public event Action<HealEventData> OnAfterOpponentHeals;
    public event Action<HealEventData> OnBeforeOpponentIsHealed;
    public event Action<HealEventData> OnAfterOpponentIsHealed;

    // Condition Events
    public event Action<ConditionAppliedEventData> OnBeforeIReceiveCondition;
    public event Action<ConditionAppliedEventData> OnAfterIReceiveCondition;
    public event Action<ConditionAppliedEventData> OnBeforeIApplyCondition;
    public event Action<ConditionAppliedEventData> OnAfterIApplyCondition;
    public event Action<ConditionAppliedEventData> OnBeforeAllyAppliesCondition;
    public event Action<ConditionAppliedEventData> OnAfterAllyAppliesCondition;
    public event Action<ConditionAppliedEventData> OnBeforeAllyReceivesCondition;
    public event Action<ConditionAppliedEventData> OnAfterAllyReceivesCondition;
    public event Action<ConditionAppliedEventData> OnBeforeOpponentAppliesCondition;
    public event Action<ConditionAppliedEventData> OnAfterOpponentAppliesCondition;
    public event Action<ConditionAppliedEventData> OnBeforeOpponentReceivesCondition;
    public event Action<ConditionAppliedEventData> OnAfterOpponentReceivesCondition;
    public event Action<ConditionRemovedEventData> OnConditionRemovedFromMe;

    // Movement Events
    public event Action<MoveEventData> OnBeforeIMove;
    public event Action<MoveEventData> OnAfterIMove;
    public event Action<MoveEventData> OnBeforeIAmForciblyMoved;
    public event Action<MoveEventData> OnAfterIAmForciblyMoved;
    public event Action<MoveEventData> OnBeforeIForciblyMoveAnother;
    public event Action<MoveEventData> OnAfterIForciblyMoveAnother;
    public event Action<MoveEventData> OnBeforeAllyMoves;
    public event Action<MoveEventData> OnAfterAllyMoves;
    public event Action<MoveEventData> OnBeforeOpponentMoves;
    public event Action<MoveEventData> OnAfterOpponentMoves;

    // Turn Events
    public event Action<TurnStartEventData> OnMyTurnStart;
    public event Action<TurnEndEventData> OnMyTurnEnd;
    public event Action<TurnStartEventData> OnAllyTurnStart;
    public event Action<TurnEndEventData> OnAllyTurnEnd;
    public event Action<TurnStartEventData> OnOpponentTurnStart;
    public event Action<TurnEndEventData> OnOpponentTurnEnd;

    // Defeat Events
    public event Action<CreatureDefeatEventData> OnIAmDefeated;
    public event Action<CreatureDefeatEventData> OnIDefeatAnother;
    public event Action<CreatureDefeatEventData> OnAllyIsDefeated;
    public event Action<CreatureDefeatEventData> OnOpponentIsDefeated;

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
        Creature actor = eventData.ActingCreature;
        Creature target = eventData.TargetCreature;

        if (actor == owner)
        {
            OnBeforeIAct?.Invoke(eventData);
        }
        else if (actor.IsAlly(owner))
        {
            OnBeforeAllyActs?.Invoke(eventData);
        }
        else
        {
            OnBeforeOpponentActs?.Invoke(eventData);
        }

        if (target == owner)
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
        Creature actor = eventData.ActingCreature;
        Creature target = eventData.TargetCreature;

        if (actor == owner)
        {
            OnAfterIAct?.Invoke(eventData);
        } else if (actor.IsAlly(owner))
        {
            OnAfterAllyActs?.Invoke(eventData);
        }
        else
        {
            OnAfterOpponentActs?.Invoke(eventData);
        }

        if (target == owner)
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
        Creature attacker = eventData.Attacker;
        Creature defender = eventData.Defender;

        if (defender == owner)
        {
            OnBeforeIReceiveDamage?.Invoke(eventData);
        }
        else if (defender.IsAlly(owner))
        {
            OnBeforeAllyReceivesDamage?.Invoke(eventData);
        }
        else
        {
            OnBeforeOpponentReceivesDamage?.Invoke(eventData);
        }

        if (attacker == owner)
        {
            OnBeforeIDealDamage?.Invoke(eventData);
        }
    }

    private void FilterAfterDamage(DamageEventData eventData)
    {
        Creature attacker = eventData.Attacker;
        Creature defender = eventData.Defender;
        if (defender == owner)
        {
            OnAfterIReceiveDamage?.Invoke(eventData);
        }
        else if (defender.IsAlly(owner))
        {
            OnAfterAllyReceivesDamage?.Invoke(eventData);
        }
        else
        {
            OnAfterOpponentReceivesDamage?.Invoke(eventData);
        }

        if (attacker == owner)
        {
            OnAfterIDealDamage?.Invoke(eventData);
        }
    }

    private void FilterBeforeHeal(HealEventData eventData)
    {
        Creature healer = eventData.HealingCreature;
        Creature patient = eventData.HealedCreature;
        if (patient == owner)
        {
            OnBeforeIAmHealed?.Invoke(eventData);
        }
        else if (patient.IsAlly(owner))
        {
            OnBeforeAllyIsHealed?.Invoke(eventData);
        }
        else
        {
            OnBeforeOpponentIsHealed?.Invoke(eventData);
        }


        if (healer == owner)
        {
            OnBeforeIHeal?.Invoke(eventData);
        }
        else if (healer.IsAlly(owner))
        {
            OnBeforeAllyHeals?.Invoke(eventData);
        }
        else
        {
            OnBeforeOpponentHeals?.Invoke(eventData);
        }
    }

    private void FilterAfterHeal(HealEventData eventData)
    {
        Creature healer = eventData.HealingCreature;
        Creature patient = eventData.HealedCreature;
        if (patient == owner)
        {
            OnAfterIAmHealed?.Invoke(eventData);
        }
        else if (patient.IsAlly(owner))
        {
            OnAfterAllyIsHealed?.Invoke(eventData);
        }
        else
        {
            OnAfterOpponentIsHealed?.Invoke(eventData);
        }

        if (healer == owner)
        {
            OnAfterIHeal?.Invoke(eventData);
        }
        else if (healer.IsAlly(owner))
        {
            OnAfterAllyHeals?.Invoke(eventData);
        }
        else
        {
            OnAfterOpponentHeals?.Invoke(eventData);
        }
    }

    private void FilterBeforeConditionApplied(ConditionAppliedEventData eventData)
    {
        Creature applier = eventData.ApplyingCreature;
        Creature receiver = eventData.TargetCreature;

        if (receiver == owner)
        { 
            OnBeforeIReceiveCondition?.Invoke(eventData);
        }
        else if (receiver.IsAlly(owner))
        {
            OnBeforeAllyReceivesCondition?.Invoke(eventData);
        }
        else
        {
            OnBeforeOpponentReceivesCondition?.Invoke(eventData);
        }

        if (applier == owner)
        {
            OnBeforeIApplyCondition?.Invoke(eventData);
        }
        else if (applier.IsAlly(owner))
        {
            OnBeforeAllyAppliesCondition?.Invoke(eventData);
        }
        else
        {
            OnBeforeOpponentAppliesCondition?.Invoke(eventData);
        }
    }

    private void FilterAfterConditionApplied(ConditionAppliedEventData eventData)
    {
        Creature applier = eventData.ApplyingCreature;
        Creature receiver = eventData.TargetCreature;
        if (receiver == owner)
        {
            OnAfterIReceiveCondition?.Invoke(eventData);
        }
        else if (receiver.IsAlly(owner))
        {
            OnAfterAllyReceivesCondition?.Invoke(eventData);
        }
        else
        {
            OnAfterOpponentReceivesCondition?.Invoke(eventData);
        }

        if (applier == owner)
        {
            OnAfterIApplyCondition?.Invoke(eventData);
        }
        else if (applier.IsAlly(owner))
        {
            OnAfterAllyAppliesCondition?.Invoke(eventData);
        }
        else
        {
            OnAfterOpponentAppliesCondition?.Invoke(eventData);
        }
    }

    public void FilterConditionRemoved(ConditionRemovedEventData eventData)
    {
        if (eventData.AffectedCreature == owner)
            OnConditionRemovedFromMe?.Invoke(eventData);
    }

    private void FilterBeforeMove(MoveEventData eventData)
    {
        Creature movingCreature = eventData.MovingCreature;
        Creature forcibleMover = eventData.ForcibleMover;

        if (movingCreature == owner)
        {
            if (forcibleMover != null)
                OnBeforeIAmForciblyMoved?.Invoke(eventData);
            else
                OnBeforeIMove?.Invoke(eventData);
        }
        else
        {
            if (forcibleMover == owner)
            {
                OnBeforeIForciblyMoveAnother?.Invoke(eventData);
            }

            if (movingCreature.IsAlly(owner))
            {
                OnBeforeAllyMoves?.Invoke(eventData);
            }
            else if (movingCreature.IsEnemy(owner))
            {
                OnBeforeOpponentMoves?.Invoke(eventData);
            }
        }
    }

    private void FilterAfterMove(MoveEventData eventData)
    {
        Creature movingCreature = eventData.MovingCreature;
        Creature forcibleMover = eventData.ForcibleMover;

        if (movingCreature == owner)
        {
            if (forcibleMover != null)
                OnAfterIAmForciblyMoved?.Invoke(eventData);
            else
                OnAfterIMove?.Invoke(eventData);
        }
        else
        {
            if (forcibleMover == owner)
            {
                OnAfterIForciblyMoveAnother?.Invoke(eventData);
            }

            if (movingCreature.IsAlly(owner))
            {
                OnAfterAllyMoves?.Invoke(eventData);
            }
            else if (movingCreature.IsEnemy(owner))
            {
                OnAfterOpponentMoves?.Invoke(eventData);
            }
        }
    }

    private void FilterTurnStart(TurnStartEventData eventData)
    {
        Creature turnCreature = eventData.CurrentCreature;

        if (turnCreature == owner)
        {
            OnMyTurnStart?.Invoke(eventData);
        }
        else if (turnCreature.IsAlly(owner))
        {
            OnAllyTurnStart?.Invoke(eventData);
        }
        else if (turnCreature.IsEnemy(owner))
        {
            OnOpponentTurnStart?.Invoke(eventData);
        }
    }

    private void FilterTurnEnd(TurnEndEventData eventData)
    {
        Creature turnCreature = eventData.CurrentCreature;
        if (turnCreature == owner)
        {
            OnMyTurnEnd?.Invoke(eventData);
        }
        else if (turnCreature.IsAlly(owner))
        {
            OnAllyTurnEnd?.Invoke(eventData);
        }
        else if (turnCreature.IsEnemy(owner))
        {
            OnOpponentTurnEnd?.Invoke(eventData);
        }
    }

    private void FilterCreatureDefeated(CreatureDefeatEventData eventData)
    {
        Creature defeated = eventData.DefeatedCreature;

        if (defeated == owner)
        { 
            OnIAmDefeated?.Invoke(eventData);
        }
        else if (defeated.IsAlly(owner))
        {
            OnAllyIsDefeated?.Invoke(eventData);
        }
        else if (defeated.IsEnemy(owner))
        {
            OnOpponentIsDefeated?.Invoke(eventData);
        }

        if (eventData.VictoriousCreature == owner)
        {
            OnIDefeatAnother?.Invoke(eventData);
        }
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
