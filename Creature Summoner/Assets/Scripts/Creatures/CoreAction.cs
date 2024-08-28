using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreAction : IAction
{
    public CoreActionBase Action { get; set; }

    public ActionBase BaseAction { get; set; }

    public CoreAction(CoreActionBase coreActionBase)
    {
        Action = coreActionBase;
        BaseAction = coreActionBase;
    }
}

