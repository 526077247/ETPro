using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RaycastHelper
{
    public static bool CastMapPoint(Camera camera, out Vector3 hitPoint)
    {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500, 1 << LayerMask.NameToLayer("Map")))
        {
            hitPoint = hit.point;
            return true;
        }
        hitPoint = Vector3.zero;
        return false;
    }

    public static bool CastUnitObj(Camera camera, out GameObject castObj)
    {
        var ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500, 1 << LayerMask.NameToLayer("Unit")))
        {
            castObj = hit.collider.gameObject;
            return true;
        }
        castObj = null;
        return false;
    }
    
}
