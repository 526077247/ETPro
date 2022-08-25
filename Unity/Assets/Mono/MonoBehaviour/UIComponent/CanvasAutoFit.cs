using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ET;
using UnityEngine.UI;

public class CanvasAutoFit : MonoBehaviour
{
    private CanvasScaler canvas;
    // Start is called before the first frame update
    void Start()
    {
        this.canvas = this.GetComponent<CanvasScaler>();
        //屏幕缩放比
        var CS_SCREEN_H = Screen.height;
        var CS_SCREEN_W = Screen.width;
        var flagx = (float)Define.DesignScreen_Width / Define.DesignScreen_Height;
        var flagy = (float)CS_SCREEN_W / CS_SCREEN_H;
        if (flagx < flagy)
            canvas.matchWidthOrHeight = 1;
        else
            canvas.matchWidthOrHeight = 0;
    }
    
}
