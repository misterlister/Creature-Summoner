using System.Linq;
using UnityEngine;

public static class GameConstantsExtensions
{
    // Max class buff level
    private static readonly int MaxValue = System.Enum.GetValues(typeof(ClassStatBuffLevel)).Cast<int>().Max();

    public static int GetValue(this ClassStatBuffLevel level)
    {
        return (int)level;
    }

    public static ClassStatBuffLevel Add(this ClassStatBuffLevel a, ClassStatBuffLevel b)
    {
        int sum = (int)a + (int)b;
        sum = Mathf.Clamp(sum, 0, MaxValue);
        return (ClassStatBuffLevel)sum;
    }
}
