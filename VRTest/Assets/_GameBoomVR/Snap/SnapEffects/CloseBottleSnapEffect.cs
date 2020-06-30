using System;
using BNG;
using UnityEngine;

public class CloseBottleSnapEffect : GrabbableEvents, ISnapAreaEnter, ISnapAreaExit, ISnapOnRelease, IUnsnap
{
    [SerializeField] Transform aboveCap;
    private Grabbable grabbable;
    private readonly float prepareToSpinTime = .2f;
    private readonly float spinTime = .5f;
    private new Collider collider;


    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

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
        iTween.MoveTo(grabbable.gameObject, iTween.Hash("position", aboveCap.localPosition, "time", prepareToSpinTime, "islocal", true, "easetype", iTween.EaseType.easeInOutCirc));
        iTween.RotateTo(grabbable.gameObject, iTween.Hash("rotation", new Vector3(0, 0, 90), "time", prepareToSpinTime, "easetype", iTween.EaseType.linear, "islocal", true));

        grabbable.GetComponent<Rigidbody>().isKinematic = true;
        grabbable.GetComponent<Collider>().enabled = false;
        grabbable.GetComponent<Grabbable>().enabled = false;
    }

    public void OnRelease(Grabbable subject)
    {
        grabbable.transform.SetParent(transform, true);
        SpinCap();
    }

    void SpinCap()
    {
        Debug.Log("Spin");
        collider.enabled = false;
        GetComponent<Grabbable>().enabled = false;
        iTween.MoveTo(grabbable.gameObject, iTween.Hash("position", Vector3.zero, "time", spinTime, "islocal", true, "oncomplete", "SpinAnimCompleted", "oncompletetarget", gameObject));
        iTween.RotateTo(grabbable.gameObject, iTween.Hash("rotation", new Vector3(180, 0, 90), "time", spinTime, "easetype", iTween.EaseType.linear, "islocal", true));
    }

    public void OnUnsnap(GrabbableEventArgs subject)
    {
        grabbable.GetComponent<Rigidbody>().isKinematic = false;
        grabbable.GetComponent<Collider>().enabled = true;
        grabbable.GetComponent<Collider>().isTrigger = false;
        grabbable.GetComponent<Grabbable>().enabled = true;
    }

    void SpinAnimCompleted()
    {
        collider.enabled = true;
        GetComponent<Grabbable>().enabled = true;
        Debug.Log("SpinAnimCompleted");
    }
}