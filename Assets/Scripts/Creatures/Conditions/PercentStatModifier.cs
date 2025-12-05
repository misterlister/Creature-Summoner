using System;

[Serializable]
public class PercentStatModifier
{
    public StatType statType;
    public float value;
    public object source;

    public PercentStatModifier(StatType statType, float value, object source = null)
    {
        this.statType = statType;
        this.value = value;
        this.source = source;
    }
}
