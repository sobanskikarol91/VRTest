using BNG;
using System;
using System.Linq;
using UnityEngine;

public enum SnapState { None, IsWaitingForRelease, Snapped }

public class SnapSlot : GrabbableEvents
{
    public bool IsEmpty => !IsFull;
    public bool IsFull => snappedItem;


    private ISnapCondition[] snapConditions;
    private IUnSnapCondition[] unsnapConditions;
    private ISnapAreaExit[] snapAreaExits;
    private ISnapOnRelease[] snapReleases;
    private IUnsnap[] unsnaps;
    private ISnapAreaEnter[] snapAreaEnters;
    private ISnapCanceled[] snapCalceled;
    private ISnapOnBeginning[] snapBegining;

    private GrabbablesInTrigger grabbableInTrigger;
    private Grabbable iteamReadyToSnap;
    private Grabber grabber;

    [SerializeField] Grabbable snappedItem;
    [SerializeField] SnapState snapState;

    protected override void Awake()
    {
        base.Awake();
        FindReferences();
        SnapOnBegining();
    }

    void SnapOnBegining()
    {
        if (snappedItem == null) return;

        snappedItem.GetComponent<Rigidbody>().isKinematic = true;
        snappedItem.GetComponentInChildren<Collider>().enabled = false;
        snappedItem.GetComponent<Grabbable>().enabled = false;
        snappedItem.transform.SetParent(transform);
        snappedItem.transform.localPosition = Vector3.zero;

        iteamReadyToSnap = snappedItem;
        snapState = SnapState.Snapped;
        Array.ForEach(snapBegining, s => s.Init(iteamReadyToSnap));
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
        snapBegining = GetComponents<ISnapOnBeginning>();
    }

    public override void OnGrab(Grabber grabber)
    {
        this.grabber = grabber;
        if (IsFull && AreUnsnapConditionsMet())
            Unsnap();
    }

    public void OnGrabRelease(GrabbableEventArgs args)
    {
        Debug.Log("GrabRelease: " + args.grabber.HeldGrabbable);
        snapState = SnapState.Snapped;
        Array.ForEach(snapReleases, s => s.OnRelease(args.grabbable));
    }

    private bool AreSnapConditionsMet()
    {
        Debug.Log("AreSnapConditionsMet : " + snapConditions.All(s => s.ShouldSnap(iteamReadyToSnap)));
        return snapConditions.All(s => s.ShouldSnap(iteamReadyToSnap));
    }

    protected bool AreUnsnapConditionsMet()
    {
        Debug.Log("Unsnap requrements:" + unsnapConditions.All(s => s.ShouldUnsnap(iteamReadyToSnap)));
        return unsnapConditions.All(s => s.ShouldUnsnap(iteamReadyToSnap));
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

        snappedItem.GetComponent<Rigidbody>().isKinematic = true;
        snappedItem.GetComponentInChildren<Collider>().enabled = false;
        snappedItem.GetComponent<Grabbable>().enabled = false;

        //grabber.TryRelease();
        grabber.GrabGrabbable(snappedItem);
        snappedItem = null;
        snapState = SnapState.None;
    }

    void Snap()
    {
        snapState = SnapState.IsWaitingForRelease;
        Array.ForEach(snapAreaEnters, s => s.SnapEnter(iteamReadyToSnap));
        snappedItem = iteamReadyToSnap;
    }

    void SnapCanceled(GrabbableEventArgs args)
    {
        Debug.Log("SnapNotUsed");
        grabber.GrabGrabbable(snappedItem);
        Array.ForEach(snapCalceled, s => s.SnapCanceled(args));
        snappedItem = null;
        snapState = SnapState.None;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter Triger: " + other.gameObject.name, other.gameObject);

        grabber = GetComponent<Grabber>();
        if (grabber == null) return;

        iteamReadyToSnap = grabber.HeldGrabbable;
        grabber.Drop += OnGrabRelease;

        if (iteamReadyToSnap == null)
            Debug.Log("Held item is null");

        if (iteamReadyToSnap && AreSnapConditionsMet())
            EnterToSnapArea();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit Triger: " + other.gameObject.name, other.gameObject);

        Grabber grabber = GetComponent<Grabber>();
        if (grabber == null) return;

        grabber.Drop -= OnGrabRelease;

        if (snapState == SnapState.IsWaitingForRelease)
            SnapCanceled(null);
    }
}
