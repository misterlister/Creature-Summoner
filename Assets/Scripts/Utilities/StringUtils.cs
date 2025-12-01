using UnityEngine;

public static class StringUtils
{
    public static string GetIndefiniteArticle(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return "a";
        }

        char first = char.ToLower(word[0]);
        return first is 'a' or 'e' or 'i' or 'o' or 'u' or 'y'
            ? "a"
            : "an";
    }
}
