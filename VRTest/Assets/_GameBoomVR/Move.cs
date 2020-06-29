using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform destinationAbove;
    [SerializeField] Transform destinationFinal;
    [SerializeField] new iTween.EaseType animation;
    [SerializeField] bool nextAnim = true;
    [SerializeField] float time = 1f;
    [SerializeField] float insertTime = 0.2f;
    Vector2 originPos;
    int animIndex = 0;

    private void Awake()
    {
        originPos = target.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            StartMovingAbove();
    }

    private void StartMovingAbove()
    {
        Debug.Log(animation);
        iTween.MoveTo(target.gameObject, iTween.Hash("position", destinationAbove.position, "time", time, "easetype", animation, "oncomplete", "StartMovingFinal", "oncompletetarget", gameObject));
    }

    private void StartMovingFinal()
    {
        iTween.MoveTo(target.gameObject, iTween.Hash("position", destinationFinal.position, "time", insertTime, "easetype", animation, "oncomplete", "Oncomplete", "oncompletetarget", gameObject));
    }

    private void Oncomplete()
    {
        if (nextAnim)
        {
            animIndex++;
            animation = (iTween.EaseType)(animIndex);
        }

        Invoke("Reset", 0.4f);
    }

    private void Reset()
    {
        target.position = originPos;
        StartMovingAbove();
    }
}
