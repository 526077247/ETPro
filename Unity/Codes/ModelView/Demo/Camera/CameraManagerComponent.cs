using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class CameraManagerComponent: Entity,IAwake,IDestroy
    {
        public static CameraManagerComponent Instance;
        public GameObject m_scene_main_camera_go;
        public Camera m_scene_main_camera;
    }
}
