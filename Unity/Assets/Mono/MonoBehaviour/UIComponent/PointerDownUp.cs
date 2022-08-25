using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerDownUp : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public UnityEvent onPressDown = new UnityEvent();
    public UnityEvent onPressUp = new UnityEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        onPressDown.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPressUp.Invoke();
    }
}
