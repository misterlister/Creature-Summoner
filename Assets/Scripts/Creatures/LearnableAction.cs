using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LearnableAction
{
    [SerializeField] ActionBase action;
    [SerializeField] int level;

    public ActionBase Action => action;
    public int Level => level;

}