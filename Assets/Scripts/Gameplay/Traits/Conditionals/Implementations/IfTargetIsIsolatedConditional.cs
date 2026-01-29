using System;

namespace Game.Traits.Conditionals
{
    [Serializable]
    public class IfTargetIsIsolatedConditional : TraitConditional
    {
        public override bool CheckConditional(BattleEventData eventData)
        {
            if (eventData is not ActionEventData actionData)
            {
                return false;
            }

            Creature target = actionData.TargetCreature;

            if (target == null)
            {
                return false;
            }


            var adjacentSlots = target.CurrentTile.GetAdjacentAllies();
            return adjacentSlots.Count == 0;
        }

        public override string GetDescription()
        {
            return $"when the target enemy has no adjacent allies";
        }
    }
}