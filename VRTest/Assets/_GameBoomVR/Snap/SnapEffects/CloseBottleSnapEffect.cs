using System;
using BNG;
using UnityEngine;

public class CloseBottleSnapEffect : GrabbableEvents, ISnapAreaEnter, ISnapAreaExit, ISnapOnRelease, IUnsnap
{
    [SerializeField] Transform aboveCap;
    private Grabbable grabbable;


    public void SnapEnter(Grabbable subject)
    {
        grabbable = subject;
        MoveToTheBottle();
    }

    public void SnapExit(Grabbable subject)
    {
        grabbable = subject;
    }

    void MoveToTheBottle()
    {
        Debug.Log("Move");
        grabbable.transform.SetParent(transform, true);
        iTween.MoveTo(grabbable.gameObject, iTween.Hash("position", aboveCap.localPosition, "time", 0.4f, "islocal", true, "easetype", iTween.EaseType.easeInOutCirc));
        iTween.RotateTo(grabbable.gameObject, iTween.Hash("rotation", new Vector3(0, 0, 90), "time", 0.4f, "easetype", iTween.EaseType.linear, "islocal", true));

        grabbable.GetComponent<Rigidbody>().isKinematic = true;
        grabbable.GetComponent<Collider>().enabled = false;
        grabbable.GetComponent<Collider>().isTrigger = true;
    }

    public void OnRelease(Grabbable subject)
    {
        grabbable.transform.SetParent(transform, true);
        grabbable.GetComponent<Rigidbody>().isKinematic = true;
        grabbable.GetComponent<Collider>().enabled = false;
        grabbable.GetComponent<Collider>().isTrigger = true;
        grabbable.GetComponent<Grabbable>().enabled = false;

        SpinCap();
    }

    void SpinCap()
    {
        Debug.Log("Spin");
        iTween.MoveTo(grabbable.gameObject, iTween.Hash("position", Vector3.zero, "time", 0.6f, "islocal", true));
        iTween.RotateTo(grabbable.gameObject, iTween.Hash("rotation", new Vector3(180, 0, 90), "time", 0.6f, "easetype", iTween.EaseType.linear, "islocal", true));
    }

    public void OnUnsnap(GrabbableEventArgs subject)
    {
        grabbable.GetComponent<Rigidbody>().isKinematic = false;
        grabbable.GetComponent<Collider>().enabled = true;
        grabbable.GetComponent<Collider>().isTrigger = false;
        grabbable.GetComponent<Grabbable>().enabled = true;
    }
}