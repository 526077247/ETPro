using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 双击事件
/// </summary>
public class PointerDoubleClick : MonoBehaviour,IPointerDownHandler
{
    public UnityEvent onClick = new UnityEvent();

    public float Interval = 0.5f;

    private float firstClicked = 0;
    private float secondClicked = 0;

    public void OnPointerDown(PointerEventData eventData)
    {
        secondClicked = Time.realtimeSinceStartup;

        if (secondClicked - firstClicked < Interval)
        {
            onClick.Invoke();
        }
        else
        {
            firstClicked = secondClicked;
        }
    }

}
