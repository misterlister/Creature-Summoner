using UnityEngine;
using System;

[Serializable]
public abstract class TraitConditional
{
    [SerializeField] string conditionalName;

    public abstract bool CheckConditional(BattleEventData eventData);
    public abstract string GetDescription();
}
