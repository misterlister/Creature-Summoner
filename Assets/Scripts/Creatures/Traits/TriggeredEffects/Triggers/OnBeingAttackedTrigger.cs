using UnityEngine;
using System;

[Serializable]
public class OnBeingAttackedTrigger : TraitTrigger
{
    

    [SerializeField] private AttackTriggerCategory attackCategory = AttackTriggerCategory.Any;

    public override bool CheckTrigger(TraitEventData eventData)
    {
        if (eventData.Defender != eventData.SourceCreature)
        { 
            return false;
        }

        if (eventData.Action == null)
        {
            return false;
        }

        return attackCategory switch
        {
            AttackTriggerCategory.Any => true,
            AttackTriggerCategory.Melee => eventData.Action.IsMelee(),
            AttackTriggerCategory.Ranged => eventData.Action.IsRanged(),
            AttackTriggerCategory.Magic => eventData.Action.IsMagic(),
            AttackTriggerCategory.Physical => eventData.Action.IsPhysical(),
            AttackTriggerCategory.Fire => eventData.Action.IsType(CreatureType.Fire),
            AttackTriggerCategory.Earth => eventData.Action.IsType(CreatureType.Earth),
            AttackTriggerCategory.Water => eventData.Action.IsType(CreatureType.Water),
            AttackTriggerCategory.Air => eventData.Action.IsType(CreatureType.Air),
            AttackTriggerCategory.Radiant => eventData.Action.IsType(CreatureType.Radiant),
            AttackTriggerCategory.Metal => eventData.Action.IsType(CreatureType.Metal),
            AttackTriggerCategory.Cold => eventData.Action.IsType(CreatureType.Cold),
            AttackTriggerCategory.Electric => eventData.Action.IsType(CreatureType.Electric),
            AttackTriggerCategory.Arcane => eventData.Action.IsType(CreatureType.Arcane),
            AttackTriggerCategory.Beast => eventData.Action.IsType(CreatureType.Beast),
            AttackTriggerCategory.Necrotic => eventData.Action.IsType(CreatureType.Necrotic),
            AttackTriggerCategory.Plant => eventData.Action.IsType(CreatureType.Plant),
            _ => false,
        };
    }

    public override string GetDescription()
    {
        return attackCategory == AttackTriggerCategory.Any ? 
            "When this creature is attacked" : 
            $"When this creature is attacked with a {attackCategory} attack";
    }
}
