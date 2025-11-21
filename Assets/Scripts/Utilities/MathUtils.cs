using UnityEngine;

public static class MathUtils
{
    public static float NormalizeInt(int value, int max)
    {
        return (float)value / max;
    }

    public static bool Compare(float a, float b, ComparisonType type)
    {
        switch (type)
        {
            case ComparisonType.LessThan:
                return a < b;
            case ComparisonType.LessThanOrEqual:
                return a <= b;
            case ComparisonType.Equal:
                return Mathf.Approximately(a, b);
            case ComparisonType.GreaterThanOrEqual:
                return a >= b;
            case ComparisonType.GreaterThan:
                return a > b;
        }

        return false;
    }
}
