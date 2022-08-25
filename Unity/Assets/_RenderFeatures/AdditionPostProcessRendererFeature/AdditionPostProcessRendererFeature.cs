namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// 可编程渲染功能
    /// 必须要继承ScriptableRendererFeature抽象类，
    /// 并且实现AddRenderPasses跟Create函数
    /// </summary>
    public class AdditionPostProcessRendererFeature : ScriptableRendererFeature
    {
        // 用于后处理的Shader 
        public Shader shader;
        // 后处理Pass
        AdditionPostProcessPass postPass;
        // 根据Shader生成的材质
        Material _Material = null;

        //在这里，您可以在渲染器中注入一个或多个渲染通道。
        //每个摄像机设置一次渲染器时，将调用此方法。
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // 检测Shader是否存在
            if (shader == null)
                return;

            // 创建材质
            if (_Material == null)
                _Material = CoreUtils.CreateEngineMaterial(shader);

            // 获取当前渲染相机的目标颜色，也就是主纹理
            var cameraColorTarget = renderer.cameraColorTarget;

            // 设置调用后处理Pass
            postPass.Setup(cameraColorTarget, _Material);

            // 添加该Pass到渲染管线中
            renderer.EnqueuePass(postPass);
        }


        // 对象初始化时会调用该函数
        public override void Create()
        {
            postPass = new AdditionPostProcessPass();
            // 渲染时机 = 透明物体渲染后
            postPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }
    }
}