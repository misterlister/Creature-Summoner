using System;
using UnityEngine;

[Serializable]
public class TurnParityCondition : TraitCondition
{
    [SerializeField] private TurnParity requiredParity = TurnParity.Even;

    public enum TurnParity
    {
        Even,
        Odd
    }

    public override bool CheckCondition(BattleEventData eventData)
    {
        int currentTurn = eventData.BattleContext.TurnNumber;

        bool isEven = currentTurn % 2 == 0;

        return requiredParity == TurnParity.Even ? isEven : !isEven;
    }

    public override string GetDescription()
    {
        return requiredParity == TurnParity.Even
            ? "on even turns"
            : "on odd turns";
    }
}
