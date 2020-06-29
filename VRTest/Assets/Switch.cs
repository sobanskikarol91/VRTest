using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class Switch : GrabbableEvents
{
    public override void OnBecomesClosestGrabbable(ControllerHand touchingHand)
    {
        Debug.Log("BC");
        base.OnBecomesClosestGrabbable(touchingHand);
    }
}
