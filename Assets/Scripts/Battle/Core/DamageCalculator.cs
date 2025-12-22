using UnityEngine;

namespace Game.Battle.Core
{
    public class DamageCalculator
    {
        private readonly BattleEventManager eventManager;
        public DamageCalculator(BattleEventManager eventManager)
        {
            this.eventManager = eventManager;
        }

        public int CalculateDamage(Creature attacker, Creature defender, ActionBase action)
        {
            // Base damage calculation
            return 0;
        }
    }
}