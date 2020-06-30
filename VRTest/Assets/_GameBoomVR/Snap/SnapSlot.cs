using BNG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SnapState { None, IsWaiting, Snapped }

public class SnapSlot : GrabbableEvents
{
    public event Action snapCompleted;
    public bool IsEmpty => !IsFull;
    public bool IsFull => grabbable;
    private bool isGrabberInRange;

    private ISnapCondition[] snapConditions;
    private IUnSnapCondition[] unsnapConditions;
    private ISnapAreaExit[] snapAreaExits;
    private ISnapOnRelease[] snapReleases;
    private IUnsnap[] unsnaps;
    private ISnapAreaEnter[] snapAreaEnters;
    private ISnapNotUsed[] snapNotUsed;
    private GrabbablesInTrigger grabbableInTrigger;
    private Grabbable snappedItem;
    private Grabbable grabbable;
    private Grabber grabber;
    private SnapState snapState;


    private void Awake()
    {
        FindReferences();
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

        if (AreSnapConditionsMet())
            EnterToSnapArea();

        isGrabberInRange = true;
    }

    public override void OnNoLongerClosestGrabbable(object sender, GrabbableEventArgs e)
    {
        e.grabber.Drop -= OnGrabRelease;

        if (snapState == SnapState.IsWaiting)
            SnapNotUsed();

        isGrabberInRange = false;
    }

    public override void OnGrab(Grabber grabber)
    {
        if (snappedItem == null) return;

        Unsnap();
    }

    public void OnGrabRelease(GrabbableEventArgs args)
    {
        if (grabbable == null) return;

        Debug.Log("GrabRelease: " + args.grabber.HeldGrabbable);
        snapState = SnapState.Snapped;
        Array.ForEach(snapReleases, s => s.OnRelease(grabbable));
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
        snapNotUsed = GetComponents<ISnapNotUsed>();
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
        snapState = SnapState.IsWaiting;
        Array.ForEach(snapAreaEnters, s => s.SnapEnter(grabbable));
        snappedItem = grabbable;
    }

    private void OnSnapCompleted()
    {
        snapCompleted?.Invoke();
    }

    void SnapNotUsed()
    {
        Debug.Log("SnapNotUsed");
        grabber.GrabGrabbable(snappedItem);
        snappedItem = null;
        snapState = SnapState.None;
    }
}
