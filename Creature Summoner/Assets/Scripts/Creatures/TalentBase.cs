using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Talent : ScriptableObject
{
    [SerializeField] string talentName;
    [TextArea]
    [SerializeField] string description;

    public string TalentName => talentName;
    public string Description => description;
}

public abstract class ActionBase : Talent
{
    [SerializeField] CreatureType type;
    [SerializeField] ActionCategory category;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool ranged;
    [SerializeField] bool offensive;
    [SerializeField] int targetNum;
    [SerializeField] List<string> tags;

    public CreatureType Type => type;
    public ActionCategory Category => category;
    public int Power => power;
    public int Accuracy => accuracy;
    public bool Ranged => ranged;
    public bool Offensive => offensive;
    public int TargetNum => targetNum;
    public List<string> Tags => tags;
}

[CreateAssetMenu(fileName = "CoreAction", menuName = "Talents/Create new Core Action")]
public class CoreActionBase : ActionBase
{
    [SerializeField] int energyGain;

    public int EnergyGain => energyGain;
}

[CreateAssetMenu(fileName = "EmpoweredAction", menuName = "Talents/Create new Empowered Action")]
public class EmpoweredActionBase : ActionBase
{
    [SerializeField] int energyCost;

    public int EnergyCost => energyCost;
}


public enum ActionCategory
{
    None,
    Physical,
    Magical
}
