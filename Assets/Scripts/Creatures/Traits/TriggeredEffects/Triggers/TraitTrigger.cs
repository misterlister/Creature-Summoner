using UnityEngine;
using System;

[Serializable]
public abstract class TraitTrigger
{
    public abstract BattleEventType GetEventType();
    public abstract bool CheckTrigger(BattleEventData eventData);
    public abstract string GetDescription();
}
