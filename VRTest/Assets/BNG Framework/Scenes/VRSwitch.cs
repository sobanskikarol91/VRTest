using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSwitch : GrabbableUnityEvents
{
    protected  void Awake()
    {

        onTriggerDown.AddListener(OnTriggerDown);
        onGrab.AddListener(OnGrab);
        onGrip.AddListener(OnGrip);
        onBecomesClosestGrabbable.AddListener(OnBecomeClosest);
    }

    public override void OnTriggerDown()
    {
        Debug.Log("OnTriggerDown");
    }

    void OnGrab()
    {
        Debug.Log("OnGrab");
    }

    void OnGrip()
    {
        Debug.Log("OnGrip");
    }

    void OnBecomeClosest()
    {
        Debug.Log("BecomeClosest");
    }
}
