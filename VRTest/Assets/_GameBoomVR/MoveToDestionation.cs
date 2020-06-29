using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToDestionation : MonoBehaviour
{
    [SerializeField] GameObject target;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Move();
    }

    private void Move()
    {
        throw new NotImplementedException();
    }
}
