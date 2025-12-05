using System;
using UnityEngine;

[Serializable]
public class FlatStatModifier
{
    public StatType statType;
    public int value;
    public object source;

    public FlatStatModifier(StatType statType, int value, object source = null)
    {
        this.statType = statType;
        this.value = value;
        this.source = source;
    }
}
