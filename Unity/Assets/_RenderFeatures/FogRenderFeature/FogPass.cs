namespace UnityEngine.Rendering.Universal
{
    public class FogPass : ScriptableRenderPass
    {
        // 属性参数组件
        DepthFog m_DepthFog;

        // 颜色渲染标识符
        RenderTargetIdentifier m_ColorAttachment;
        // 临时的渲染目标
        RenderTargetHandle m_TemporaryColorTexture01;

        private static int _fogColorID = Shader.PropertyToID("_FogColor");
        private static int _fogDensityID = Shader.PropertyToID("_FogDensity");
        private static int _fogStartID = Shader.PropertyToID("_FogStart");
        private static int _fogEndID = Shader.PropertyToID("_FogEnd");
        private static int _fogDeepStartID = Shader.PropertyToID("_FogDeepStart");
        private static int _fogDeepEndID = Shader.PropertyToID("_FogDeepEnd");
        private static int _ClipToWorldMatrix = Shader.PropertyToID("_ClipToWorldMatrix");
        private static int _CameraForward = Shader.PropertyToID("_CameraForward");
        private static int _NearPlane = Shader.PropertyToID("_NearPlane");

        private static int _CameraWorldPos = Shader.PropertyToID("_CameraWorldPos");


        const string m_ProfilerTag = "FogPass";
        Material m_BlitMaterial;

        private const string DEEP_KEY = "_ENABLE_DEEP_FOG";
        private const string FAR_KEY = "_ENALBE_FAR_FOG";
        // private const string ORTHO_CAM = "_ORTHO_CAM";

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="material"></param>
        public void Setup(Material material)
        {
            m_BlitMaterial = material;
            renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 从Volume框架中获取所有堆栈
            var stack = VolumeManager.instance.stack;
            // 从堆栈中查找对应的属性参数组件
            this.m_DepthFog = stack.GetComponent<DepthFog>();

            var res = Render(ref renderingData);
            if(!res) return;
            // Note: We need to get the cameraData.targetTexture as this will get the targetTexture of the camera stack.
            // Overlay cameras need to output to the target described in the base camera while doing camera stack.
            ref CameraData cameraData = ref renderingData.cameraData;
            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            if (isSceneViewCamera)
            {
                return;
            }

            if (m_BlitMaterial == null)
            {
                Debug.LogErrorFormat(
                    "Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.",
                    m_BlitMaterial, GetType().Name);
                return;
            }


            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);


            Camera camera = cameraData.camera;
            m_BlitMaterial.SetVector(_CameraForward, camera.transform.forward);
            Shader.SetGlobalMatrix(_ClipToWorldMatrix, camera.cameraToWorldMatrix * camera.projectionMatrix.inverse);
            m_BlitMaterial.SetFloat(_NearPlane, camera.nearClipPlane);
            Shader.SetGlobalVector(_CameraWorldPos, camera.transform.position);

            // if (camera.orthographic)
            //     m_BlitMaterial.EnableKeyword(ORTHO_CAM);
            // else
            // {
            //     m_BlitMaterial.DisableKeyword(ORTHO_CAM);
            // }

            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.SetViewport(camera.pixelRect);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_BlitMaterial);
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);


            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        // 渲染
        bool Render(ref RenderingData renderingData)
        {
            // VolumeComponent是否开启，且非Scene视图摄像机
            if (this.m_DepthFog.active&&this.m_DepthFog.IsActive() && !renderingData.cameraData.isSceneViewCamera)
            {
                // 写入参数
                m_BlitMaterial.SetColor(_fogColorID, this.m_DepthFog.FogColor.value);
                m_BlitMaterial.SetFloat(_fogDensityID, this.m_DepthFog.FogDensity.value);
                m_BlitMaterial.SetFloat(_fogStartID, this.m_DepthFog.FogStart.value);
                m_BlitMaterial.SetFloat(_fogEndID, this.m_DepthFog.FogEnd.value);
                m_BlitMaterial.SetFloat(_fogDeepStartID, this.m_DepthFog.FogDeepStart.value);
                m_BlitMaterial.SetFloat(_fogDeepEndID, this.m_DepthFog.FogDeepEnd.value);
                if (this.m_DepthFog.EnableDeepFog.value)
                {
                    m_BlitMaterial.EnableKeyword(DEEP_KEY);
                }
                else
                {
                    m_BlitMaterial.DisableKeyword(DEEP_KEY);
                }
                if (this.m_DepthFog.EnableFarFog.value)
                {
                    m_BlitMaterial.EnableKeyword(FAR_KEY);
                }
                else
                {
                    m_BlitMaterial.DisableKeyword(FAR_KEY);
                }
                Debug.Log("123321"+this.m_DepthFog.IsActive() );
                return true;
            }

            return false;
        }
    }
}