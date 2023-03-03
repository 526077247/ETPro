using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class CameraManagerComponent: Entity,IAwake,IDestroy
    {
        public static CameraManagerComponent Instance;
        public GameObject sceneMainCameraGo;
        public Camera sceneMainCamera;
    }
}
