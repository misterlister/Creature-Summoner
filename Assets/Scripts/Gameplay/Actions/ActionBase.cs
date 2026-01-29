using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAction", menuName = "Action/Create new Action")]
public class ActionBase : ScriptableObject
{
    [SerializeField] string actionName;
    [TextArea]
    [SerializeField] string description;

    public string ActionName => actionName;
    public string Description => description;

    [SerializeField] CreatureElement element;
    [SerializeField] ActionSlotType slotType;
    [SerializeField] ActionSource source;
    [SerializeField] ActionRole role;
    [SerializeField] ActionRange range;
    [SerializeField] TargetType validTargets;
    [SerializeField] int energyValue = 0;
    [SerializeField] int power = 40;
    [SerializeField] int accuracy = 90;
    [SerializeField] int baseCrit = 5;
    [SerializeField] AOE areaOfEffect = AOE.Single;
    [SerializeField] bool preparation = false;
    [SerializeField] List<ActionTag> tags;

    public CreatureElement Element => element;
    public ActionSlotType SlotType => slotType;
    public ActionSource Source => source;
    public ActionRole Role => role;
    public int Power => power;
    public int Accuracy => accuracy;
    public ActionRange Range => range;
    public TargetType ValidTargets => validTargets;
    public bool Preparation => preparation;
    public AOE AreaOfEffect => areaOfEffect;
    public int BaseCrit => baseCrit;
    public List<ActionTag> Tags => tags;
    public int EnergyValue => energyValue;

    // Simple query methods
    public bool IsMelee() => !Tags.Contains(ActionTag.NoContact) && range == ActionRange.Melee;
    public bool IsRanged() => range == ActionRange.Short || range == ActionRange.Long;
    public bool IsMagic() => source == ActionSource.Magical;
    public bool IsPhysical() => source == ActionSource.Physical;
    public bool IsElement(CreatureElement checkType) => element == checkType;

    /// <summary>
    /// Get all valid targets for this action.
    /// Delegates to the battlefield's targeting system.
    /// </summary>
    public List<BattleTile> GetValidTargets(Creature attacker, UnifiedBattlefield battlefield)
    {
        if (attacker == null || battlefield == null)
            return new List<BattleTile>();

        return battlefield.TargetingSystem.GetValidTargetsForAction(attacker, this);
    }

    /// <summary>
    /// Get AOE targets centered on a specific tile.
    /// </summary>
    public List<BattleTile> GetAOETargets(BattleTile centerTile, UnifiedBattlefield battlefield, int yChoice = 0)
    {
        if (centerTile == null || battlefield == null || AreaOfEffect == AOE.Single)
            return new List<BattleTile> { centerTile };

        var centerPos = battlefield.GetBattlePosition(centerTile);
        var isPlayer = centerPos.GetTeamSide() == TeamSide.Player;

        var aoeTargets = AOETargetCalculator.GetTargets(
            centerPos,
            AreaOfEffect,
            battlefield,
            isPlayer,
            yChoice
        );

        var allTargets = new List<BattleTile> { centerTile };
        allTargets.AddRange(aoeTargets);

        return allTargets;
    }

    /// <summary>
    /// Check if this action can be used by the creature.
    /// </summary>
    public bool CanUse(Creature attacker)
    {
        if (attacker == null || attacker.IsDefeated)
            return false;

        if (SlotType == ActionSlotType.Empowered && attacker.Energy < EnergyValue)
            return false;

        return true;
    }

    /// <summary>
    /// Check if this action has any valid targets available.
    /// </summary>
    public bool HasValidTargets(Creature attacker, UnifiedBattlefield battlefield)
    {
        var targets = GetValidTargets(attacker, battlefield);
        return targets.Count > 0;
    }
}

[System.Serializable]
public class LearnableAction
{
    [SerializeField] ActionBase action;
    [SerializeField] int level;

    public ActionBase Action => action;
    public int Level => level;
}