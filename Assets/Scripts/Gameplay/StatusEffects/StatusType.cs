namespace Game.Statuses
{
    public enum StatusType
    {
        // DOTs (Banes)
        Chilled, Poisoned, Burning, Bleeding,

        // HOTs (Boons)
        Energized, Regenerating,

        // Persistent (Banes)
        Rooted, Destabilized, Exhausted, Withered,

        // CC (Banes)
        Blinded, Confused, Feared, Taunted,

        // Triggered (Banes)
        Stunned, Exposed,

        // Stat Buffs (Boons)
        Empowered, Enchanted, Honed, Hastened, Fortified, Warded,

        // Stat Debuffs (Banes)
        Weakened, Hexed, Hindered, Slowed, Sundered, Cursed
    }
}