using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LearnableAction
{
    [SerializeField] ActionBase actionBase;
    [SerializeField] int level;

    public ActionBase Action => actionBase;
    public int Level => level;

}