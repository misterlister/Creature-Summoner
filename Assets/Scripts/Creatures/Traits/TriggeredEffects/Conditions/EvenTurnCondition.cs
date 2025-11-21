using System;

[Serializable]
public class EvenTurnCondition : TraitCondition
{
    public override bool CheckCondition(TraitEventData eventData)
    {
        return eventData.TurnNumber % 2 == 0;
    }

    public override string GetDescription()
    {
        return "on even-numbered turns";
    }
}
