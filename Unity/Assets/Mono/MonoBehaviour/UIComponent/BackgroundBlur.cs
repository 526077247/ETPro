using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ET
{
    /// <summary>
    /// UI弹窗后面的背景截图
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class BackgroundBlur : MonoBehaviour
    {
        private RawImage rImage;

        private static Texture2D screenShotTemp;
        public static int RefCount = 0;//引用次数

        private void Awake()
        {
            rImage = this.GetComponent<RawImage>();
        }
        
        private void OnEnable()
        {
            StartCoroutine(Snapshoot());
        }

        private IEnumerator Snapshoot()
        {
            if (rImage == null)
            {
                Log.Warning("Background Blur is warring !!!");
                yield return null;
            }
            else
            {
                yield return ReadPixels();
            }
            
        }

        private IEnumerator ReadPixels()
        {
            rImage.enabled = false;
            while (RefCount>0 && screenShotTemp == null)
            {
                yield return new WaitForEndOfFrame();
            }
            RefCount++;
            if (screenShotTemp == null)
            {
                GameObject uiCamera = null;
                var cd = Camera.main.GetUniversalAdditionalCameraData();
                for (int i = 0; i < cd.cameraStack.Count; i++)
                {
                    if (cd.cameraStack[i].gameObject.layer == LayerMask.NameToLayer("UI"))
                    {
                        uiCamera = cd.cameraStack[i].gameObject;
                        break;
                    }
                }
                uiCamera?.SetActive(false);
                yield return new WaitForEndOfFrame();
                if (RefCount > 0)//防止等一帧回来已经被关了
                {
                    // 先创建一个的空纹理，大小可根据实现需要来设置  
                    var rect = new Rect(0, 0, Screen.width, Screen.height);
                    screenShotTemp = new Texture2D((int) rect.width, (int) rect.height, TextureFormat.RGB24, false);
                    // 读取屏幕像素信息并存储为纹理数据，  
                    screenShotTemp.ReadPixels(rect, 0, 0);
                    screenShotTemp.Apply();
                    // todo: 调用shader模糊
                }
                uiCamera?.SetActive(true);
            }
            
            rImage.enabled = true;
            rImage.texture = screenShotTemp;
        }

        private void OnDisable()
        {
            RefCount--;
            if (RefCount <= 0)
            {
                if (screenShotTemp != null)
                    Destroy(screenShotTemp);
            }
        }
    }
}
