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
