using BNG;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SnapState { None, IsWaitingForRelease, Snapped }

public class SnapSlot : MonoBehaviour
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

    protected void Awake()
    {
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

    public void OnGrab()
    {
        if (IsFull && AreUnsnapConditionsMet())
            Unsnap();
    }

    public void OnRelease()
    {
        if (snappedItem == null) return;

        snapState = SnapState.Snapped;
        snappedItem.transform.SetParent(transform, true);
        grabber.TryRelease();
        SetColliders(false);
        SetRigidbody(true);
        Array.ForEach(snapReleases, s => s.OnRelease(snappedItem));
    }

    private void SetRigidbody(bool isKinematic)
    {
        Rigidbody rb = snappedItem.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = isKinematic;
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

    private void Unsnap()
    {
        Debug.Log("Unsnap");
        Array.ForEach(unsnaps, s => s.OnUnsnap(null));

        SetColliders(true);
        SetRigidbody(false);
        grabber.GrabGrabbable(snappedItem);
        snappedItem = null;
        snapState = SnapState.None;
    }

    void SetColliders(bool isEnabled)
    {
        List<Collider> disabledColliders = iteamReadyToSnap.GetComponentsInChildren<Collider>(false).ToList();

        for (int x = 0; x < disabledColliders.Count; x++)
        {
            Collider c = disabledColliders[x];
            c.enabled = isEnabled;
        }

        iteamReadyToSnap.enabled = isEnabled;
    }

    void Snap()
    {
        Debug.Log("Snap");
        snapState = SnapState.IsWaitingForRelease;
        Array.ForEach(snapAreaEnters, s => s.SnapEnter(iteamReadyToSnap));
        snappedItem = iteamReadyToSnap;
    }

    void SnapCanceled(GrabbableEventArgs args)
    {
        Debug.Log("SnapCanceled");
        grabber.GrabGrabbable(snappedItem);
        Array.ForEach(snapCalceled, s => s.SnapCanceled(args));
        snappedItem = null;
        snapState = SnapState.None;
    }

    private void OnTriggerEnter(Collider other)
    {
        grabber = other.gameObject.GetComponent<Grabber>();

        if (grabber == null) return;


        Debug.Log(gameObject.name + " Enter Triger: " + other.gameObject.name, other.gameObject);

        if (grabber.HeldGrabbable == null)
        {
            Debug.Log("Held Item null Trigger Enter");
            return;
        }

        Debug.Log("Trzyma item:" + grabber.HeldGrabbable.name);
        iteamReadyToSnap = grabber.HeldGrabbable;


        if (iteamReadyToSnap == null)
            Debug.Log("Held item is null");

        if (iteamReadyToSnap && AreSnapConditionsMet())
            Snap();
    }

    private void OnTriggerExit(Collider other)
    {
        grabber = other.gameObject.GetComponent<Grabber>();
        if (grabber == null) return;
        Debug.Log(gameObject.name + " Exit Triger: " + other.gameObject.name, other.gameObject);

        if (snapState == SnapState.IsWaitingForRelease)
            SnapCanceled(null);
    }

    float currentGripForce = 0;
    float previousGripForce = 0;
    enum buttonState { increase, decrease, none };

    buttonState currentState = buttonState.decrease;
    buttonState previousState = buttonState.decrease;

    void Update()
    {
        if (grabber == null) return;

        currentGripForce = InputBridge.Instance.LeftGrip;

        if (currentGripForce > previousGripForce)
            currentState = buttonState.increase;
        else if (currentGripForce < previousGripForce)
            currentState = buttonState.decrease;

        if (currentState == buttonState.decrease && previousState == buttonState.increase)
            OnRelease();
        else if (currentState == buttonState.increase && previousState == buttonState.decrease)
            OnGrab();

        previousState = currentState;
        previousGripForce = currentGripForce;
       // Debug.Log("Current state:" + currentState + " " + InputBridge.Instance.LeftGrip);
    }
}