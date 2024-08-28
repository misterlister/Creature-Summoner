using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmpoweredAction : IAction
{
    public EmpoweredActionBase Action { get; set; }
    public ActionBase BaseAction { get; set; }

    public EmpoweredAction(EmpoweredActionBase empoweredActionBase)
    {
        Action = empoweredActionBase;
        BaseAction = empoweredActionBase;
    }
}