using Game.Traits.Triggers;

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

    public static string GetPerspectiveString(Perspective? perspective)
    {
        return (perspective) switch
        {
            null => "",
            Perspective.Self => "this creature",
            Perspective.Ally => "an allied creature",
            Perspective.Opponent => "an enemy",
            Perspective.Team => "a member of this team",
            _ => "unknown perspective",
        };
    }

    public static string GetTimingString(ActionTiming timing)
    {
        return (timing) switch
        {
            ActionTiming.Before => "When",
            ActionTiming.After => "After",
            _ => "unknown timing"
        };
    }
}
