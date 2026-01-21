using System.Collections.Generic;
using UnityEngine;
using Game.Traits;

[CreateAssetMenu(fileName = "NewCreatureClass", menuName = "Classes/Create new Base Class")]
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

[CreateAssetMenu(fileName = "CreatureClass", menuName = "Classes/Create new Specialized Class")]
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
    public override ClassStatBuffLevel MagicAdjustment => baseClass.MagicAdjustment.Add(specMagicAdjustment);
    public override ClassStatBuffLevel SkillAdjustment => baseClass.SkillAdjustment.Add(specSkillAdjustment);
    public override ClassStatBuffLevel SpeedAdjustment => baseClass.SpeedAdjustment.Add(specSpeedAdjustment);
    public override ClassStatBuffLevel DefenseAdjustment => baseClass.DefenseAdjustment.Add(specDefenseAdjustment);
    public override ClassStatBuffLevel ResistanceAdjustment => baseClass.ResistanceAdjustment.Add(specResistanceAdjustment);
}

[System.Serializable]
public class CreatureClassInstance : ISerializationCallbackReceiver
{
    [SerializeField] CreatureClass creatureClass;
    [SerializeField] int classLevel;
    [SerializeField] int classXP;
    public int ClassXP => classXP;
    public int ClassLevel => classLevel;
    public CreatureClass CreatureClass => creatureClass;

    public int XPForNextLevel => XPSystem.GetXPForNextClassLevel(classLevel);
    public float XPProgress => (float)classXP / XPForNextLevel;

    public event System.Action<int> onClassLevelUp;

    public CreatureClassInstance() { }
    public CreatureClassInstance(CreatureClass creatureClass, int classLevel = 1, int classXP = 0)
    {
        this.creatureClass = creatureClass;
        this.classLevel = classLevel;
        this.classXP = classXP;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() 
    { 
        if (classLevel < 1) classLevel = 1;
    }

    public bool AddClassXP(int xp)
    {
        bool leveledUp = false;
        classXP += xp;
        while (classXP >= XPForNextLevel && classLevel < GameConstants.MAX_CLASS_LEVEL)
        {
            classXP -= XPForNextLevel;
            LevelUp();
            leveledUp = true;
        }
        return leveledUp;
    }

    public void LevelUp()
    {

        classLevel++;
        onClassLevelUp?.Invoke(classLevel);
    }

    public int GetStatModifier(StatType stat)
    {
        if (creatureClass == null)
        {
            return 0;
        }

        switch (stat) 
            {
            case StatType.HP:
                return creatureClass.HpAdjustment.GetValue() * classLevel;
            case StatType.Energy:
                return creatureClass.EnergyAdjustment.GetValue() * classLevel;
            case StatType.Strength:
                return creatureClass.StrengthAdjustment.GetValue() * classLevel;
            case StatType.Magic:
                return creatureClass.MagicAdjustment.GetValue() * classLevel;
            case StatType.Skill:
                return creatureClass.SkillAdjustment.GetValue() * classLevel;
            case StatType.Speed:
                return creatureClass.SpeedAdjustment.GetValue() * classLevel;
            case StatType.Defense:
                return creatureClass.DefenseAdjustment.GetValue() * classLevel;
            case StatType.Resistance:
                return creatureClass.ResistanceAdjustment.GetValue() * classLevel;
            default:
                return 0;
        }
    }
}