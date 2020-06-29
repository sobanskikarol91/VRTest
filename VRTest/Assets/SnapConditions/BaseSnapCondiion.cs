using BNG;
using UnityEngine;

public class BaseSnapCondiion : MonoBehaviour, ISnapCondition
{
    private SnapSlot snapSlot;

    private void Awake()
    {
        snapSlot = GetComponent<SnapSlot>();
    }

    public bool ShouldSnap(Grabbable subject)
    {
        return snapSlot.IsEmpty && subject != null;
    }
}