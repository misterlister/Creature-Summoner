
public enum ActionCategoryMenuOptions
{
    CoreActions,
    EmpoweredActions,
    MasteryActions,
    Move,
    Examine,
    ClassAction,
    EndTurn,
    Flee
}

public static class ActionCategoryMenuOptionsExtensions
{
    public static string ToDisplayName(this ActionCategoryMenuOptions option) => option switch
    {
        ActionCategoryMenuOptions.CoreActions => "Core Actions",
        ActionCategoryMenuOptions.EmpoweredActions => "Empowered Actions",
        ActionCategoryMenuOptions.MasteryActions => "Mastery Actions",
        ActionCategoryMenuOptions.Move => "Move",
        ActionCategoryMenuOptions.Examine => "Examine",
        ActionCategoryMenuOptions.ClassAction => "Class Action",
        ActionCategoryMenuOptions.EndTurn => "End Turn",
        ActionCategoryMenuOptions.Flee => "Flee",
        _ => option.ToString()
    };
}