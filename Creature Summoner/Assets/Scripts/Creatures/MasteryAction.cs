using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasteryAction : IAction
{
    public MasteryActionBase Action { get; set; }
    public ActionBase BaseAction { get; set; }

    public MasteryAction(MasteryActionBase masteryActionBase)
    {
        Action = masteryActionBase;
        BaseAction = masteryActionBase;
    }
}
