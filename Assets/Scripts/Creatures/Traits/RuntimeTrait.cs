using UnityEngine;
using System;
using System.Collections.Generic;

public class RuntimeTrait
{
    public TraitBase TraitData { get; private set; }
    public Creature Owner { get; private set; }

    private Dictionary<BattleEventType, List<Action<BattleEventData>>> eventHandlers
        = new();

    public RuntimeTrait(TraitBase traitData, Creature owner)
    {
        TraitData = traitData;
        Owner = owner;
    }

    public void Subscribe(CreatureEventProxy creatureEvents)
    {
        foreach (var effect in TraitData.TriggeredEffects)
        {
            if (effect.trigger == null)
            {
                Debug.LogWarning($"Trait '{TraitData.TraitName}' has an effect missing a trigger.");
                continue;
            }

            if (effect.result == null)
            {
                Debug.LogWarning($"Trait '{TraitData.TraitName}' has an effect missing a result.");
                continue;
            }

            BattleEventType eventType = effect.trigger.GetEventType();

            Action<BattleEventData> handler = (eventData) =>
            {
                if (effect.ShouldExecute(eventData))
                {
                    effect.Execute(eventData);
                }
            };

            if (!eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType] = new List<Action<BattleEventData>>();
            }
            eventHandlers[eventType].Add(handler);

            SubscribeToEvent(creatureEvents, eventType, handler);
        }
    }

    private void SubscribeToEvent(CreatureEventProxy proxy, BattleEventType eventType, Action<BattleEventData> handler)
    {
        switch (eventType)
        {
            // Action Events
            case BattleEventType.BeforeIAct:
                proxy.OnBeforeIAct += handler;
                break;
            case BattleEventType.AfterIAct:
                proxy.OnAfterIAct += handler;
                break;
            case BattleEventType.BeforeIAmTargetedByAction:
                proxy.OnBeforeIAmTargetedByAction += handler;
                break;
            case BattleEventType.AfterIAmTargetedByAction:
                proxy.OnAfterIAmTargetedByAction += handler;
                break;
            case BattleEventType.BeforeIAmTargetedByAOE:
                proxy.OnBeforeIAmTargetedByAOE += handler;
                break;
            case BattleEventType.AfterIAmTargetedByAOE:
                proxy.OnAfterIAmTargetedByAOE += handler;
                break;
            case BattleEventType.BeforeAllyActs:
                proxy.OnBeforeAllyActs += handler;
                break;
            case BattleEventType.AfterAllyActs:
                proxy.OnAfterAllyActs += handler;
                break;
            case BattleEventType.BeforeOpponentActs:
                proxy.OnBeforeOpponentActs += handler;
                break;
            case BattleEventType.AfterOpponentActs:
                proxy.OnAfterOpponentActs += handler;
                break;
            case BattleEventType.BeforeTeamActs:
                proxy.OnBeforeIAct += handler;
                proxy.OnBeforeAllyActs += handler;
                break;
            case BattleEventType.AfterTeamActs:
                proxy.OnAfterIAct += handler;
                proxy.OnAfterAllyActs += handler;
                break;

            // Damage Events
            case BattleEventType.BeforeIReceiveDamage:
                proxy.OnBeforeIReceiveDamage += handler;
                break;
            case BattleEventType.AfterIReceiveDamage:
                proxy.OnAfterIReceiveDamage += handler;
                break;
            case BattleEventType.BeforeIDealDamage:
                proxy.OnBeforeIDealDamage += handler;
                break;
            case BattleEventType.AfterIDealDamage:
                proxy.OnAfterIDealDamage += handler;
                break;
            case BattleEventType.BeforeAllyReceivesDamage:
                proxy.OnBeforeAllyReceivesDamage += handler;
                break;
            case BattleEventType.AfterAllyReceivesDamage:
                proxy.OnAfterAllyReceivesDamage += handler;
                break;
            case BattleEventType.BeforeOpponentReceivesDamage:
                proxy.OnBeforeOpponentReceivesDamage += handler;
                break;
            case BattleEventType.AfterOpponentReceivesDamage:
                proxy.OnAfterOpponentReceivesDamage += handler;
                break;
            case BattleEventType.BeforeTeamReceivesDamage:
                proxy.OnBeforeIReceiveDamage += handler;
                proxy.OnBeforeAllyReceivesDamage += handler;
                break;
            case BattleEventType.AfterTeamReceivesDamage:
                proxy.OnAfterIReceiveDamage += handler;
                proxy.OnAfterAllyReceivesDamage += handler;
                break;

            // Healing Events
            case BattleEventType.BeforeIHeal:
                proxy.OnBeforeIHeal += handler;
                break;
            case BattleEventType.AfterIHeal:
                proxy.OnAfterIHeal += handler;
                break;
            case BattleEventType.BeforeIAmHealed:
                proxy.OnBeforeIAmHealed += handler;
                break;
            case BattleEventType.AfterIAmHealed:
                proxy.OnAfterIAmHealed += handler;
                break;
            case BattleEventType.BeforeAllyHeals:
                proxy.OnBeforeAllyHeals += handler;
                break;
            case BattleEventType.AfterAllyHeals:
                proxy.OnAfterAllyHeals += handler;
                break;
            case BattleEventType.BeforeAllyIsHealed:
                proxy.OnBeforeAllyIsHealed += handler;
                break;
            case BattleEventType.AfterAllyIsHealed:
                proxy.OnAfterAllyIsHealed += handler;
                break;
            case BattleEventType.BeforeOpponentHeals:
                proxy.OnBeforeOpponentHeals += handler;
                break;
            case BattleEventType.AfterOpponentHeals:
                proxy.OnAfterOpponentHeals += handler;
                break;
            case BattleEventType.BeforeOpponentIsHealed:
                proxy.OnBeforeOpponentIsHealed += handler;
                break;
            case BattleEventType.AfterOpponentIsHealed:
                proxy.OnAfterOpponentIsHealed += handler;
                break;
            case BattleEventType.BeforeTeamHeals:
                proxy.OnBeforeIHeal += handler;
                proxy.OnBeforeAllyHeals += handler;
                break;
            case BattleEventType.AfterTeamHeals:
                proxy.OnAfterIHeal += handler;
                proxy.OnAfterAllyHeals += handler;
                break;
            case BattleEventType.BeforeTeamIsHealed:
                proxy.OnBeforeIAmHealed += handler;
                proxy.OnBeforeAllyIsHealed += handler;
                break;
            case BattleEventType.AfterTeamIsHealed:
                proxy.OnAfterIAmHealed += handler;
                proxy.OnAfterAllyIsHealed += handler;
                break;

            // Condition Events
            case BattleEventType.BeforeIApplyCondition:
                proxy.OnBeforeIApplyCondition += handler;
                break;
            case BattleEventType.AfterIApplyCondition:
                proxy.OnAfterIApplyCondition += handler;
                break;
            case BattleEventType.BeforeIReceiveCondition:
                proxy.OnBeforeIReceiveCondition += handler;
                break;
            case BattleEventType.AfterIReceiveCondition:
                proxy.OnAfterIReceiveCondition += handler;
                break;
            case BattleEventType.BeforeAllyAppliesCondition:
                proxy.OnBeforeAllyAppliesCondition += handler;
                break;
            case BattleEventType.AfterAllyAppliesCondition:
                proxy.OnAfterAllyAppliesCondition += handler;
                break;
            case BattleEventType.BeforeAllyReceivesCondition:
                proxy.OnBeforeAllyReceivesCondition += handler;
                break;
            case BattleEventType.AfterAllyReceivesCondition:
                proxy.OnAfterAllyReceivesCondition += handler;
                break;
            case BattleEventType.BeforeOpponentAppliesCondition:
                proxy.OnBeforeOpponentAppliesCondition += handler;
                break;
            case BattleEventType.AfterOpponentAppliesCondition:
                proxy.OnAfterOpponentAppliesCondition += handler;
                break;
            case BattleEventType.BeforeOpponentReceivesCondition:
                proxy.OnBeforeOpponentReceivesCondition += handler;
                break;
            case BattleEventType.AfterOpponentReceivesCondition:
                proxy.OnAfterOpponentReceivesCondition += handler;
                break;
            case BattleEventType.ConditionRemovedFromMe:
                proxy.OnConditionRemovedFromMe += handler;
                break;
            case BattleEventType.BeforeTeamAppliesCondition:
                proxy.OnBeforeIApplyCondition += handler;
                proxy.OnBeforeAllyAppliesCondition += handler;
                break;
            case BattleEventType.AfterTeamAppliesCondition:
                proxy.OnAfterIApplyCondition += handler;
                proxy.OnAfterAllyAppliesCondition += handler;
                break;
            case BattleEventType.BeforeTeamReceivesCondition:
                proxy.OnBeforeIReceiveCondition += handler;
                proxy.OnBeforeAllyReceivesCondition += handler;
                break;
            case BattleEventType.AfterTeamReceivesCondition:
                proxy.OnAfterIReceiveCondition += handler;
                proxy.OnAfterAllyReceivesCondition += handler;
                break;

            // Movement Events
            case BattleEventType.BeforeIMove:
                proxy.OnBeforeIMove += handler;
                break;
            case BattleEventType.AfterIMove:
                proxy.OnAfterIMove += handler;
                break;
            case BattleEventType.BeforeIAmForciblyMoved:
                proxy.OnBeforeIAmForciblyMoved += handler;
                break;
            case BattleEventType.AfterIAmForciblyMoved:
                proxy.OnAfterIAmForciblyMoved += handler;
                break;
            case BattleEventType.BeforeAllyMoves:
                proxy.OnBeforeAllyMoves += handler;
                break;
            case BattleEventType.AfterAllyMoves:
                proxy.OnAfterAllyMoves += handler;
                break;
            case BattleEventType.BeforeOpponentMoves:
                proxy.OnBeforeOpponentMoves += handler;
                break;
            case BattleEventType.AfterOpponentMoves:
                proxy.OnAfterOpponentMoves += handler;
                break;
            case BattleEventType.BeforeTeamMoves:
                proxy.OnBeforeIMove += handler;
                proxy.OnBeforeAllyMoves += handler;
                break;
            case BattleEventType.AfterTeamMoves:
                proxy.OnAfterIMove += handler;
                proxy.OnAfterAllyMoves += handler;
                break;

            // Turn Events
            case BattleEventType.MyTurnStart:
                proxy.OnMyTurnStart += handler;
                break;
            case BattleEventType.MyTurnEnd:
                proxy.OnMyTurnEnd += handler;
                break;
            case BattleEventType.AllyTurnStart:
                proxy.OnAllyTurnStart += handler;
                break;
            case BattleEventType.AllyTurnEnd:
                proxy.OnAllyTurnEnd += handler;
                break;
            case BattleEventType.OpponentTurnStart:
                proxy.OnOpponentTurnStart += handler;
                break;
            case BattleEventType.OpponentTurnEnd:
                proxy.OnOpponentTurnEnd += handler;
                break;
            case BattleEventType.TeamTurnStart:
                proxy.OnMyTurnStart += handler;
                proxy.OnAllyTurnStart += handler;
                break;
            case BattleEventType.TeamTurnEnd:
                proxy.OnMyTurnEnd += handler;
                proxy.OnAllyTurnEnd += handler;
                break;

            // Defeat Events
            case BattleEventType.IAmDefeated:
                proxy.OnIAmDefeated += handler;
                break;
            case BattleEventType.IDefeatAnother:
                proxy.OnIDefeatAnother += handler;
                break;
            case BattleEventType.AllyIsDefeated:
                proxy.OnAllyIsDefeated += handler;
                break;
            case BattleEventType.OpponentIsDefeated:
                proxy.OnOpponentIsDefeated += handler;
                break;
            case BattleEventType.TeamIsDefeated:
                proxy.OnIAmDefeated += handler;
                proxy.OnAllyIsDefeated += handler;
                break;

            default:
                Debug.LogWarning($"Unhandled event type: {eventType}");
                break;
        }
    }

    public void Unsubscribe(CreatureEventProxy creatureEvents)
    {
        foreach (var kvp in eventHandlers)
        {
            BattleEventType eventType = kvp.Key;
            List<Action<BattleEventData>> handlers = kvp.Value;

            foreach (var handler in handlers)
            {
                UnsubscribeFromEvent(creatureEvents, eventType, handler);
            }
        }
        eventHandlers.Clear();
    }

    private void UnsubscribeFromEvent(CreatureEventProxy proxy, BattleEventType eventType, Action<BattleEventData> handler)
    {
        switch (eventType)
        {
            // Action Events
            case BattleEventType.BeforeIAct:
                proxy.OnBeforeIAct -= handler;
                break;
            case BattleEventType.AfterIAct:
                proxy.OnAfterIAct -= handler;
                break;
            case BattleEventType.BeforeIAmTargetedByAction:
                proxy.OnBeforeIAmTargetedByAction -= handler;
                break;
            case BattleEventType.AfterIAmTargetedByAction:
                proxy.OnAfterIAmTargetedByAction -= handler;
                break;
            case BattleEventType.BeforeIAmTargetedByAOE:
                proxy.OnBeforeIAmTargetedByAOE -= handler;
                break;
            case BattleEventType.AfterIAmTargetedByAOE:
                proxy.OnAfterIAmTargetedByAOE -= handler;
                break;
            case BattleEventType.BeforeAllyActs:
                proxy.OnBeforeAllyActs -= handler;
                break;
            case BattleEventType.AfterAllyActs:
                proxy.OnAfterAllyActs -= handler;
                break;
            case BattleEventType.BeforeOpponentActs:
                proxy.OnBeforeOpponentActs -= handler;
                break;
            case BattleEventType.AfterOpponentActs:
                proxy.OnAfterOpponentActs -= handler;
                break;

            // Damage Events
            case BattleEventType.BeforeIReceiveDamage:
                proxy.OnBeforeIReceiveDamage -= handler;
                break;
            case BattleEventType.AfterIReceiveDamage:
                proxy.OnAfterIReceiveDamage -= handler;
                break;
            case BattleEventType.BeforeIDealDamage:
                proxy.OnBeforeIDealDamage -= handler;
                break;
            case BattleEventType.AfterIDealDamage:
                proxy.OnAfterIDealDamage -= handler;
                break;
            case BattleEventType.BeforeAllyReceivesDamage:
                proxy.OnBeforeAllyReceivesDamage -= handler;
                break;
            case BattleEventType.AfterAllyReceivesDamage:
                proxy.OnAfterAllyReceivesDamage -= handler;
                break;
            case BattleEventType.BeforeOpponentReceivesDamage:
                proxy.OnBeforeOpponentReceivesDamage -= handler;
                break;
            case BattleEventType.AfterOpponentReceivesDamage:
                proxy.OnAfterOpponentReceivesDamage -= handler;
                break;

            // Healing Events
            case BattleEventType.BeforeIHeal:
                proxy.OnBeforeIHeal -= handler;
                break;
            case BattleEventType.AfterIHeal:
                proxy.OnAfterIHeal -= handler;
                break;
            case BattleEventType.BeforeIAmHealed:
                proxy.OnBeforeIAmHealed -= handler;
                break;
            case BattleEventType.AfterIAmHealed:
                proxy.OnAfterIAmHealed -= handler;
                break;
            case BattleEventType.BeforeAllyHeals:
                proxy.OnBeforeAllyHeals -= handler;
                break;
            case BattleEventType.AfterAllyHeals:
                proxy.OnAfterAllyHeals -= handler;
                break;
            case BattleEventType.BeforeAllyIsHealed:
                proxy.OnBeforeAllyIsHealed -= handler;
                break;
            case BattleEventType.AfterAllyIsHealed:
                proxy.OnAfterAllyIsHealed -= handler;
                break;
            case BattleEventType.BeforeOpponentHeals:
                proxy.OnBeforeOpponentHeals -= handler;
                break;
            case BattleEventType.AfterOpponentHeals:
                proxy.OnAfterOpponentHeals -= handler;
                break;
            case BattleEventType.BeforeOpponentIsHealed:
                proxy.OnBeforeOpponentIsHealed -= handler;
                break;
            case BattleEventType.AfterOpponentIsHealed:
                proxy.OnAfterOpponentIsHealed -= handler;
                break;

            // Condition Events
            case BattleEventType.BeforeIApplyCondition:
                proxy.OnBeforeIApplyCondition -= handler;
                break;
            case BattleEventType.AfterIApplyCondition:
                proxy.OnAfterIApplyCondition -= handler;
                break;
            case BattleEventType.BeforeIReceiveCondition:
                proxy.OnBeforeIReceiveCondition -= handler;
                break;
            case BattleEventType.AfterIReceiveCondition:
                proxy.OnAfterIReceiveCondition -= handler;
                break;
            case BattleEventType.BeforeAllyAppliesCondition:
                proxy.OnBeforeAllyAppliesCondition -= handler;
                break;
            case BattleEventType.AfterAllyAppliesCondition:
                proxy.OnAfterAllyAppliesCondition -= handler;
                break;
            case BattleEventType.BeforeAllyReceivesCondition:
                proxy.OnBeforeAllyReceivesCondition -= handler;
                break;
            case BattleEventType.AfterAllyReceivesCondition:
                proxy.OnAfterAllyReceivesCondition -= handler;
                break;
            case BattleEventType.BeforeOpponentAppliesCondition:
                proxy.OnBeforeOpponentAppliesCondition -= handler;
                break;
            case BattleEventType.AfterOpponentAppliesCondition:
                proxy.OnAfterOpponentAppliesCondition -= handler;
                break;
            case BattleEventType.BeforeOpponentReceivesCondition:
                proxy.OnBeforeOpponentReceivesCondition -= handler;
                break;
            case BattleEventType.AfterOpponentReceivesCondition:
                proxy.OnAfterOpponentReceivesCondition -= handler;
                break;
            case BattleEventType.ConditionRemovedFromMe:
                proxy.OnConditionRemovedFromMe -= handler;
                break;

            // Movement Events
            case BattleEventType.BeforeIMove:
                proxy.OnBeforeIMove -= handler;
                break;
            case BattleEventType.AfterIMove:
                proxy.OnAfterIMove -= handler;
                break;
            case BattleEventType.BeforeIAmForciblyMoved:
                proxy.OnBeforeIAmForciblyMoved -= handler;
                break;
            case BattleEventType.AfterIAmForciblyMoved:
                proxy.OnAfterIAmForciblyMoved -= handler;
                break;
            case BattleEventType.BeforeAllyMoves:
                proxy.OnBeforeAllyMoves -= handler;
                break;
            case BattleEventType.AfterAllyMoves:
                proxy.OnAfterAllyMoves -= handler;
                break;
            case BattleEventType.BeforeOpponentMoves:
                proxy.OnBeforeOpponentMoves -= handler;
                break;
            case BattleEventType.AfterOpponentMoves:
                proxy.OnAfterOpponentMoves -= handler;
                break;

            // Turn Events
            case BattleEventType.MyTurnStart:
                proxy.OnMyTurnStart -= handler;
                break;
            case BattleEventType.MyTurnEnd:
                proxy.OnMyTurnEnd -= handler;
                break;
            case BattleEventType.AllyTurnStart:
                proxy.OnAllyTurnStart -= handler;
                break;
            case BattleEventType.AllyTurnEnd:
                proxy.OnAllyTurnEnd -= handler;
                break;
            case BattleEventType.OpponentTurnStart:
                proxy.OnOpponentTurnStart -= handler;
                break;
            case BattleEventType.OpponentTurnEnd:
                proxy.OnOpponentTurnEnd -= handler;
                break;

            // Defeat Events
            case BattleEventType.IAmDefeated:
                proxy.OnIAmDefeated -= handler;
                break;
            case BattleEventType.IDefeatAnother:
                proxy.OnIDefeatAnother -= handler;
                break;
            case BattleEventType.AllyIsDefeated:
                proxy.OnAllyIsDefeated -= handler;
                break;
            case BattleEventType.OpponentIsDefeated:
                proxy.OnOpponentIsDefeated -= handler;
                break;
        }
    }
}
