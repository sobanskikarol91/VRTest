using System;
using BNG;
using UnityEngine;

public class RemoveReferenceFromGrabberSnapEffect : MonoBehaviour, ISnapEffect
{
    public event Action OnCompletedAnimation;

    public void SnapEffect(Grabbable subject)
    {
        subject.DropFromGrabber();
    }
}
