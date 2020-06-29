using BNG;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class GrabbableDetector : MonoBehaviour
{
    [SerializeField] LayerMask validMasks;

    private float detectionRadius;
    private SphereCollider rangeCollider;
    private Collider[] grabbablesColliders = new Collider[64];
    private List<Grabbable> currentFrameGrabbables = new List<Grabbable>();
    private List<Grabbable> previousFrameGrabbables = new List<Grabbable>();
    private List<Grabbable> exitGrabbables = new List<Grabbable>();
    private List<Grabbable> enterGrabbables = new List<Grabbable>();

    public event Action<Grabbable> RangeEnter;
    public event Action<Grabbable> RangeExit;
    public event Action<Grabbable> RangeStay;


    private void Awake()
    {
        FindReferences();
        SetRadiusDependsOnColliderSize();
        IgnoreSelfCollisions();
    }

    private void Update()
    {
        FindCollidersInRange();
        FindGrabbables();
        FindEnterGrabbables();
        FindExitGrabbables();
    }

    private void IgnoreSelfCollisions()
    {
        Physics.IgnoreCollision(rangeCollider, rangeCollider);
    }

    private void SetRadiusDependsOnColliderSize()
    {
        detectionRadius = rangeCollider.radius;
    }

    private void FindReferences()
    {
        rangeCollider = GetComponent<SphereCollider>();
    }

    private void FindCollidersInRange()
    {
        Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, grabbablesColliders, validMasks);
    }

    private void FindGrabbables()
    {
        Grabbable testGrabbable;

        for (int i = 0; i < grabbablesColliders.Length; i++)
        {
            if (!grabbablesColliders[i]) break;

            testGrabbable = grabbablesColliders[i].GetComponent<Grabbable>();

            if (!testGrabbable) break;

            currentFrameGrabbables.Add(testGrabbable);
        }
    }

    void FindExitGrabbables()
    {
        exitGrabbables = previousFrameGrabbables.Except(currentFrameGrabbables).ToList();
    }

    void FindEnterGrabbables()
    {
        enterGrabbables = currentFrameGrabbables.Except(previousFrameGrabbables).ToList();
    }

    void OnRangeEnter(Grabbable grabbable)
    {
        RangeEnter?.Invoke(grabbable);
    }

    void OnRangeExit(Grabbable grabbable)
    {
        RangeExit?.Invoke(grabbable);
    }

    void OnRangeStay(Grabbable grabbable)
    {
        RangeStay?.Invoke(grabbable);
    }
}
