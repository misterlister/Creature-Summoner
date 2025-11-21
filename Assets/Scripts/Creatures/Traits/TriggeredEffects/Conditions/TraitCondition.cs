using UnityEngine;
using System;

[Serializable]
public abstract class TraitCondition
{
    [SerializeField] string conditionName;

    public abstract bool CheckCondition(TraitEventData eventData);
    public abstract string GetDescription();
}
