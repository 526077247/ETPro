using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class PointerMask : MonoBehaviour,IPointerClickHandler
{
    public GameObject Target;
    public void OnPointerClick(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerClickHandler);
    }

    private void PassEvent<T>(PointerEventData pointerEventData, ExecuteEvents.EventFunction<T> eventFunction)
            where T : IEventSystemHandler
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject == Target)
            {
                ExecuteEvents.Execute(results[i].gameObject, pointerEventData, eventFunction);
            }
        }
    }
}
