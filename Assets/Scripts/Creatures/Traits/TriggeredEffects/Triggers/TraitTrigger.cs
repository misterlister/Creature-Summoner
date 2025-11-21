using UnityEngine;
using System;

[Serializable]
public abstract class TraitTrigger
{
    [SerializeField] protected string triggerName;

    public abstract bool CheckTrigger(TraitEventData eventData);
    public abstract string GetDescription();
}
