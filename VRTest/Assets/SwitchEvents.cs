using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class SwitchEvents : MonoBehaviour
{
    Grabbable grabbable;

    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();    
    }

    void Rotate()
    {

    }
}
