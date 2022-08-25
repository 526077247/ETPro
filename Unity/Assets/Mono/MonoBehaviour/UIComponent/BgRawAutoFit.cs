﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ET;
[ExecuteAlways()]
public class BgRawAutoFit : MonoBehaviour
{
    RectTransform rectTransform;
    RawImage bg;
    public Texture bgSprite;
    // Start is called before the first frame update
    private void Awake() {
        rectTransform=GetComponent<RectTransform>();
        bg=GetComponent<RawImage>();
    }
    void Start()
    {
        if(bgSprite==null)
            bgSprite=bg.texture;
        else
            bg.texture = bgSprite;
        Size();
    }
    void Size()
    {
        //屏幕缩放比
        var CS_SCREEN_H = Screen.height;
        var CS_SCREEN_W = Screen.width;
        var flagx = (float)Define.DesignScreen_Width / Define.DesignScreen_Height;
        var flagy = (float)CS_SCREEN_W / CS_SCREEN_H;
        var sign_flag = flagx > flagy ? (float)Define.DesignScreen_Width / CS_SCREEN_W : (float)Define.DesignScreen_Height / CS_SCREEN_H;
        //图片缩放比
        var cs_width = CS_SCREEN_W > CS_SCREEN_H ? CS_SCREEN_W : CS_SCREEN_H;
        var cs_height = CS_SCREEN_W < CS_SCREEN_H ? CS_SCREEN_W : CS_SCREEN_H;
        var texture = bgSprite;
        var flag1 = (float)cs_width / texture.width;
        var flag2 = (float)cs_height / texture.height;
        if (flag1 < flag2)
            rectTransform.sizeDelta = new Vector2(flag2 * texture.width * sign_flag, cs_height * sign_flag);
        else
            rectTransform.sizeDelta = new Vector2(cs_width * sign_flag, flag1 * texture.height * sign_flag);
    }

    public void SetSprite(Texture newBgSprite){
        bgSprite=newBgSprite;
        if(bgSprite==null)
            bgSprite = bg.texture;
        else
            bg.texture=bgSprite;
        Size();
    }
}
