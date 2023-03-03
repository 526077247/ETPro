using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ET
{
    [ObjectSystem]
    public class CameraManagerComponentAwakeSystem : AwakeSystem<CameraManagerComponent>
    {
        public override void Awake(CameraManagerComponent self)
        {
            CameraManagerComponent.Instance = self;
        }
    }

    [ObjectSystem]
    public class CameraManagerComponentDestroySystem : DestroySystem<CameraManagerComponent>
    {
        public override void Destroy(CameraManagerComponent self)
        {
            CameraManagerComponent.Instance = null;
        }
    }
    [FriendClass(typeof(CameraManagerComponent))]
    public static class CameraManagerComponentSystem
    {
        
        //在场景loading开始时设置camera statck
        //loading时场景被销毁，这个时候需要将UI摄像机从overlay->base
        public static void SetCameraStackAtLoadingStart(this CameraManagerComponent self)
        {
            var uiCamera = UIManagerComponent.Instance.GetUICamera();
            uiCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
            self.ResetSceneCamera();
        }

        public static void ResetSceneCamera(this CameraManagerComponent self)
        {
            self.sceneMainCameraGo = null;
            self.sceneMainCamera = null;
        }

        public static Camera MainCamera(this CameraManagerComponent self)
        {
            return self.sceneMainCamera;
        }
        public static void SetCameraStackAtLoadingDone(this CameraManagerComponent self)
        {
            self.sceneMainCameraGo = Camera.main.gameObject;
            self.sceneMainCamera = self.sceneMainCameraGo.GetComponent<Camera>();
            var render = self.sceneMainCamera.GetUniversalAdditionalCameraData();
            render.renderPostProcessing = true;
            render.renderType = CameraRenderType.Base;
            render.SetRenderer(1);
            var uiCamera = UIManagerComponent.Instance.GetUICamera();
            AddOverlayCamera(self.sceneMainCamera, uiCamera);
        }


        static void AddOverlayCamera(Camera baseCamera, Camera overlayCamera)
        {
            overlayCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            baseCamera.GetUniversalAdditionalCameraData().cameraStack.Add(overlayCamera);
        }
    }
}
