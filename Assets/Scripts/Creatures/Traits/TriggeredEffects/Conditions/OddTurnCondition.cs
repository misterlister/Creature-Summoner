using System;

[Serializable]
public class OddTurnCondition : TraitCondition
{
    public override bool CheckCondition(TraitEventData eventData)
    {
        return eventData.TurnNumber % 2 == 1;
    }

    public override string GetDescription()
    {
        return "on odd-numbered turns";
    }
}
