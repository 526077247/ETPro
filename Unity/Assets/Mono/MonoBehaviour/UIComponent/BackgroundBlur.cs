using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    /// <summary>
    /// UI弹窗后面的背景模糊
    /// </summary>
    public class BackgroundBlur : MonoBehaviour
    {
        public static float s_LobbyBlackTime = 0.6f;

        public RawImage rImage;

        private Texture2D m_screenShotTemp;
        
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
            rImage.gameObject.SetActive(false);
            if (m_screenShotTemp != null)
                GameObject.Destroy(m_screenShotTemp);

			yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            // 先创建一个的空纹理，大小可根据实现需要来设置  
            var rect = new Rect(0, 0, Screen.width, Screen.height);
            m_screenShotTemp = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            // 读取屏幕像素信息并存储为纹理数据，  
            m_screenShotTemp.ReadPixels(rect, 0, 0);
            m_screenShotTemp.Apply();
            rImage.gameObject.SetActive(true);
            rImage.texture = m_screenShotTemp;
        }

        private void OnDestroy()
        {
            if (m_screenShotTemp != null)
                GameObject.Destroy(m_screenShotTemp);
        }
    }
}
