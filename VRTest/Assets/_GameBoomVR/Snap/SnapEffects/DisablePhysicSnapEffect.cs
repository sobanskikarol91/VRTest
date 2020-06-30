using System;
using BNG;
using UnityEngine;

public class DisablePhysicSnapEffect : MonoBehaviour, ISnapOnRelease
{

    public void Snap(Grabbable subject)
    {
        Collider[] disabledColliders = subject.GetComponentsInChildren<Collider>();

        Rigidbody rb = subject.GetComponent<Rigidbody>();

        if (rb) rb.isKinematic = true;
        Array.ForEach(disabledColliders, d => d.enabled = false);
    }
}
