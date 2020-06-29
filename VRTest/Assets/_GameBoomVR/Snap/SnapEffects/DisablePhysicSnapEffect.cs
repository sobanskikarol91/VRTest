using System;
using BNG;
using UnityEngine;

public class DisablePhysicSnapEffect : MonoBehaviour, ISnapEffect
{
    public event Action OnCompletedAnimation;
    
    public void SnapEffect(Grabbable subject)
    {
        Collider[] disabledColliders = subject.GetComponentsInChildren<Collider>();

        subject.GetComponent<Rigidbody>().isKinematic = true;
        Array.ForEach(disabledColliders, d => d.enabled = false);
    }
}
