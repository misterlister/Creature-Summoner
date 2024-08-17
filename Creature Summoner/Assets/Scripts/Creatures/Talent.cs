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

public class EmpoweredAction
{
    public EmpoweredActionBase Action { get; set; }

    public EmpoweredAction(EmpoweredActionBase empoweredActionBase)
    {
        Action = empoweredActionBase;
    }
}