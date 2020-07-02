using BNG;
using UnityEngine;

public class CloseBottleAnimation : GrabbableEvents, ISnapAreaEnter, ISnapAreaExit, ISnapOnRelease, ISnapOnBeginning
{
    [SerializeField] Transform prepareToReleasePos;

    private Grabbable item, ownGrabbable;
    private readonly float prepareToSpinTime = .2f;
    private readonly float spinTime = .5f;


    protected override void Awake()
    {
        ownGrabbable = GetComponent<Grabbable>();
        base.Awake();
    }

    public void SnapEnter(Grabbable subject)
    {
        item = subject;
        MoveToTheBottle();
    }

    public void SnapExit(Grabbable subject)
    {
        item = subject;
    }

    void MoveToTheBottle()
    {
        Debug.Log("Move");
        item.transform.SetParent(transform, true);
        iTween.MoveTo(item.gameObject, iTween.Hash("position", prepareToReleasePos.localPosition, "time", prepareToSpinTime, "islocal", true, "easetype", iTween.EaseType.easeInOutCirc, "oncomplete", nameof(PrepareAnimCompleted), "oncompletetarget", gameObject));
        iTween.RotateTo(item.gameObject, iTween.Hash("rotation", Vector3.zero, "time", prepareToSpinTime, "easetype", iTween.EaseType.linear, "islocal", true));

        item.GetComponent<Rigidbody>().isKinematic = true;
        item.GetComponentInChildren<Collider>().enabled = false;
        item.GetComponent<Grabbable>().enabled = false;
        ownGrabbable.enabled = false;
    }

    public void OnRelease(Grabbable subject)
    {
        CloseCap();
    }

    void CloseCap()
    {

        Debug.Log("Spin");
        ownGrabbable.enabled = false;
        iTween.MoveTo(item.gameObject, iTween.Hash("position", Vector3.zero, "time", spinTime, "islocal", true, "oncomplete", nameof(CloseAnimCompleted), "oncompletetarget", gameObject));
        iTween.RotateTo(item.gameObject, iTween.Hash("rotation", new Vector3(0, 180, 0), "time", spinTime, "easetype", iTween.EaseType.linear, "islocal", true));
    }

    void CloseAnimCompleted()
    {
        ownGrabbable.enabled = true;
        Debug.Log("SpinAnimCompleted");
    }

    void PrepareAnimCompleted()
    {
        ownGrabbable.enabled = true;
        Debug.Log("PrepareAnimCompleted");
    }

    public void Init(Grabbable grabbable)
    {
        this.item = grabbable;
    }

}