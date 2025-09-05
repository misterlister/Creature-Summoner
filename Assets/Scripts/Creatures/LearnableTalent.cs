using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LearnableTalent
{
    [SerializeField] Talent talentBase;
    [SerializeField] int level;

    public Talent TalentBase => talentBase;
    public int Level => level;

}