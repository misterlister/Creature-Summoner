using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionBase : Talent
{
    [SerializeField] CreatureType type;
    [SerializeField] ActionCategory category;
    [SerializeField] int power = 40;
    [SerializeField] int accuracy = 90;
    [SerializeField] bool ranged;
    [SerializeField] bool offensive = true;
    [SerializeField] bool preparation = false;
    [SerializeField] int numTargets = 1;
    [SerializeField] List<string> tags;

    public CreatureType Type => type;
    public ActionCategory Category => category;
    public int Power => power;
    public int Accuracy => accuracy;
    public bool Ranged => ranged;
    public bool Offensive => offensive;
    public bool Preparation => preparation;
    public int NumTargets => numTargets;
    public List<string> Tags => tags;
}