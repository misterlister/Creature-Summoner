using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureClass", menuName = "Classes/Create a new Base Class")]
public class CreatureClass : ScriptableObject
{
    [SerializeField] string creatureClassName;
    [TextArea]
    [SerializeField] string description;

    [SerializeField] ClassStatBuffLevel hpAdjustment;
    [SerializeField] ClassStatBuffLevel energyAdjustment;
    [SerializeField] ClassStatBuffLevel strengthAdjustment;
    [SerializeField] ClassStatBuffLevel magicAdjustment;
    [SerializeField] ClassStatBuffLevel skillAdjustment;
    [SerializeField] ClassStatBuffLevel speedAdjustment;
    [SerializeField] ClassStatBuffLevel defenseAdjustment;
    [SerializeField] ClassStatBuffLevel resistanceAdjustment;

    public string CreatureClassName => creatureClassName;
    public string Description => description;
    public virtual ClassStatBuffLevel HpAdjustment => hpAdjustment;
    public virtual ClassStatBuffLevel EnergyAdjustment => energyAdjustment;
    public virtual ClassStatBuffLevel StrengthAdjustment => strengthAdjustment;
    public virtual ClassStatBuffLevel MagicAdjustment => magicAdjustment;
    public virtual ClassStatBuffLevel SkillAdjustment => skillAdjustment;
    public virtual ClassStatBuffLevel SpeedAdjustment => speedAdjustment;
    public virtual ClassStatBuffLevel DefenseAdjustment => defenseAdjustment;
    public virtual ClassStatBuffLevel ResistanceAdjustment => resistanceAdjustment;

    [SerializeField] List<LearnableTrait> learnableTraits;
    public List<LearnableTrait> LearnableTraits => learnableTraits;

}

[CreateAssetMenu(fileName = "CreatureClass", menuName = "Classes/Create a new Specialized Class")]
public class SpecializedClass : CreatureClass
{
    [SerializeField] CreatureClass baseClass;

    [SerializeField] ClassStatBuffLevel specHpAdjustment;
    [SerializeField] ClassStatBuffLevel specEnergyAdjustment;
    [SerializeField] ClassStatBuffLevel specStrengthAdjustment;
    [SerializeField] ClassStatBuffLevel specMagicAdjustment;
    [SerializeField] ClassStatBuffLevel specSkillAdjustment;
    [SerializeField] ClassStatBuffLevel specSpeedAdjustment;
    [SerializeField] ClassStatBuffLevel specDefenseAdjustment;
    [SerializeField] ClassStatBuffLevel specResistanceAdjustment;

    public override ClassStatBuffLevel HpAdjustment => baseClass.HpAdjustment.Add(specHpAdjustment);
    public override ClassStatBuffLevel EnergyAdjustment => baseClass.EnergyAdjustment.Add(specEnergyAdjustment);
    public override ClassStatBuffLevel StrengthAdjustment => baseClass.StrengthAdjustment.Add(specStrengthAdjustment);
    public override ClassStatBuffLevel MagicAdjustment => baseClass.MagicAdjustment.Add(specMagicAdjustment)    ;
    public override ClassStatBuffLevel SkillAdjustment => baseClass.SkillAdjustment.Add(specSkillAdjustment);
    public override ClassStatBuffLevel SpeedAdjustment => baseClass.SpeedAdjustment.Add(specSpeedAdjustment);
    public override ClassStatBuffLevel DefenseAdjustment => baseClass.DefenseAdjustment.Add(specDefenseAdjustment);
    public override ClassStatBuffLevel ResistanceAdjustment => baseClass.ResistanceAdjustment.Add(specResistanceAdjustment);
}