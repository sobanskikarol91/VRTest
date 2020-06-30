using BNG;
using UnityEngine;

public class UnsnapIfItemIsHeld : MonoBehaviour, IUnSnapCondition
{
    [SerializeField] Grabbable reqiredHeldItem;

    public bool ShouldUnsnap(Grabbable subject)
    {
        return reqiredHeldItem.BeingHeld;
    }
}
