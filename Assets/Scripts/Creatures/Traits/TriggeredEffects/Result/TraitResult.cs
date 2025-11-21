using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class TraitResult
{
    public abstract void Execute(TraitEventData eventData);
    public abstract string GetDescription();
}
