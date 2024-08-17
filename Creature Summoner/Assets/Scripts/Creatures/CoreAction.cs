using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreAction
{
    public CoreActionBase Action { get; set; }

    public CoreAction(CoreActionBase coreActionBase)
    {
        Action = coreActionBase;
    }
}

