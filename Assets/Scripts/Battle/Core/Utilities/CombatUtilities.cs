public static class CombatUtilities
{
    public static float GetTotalCoverBonus(Creature creature, UnifiedBattlefield battlefield)
    {
        if (creature?.CurrentTile == null) return 0f;

        float totalBonus = 0f;

        // Add light cover from current tile
        if (creature.CurrentTile.Terrain != null)
        {
            totalBonus += creature.CurrentTile.Terrain.GetRangedDefenseAdjustment(creature);
        }

        // Add heavy cover from front tile
        var pos = battlefield.GetBattlePosition(creature);
        var frontPos = pos.GetForwardPosition();

        if (frontPos.HasValue)
        {
            var frontTile = battlefield.GetTile(frontPos.Value);
            if (frontTile?.Terrain is HeavyCoverTerrain)
            {
                totalBonus += frontTile.Terrain.GetRangedDefenseAdjustment(creature);
            }
        }

        return totalBonus;
    }
}