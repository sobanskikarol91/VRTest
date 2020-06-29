using System;
using BNG;
using UnityEngine;

public class CloseBottleSnapEffect : GrabbableEvents, ISnapEffect
{
    [SerializeField] Transform aboveCap;
    private Grabbable grabbable;


    public event Action OnCompletedAnimation;

    public void SnapEffect(Grabbable subject)
    {
        grabbable = subject;
        Move();
    }

    private void Move()
    {
        grabbable.transform.SetParent(transform, true);
        iTween.MoveTo(grabbable.gameObject, iTween.Hash("position", aboveCap.localPosition, "time", 0.4f, "islocal", true, "oncomplete", "StartMovingFinal", "oncompletetarget", gameObject, "easetype", iTween.EaseType.easeInOutCirc));
        iTween.RotateTo(grabbable.gameObject, iTween.Hash("rotation", new Vector3(0, 0, 90), "time", 0.4f, "easetype", iTween.EaseType.linear, "islocal", true));
        grabbable.GetComponentInChildren<Rigidbody>().isKinematic = true;

        grabbable.GetComponent<Collider>().enabled = false;
        grabbable.GetComponent<Collider>().isTrigger = true;
    }

    void StartMovingFinal()
    {
        iTween.MoveTo(grabbable.gameObject, iTween.Hash("position", Vector3.zero, "time", 0.6f, "islocal", true));
        iTween.RotateTo(grabbable.gameObject, iTween.Hash("rotation", new Vector3(180, 0, 90), "time", 0.6f, "easetype", iTween.EaseType.linear, "islocal", true));
    }

    void MoveComplete()
    {
        OnCompletedAnimation?.Invoke();
    }

}
