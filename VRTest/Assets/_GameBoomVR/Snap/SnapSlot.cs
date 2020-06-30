using BNG;
using System;
using System.Linq;
using UnityEngine;

public enum SnapState { None, IsWaitingForRelease, Snapped }

public class SnapSlot : GrabbableEvents
{
    public bool IsEmpty => !IsFull;
    public bool IsFull => snappedItem;
    private bool isGrabberInRange;

    private ISnapCondition[] snapConditions;
    private IUnSnapCondition[] unsnapConditions;
    private ISnapAreaExit[] snapAreaExits;
    private ISnapOnRelease[] snapReleases;
    private IUnsnap[] unsnaps;
    private ISnapAreaEnter[] snapAreaEnters;
    private ISnapCanceled[] snapCalceled;
    private SnapState snapState;
    private GrabbablesInTrigger grabbableInTrigger;
    private Grabbable grabbable, snappedItem;
    private Grabber grabber;


    private void Awake()
    {
        FindReferences();
    }

    private void FindReferences()
    {
        grabbableInTrigger = GetComponent<GrabbablesInTrigger>();
        snapConditions = GetComponents<ISnapCondition>();
        unsnapConditions = GetComponents<IUnSnapCondition>();
        snapReleases = GetComponents<ISnapOnRelease>();
        snapAreaExits = GetComponents<ISnapAreaExit>();
        snapAreaEnters = GetComponents<ISnapAreaEnter>();
        unsnaps = GetComponents<IUnsnap>();
        snapCalceled = GetComponents<ISnapCanceled>();
    }

    public override void OnBecomesClosestGrabbable(object sender, GrabbableEventArgs e)
    {
        if (isGrabberInRange)
        {
            Debug.Log("isGrabberInRange");
            return;
        }

        Debug.Log("OnBecomesClosestGrabbable: ");

        grabbable = e.grabber.HeldGrabbable;
        grabber = e.grabber;
        grabber.Drop += OnGrabRelease;

        if (grabbable && AreSnapConditionsMet())
            EnterToSnapArea();

        isGrabberInRange = true;
    }

    public override void OnNoLongerClosestGrabbable(object sender, GrabbableEventArgs e)
    {
        e.grabber.Drop -= OnGrabRelease;

        if (snapState == SnapState.IsWaitingForRelease)
            SnapCanceled(e);

        isGrabberInRange = false;
    }

    public override void OnGrab(Grabber grabber)
    {
        if (IsFull && AreUnsnapConditionsMet())
            Unsnap();
    }

    public void OnGrabRelease(GrabbableEventArgs args)
    {
        if (grabbable == null) return;

        Debug.Log("GrabRelease: " + args.grabber.HeldGrabbable);
        snapState = SnapState.Snapped;
        Array.ForEach(snapReleases, s => s.OnRelease(grabbable));
    }

    private bool AreSnapConditionsMet()
    {
        Debug.Log("AreSnapConditionsMet : " + snapConditions.All(s => s.ShouldSnap(grabbable)));
        return snapConditions.All(s => s.ShouldSnap(grabbable));
    }

    protected bool AreUnsnapConditionsMet()
    {
        return unsnapConditions.All(s => s.ShouldUnsnap(grabbable));
    }

    private void EnterToSnapArea()
    {
        Debug.Log("Snap AreaEnter");
        Snap();
    }

    private void Unsnap()
    {
        Debug.Log("Unsnap");
        Array.ForEach(unsnaps, s => s.OnUnsnap(null));
        grabber.TryRelease();
        grabber.GrabGrabbable(snappedItem);
        snappedItem = null;
        snapState = SnapState.None;
    }

    void Snap()
    {
        snapState = SnapState.IsWaitingForRelease;
        Array.ForEach(snapAreaEnters, s => s.SnapEnter(grabbable));
        snappedItem = grabbable;
    }

    void SnapCanceled(GrabbableEventArgs args)
    {
        Debug.Log("SnapNotUsed");
        grabber.GrabGrabbable(snappedItem);
        Array.ForEach(snapCalceled, s => s.SnapCanceled(args));
        snappedItem = null;
        snapState = SnapState.None;
    }
}
