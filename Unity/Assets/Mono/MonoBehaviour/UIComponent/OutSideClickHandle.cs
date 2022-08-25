using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OutSideClickHandle : MonoBehaviour
{
    public Action outsideClick;

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
            if (IsOutSideClick()) 
                outsideClick?.Invoke();
    }

    bool IsOutSideClick()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        for (int i = 0; i < results.Count; i++)
            if (results[i].gameObject == gameObject)
                return false;
        return true;
    }

    public void AddListener(Action action)
    {
        outsideClick = action;
    }

    public void RemoveListener()
    {
        outsideClick = null;
    }
}