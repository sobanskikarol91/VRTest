using BNG;
using UnityEngine;

public class SnapObjectFromGroup : MonoBehaviour, ISnapCondition
{
    [SerializeField] string[] snapToObjects;

    public bool ShouldSnap(Grabbable subject)
    {
        Group subjectSnapGroup = subject.GetComponent<Group>();

        if (!subjectSnapGroup) return false;

        return subjectSnapGroup.IsContaingAnyElement(snapToObjects);
    }
}
