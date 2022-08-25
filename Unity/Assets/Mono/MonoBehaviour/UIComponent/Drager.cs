using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityEngine.Events
{
    [System.Serializable]
    public class UIPointerEvent : UnityEvent<PointerEventData> { }
    public class UIRaycastEvent : UnityEvent<List<RaycastResult>> { }
}

public class Drager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private bool isDraging = false;
    public UIPointerEvent onBeginDrag = new UIPointerEvent();
    public UIPointerEvent onDrag = new UIPointerEvent();
    public UIPointerEvent onEndDrag = new UIPointerEvent();
    //这里需要区分一下 当前是click还是drag
    public UIPointerEvent onClick = new UIPointerEvent();
    public UIRaycastEvent onEndDragRaycastAll = new UIRaycastEvent();
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDraging)
        {
            onClick.Invoke(eventData);
        }
    }

    // begin draggin
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDraging = true;
        onBeginDrag.Invoke(eventData);
    }

    // during dragging
    public void OnDrag(PointerEventData eventData)
    {
        isDraging = true;
        onDrag.Invoke(eventData);
    }

    // end dragging
    public void OnEndDrag(PointerEventData eventData)
    {
        isDraging = false;
        onEndDrag.Invoke(eventData);
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        GraphicRaycaster raycaster = GetComponentInParent<GraphicRaycaster>();
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);
        onEndDragRaycastAll.Invoke(results);
    }  

}

