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
            var ui_camera = UIManagerComponent.Instance.GetUICamera();
            ui_camera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
            self.ResetSceneCamera();
        }

        public static void  ResetSceneCamera(this CameraManagerComponent self)
        {
            self.m_scene_main_camera_go = null;
            self.m_scene_main_camera = null;
        }
        public static void SetCameraStackAtLoadingDone(this CameraManagerComponent self)
        {
            self.m_scene_main_camera_go = Camera.main.gameObject;
            self.m_scene_main_camera = self.m_scene_main_camera_go.GetComponent<Camera>();
            var render = self.m_scene_main_camera.GetUniversalAdditionalCameraData();
            render.renderPostProcessing = true;
            render.renderType = CameraRenderType.Base;
            render.SetRenderer(1);
            var ui_camera = UIManagerComponent.Instance.GetUICamera();
            __AddOverlayCamera(self.m_scene_main_camera, ui_camera);
        }


        static void __AddOverlayCamera(Camera baseCamera, Camera overlayCamera)
        {
            overlayCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            baseCamera.GetUniversalAdditionalCameraData().cameraStack.Add(overlayCamera);
        }
    }
}
