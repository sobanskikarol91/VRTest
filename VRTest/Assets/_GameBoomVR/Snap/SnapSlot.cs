using BNG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnapSlot : MonoBehaviour
{
    [SerializeField] Grabbable ClosestGrabbable;

    public event Action snapCompleted;
    public Grabbable GrabbableInSlot { get; private set; }
    public bool IsEmpty => !IsFull;
    public bool IsFull => GrabbableInSlot;

    private ISnapCondition[] snapConditions;
    private IUnSnapCondition[] unsnapConditions;
    private ISnapEffect[] snapEffects;
    private ISnapAreaExit[] snapAreaExits;
    private ISnapAreaEnter[] snapAreaEnters;
    private GrabbablesInTrigger grabbableInTrigger;


    private void Awake()
    {
        FindReferences();
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        Array.ForEach(snapEffects, s => s.OnCompletedAnimation += OnSnapCompleted);
    }

    private void FindReferences()
    {
        grabbableInTrigger = GetComponent<GrabbablesInTrigger>();
        snapConditions = GetComponents<ISnapCondition>();
        unsnapConditions = GetComponents<IUnSnapCondition>();
        snapEffects = GetComponents<ISnapEffect>();
        snapAreaExits = GetComponents<ISnapAreaExit>();
        snapAreaEnters = GetComponents<ISnapAreaEnter>();
    }

    private void Update()
    {
        FindClosestGrabbableInRange();
    }

    void FindClosestGrabbableInRange()
    {
        ClosestGrabbable = grabbableInTrigger.NearbyGrabbables.FirstOrDefault().Value;

        if (AreSnapConditionsMet())
            SnapGrabbableToSlot();
    }

    private bool AreSnapConditionsMet()
    {
        return snapConditions.All(s => s.ShouldSnap(ClosestGrabbable));
    }

    protected bool AreUnsnapConditionsMet()
    {
        return unsnapConditions.All(s => s.ShouldUnsnap(ClosestGrabbable));
    }

    private void SnapGrabbableToSlot()
    {
        Array.ForEach(snapEffects, s => s.SnapEffect(ClosestGrabbable));
    }

    private void OnSnapCompleted()
    {
        snapCompleted?.Invoke();
    }

    private void OnSnapExit()
    {
        Array.ForEach(snapAreaExits, s => s.SnapExit(ClosestGrabbable));
    }

    private void OnSnapEnter()
    {
        Array.ForEach(snapAreaEnters, s => s.SnapEnter(ClosestGrabbable));
    }
}
