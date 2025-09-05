using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAction
{
    public ActionBase Action { get; set; }

    public CreatureAction(ActionBase actionBase)
    {
        Action = actionBase;
    }
}

