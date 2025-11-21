using System;

[Serializable]
public class StatModification
{
    public float flatBonus = 0f;
    public float multiplier = 1f;

    public void Combine(StatModification other)
    {
        flatBonus += other.flatBonus;
        multiplier *= other.multiplier;
    }

    public float Apply(float baseValue)
    {
        return (baseValue + flatBonus) * multiplier;
    }
}
