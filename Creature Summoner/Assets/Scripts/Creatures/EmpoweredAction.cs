using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpoweredAction
{
    public EmpoweredActionBase Action { get; set; }

    public EmpoweredAction(EmpoweredActionBase empoweredActionBase)
    {
        Action = empoweredActionBase;
    }
}