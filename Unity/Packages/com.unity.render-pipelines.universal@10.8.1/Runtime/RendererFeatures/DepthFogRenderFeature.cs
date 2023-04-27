namespace UnityEngine.Rendering.Universal
{
    public class DepthFogRenderFeature: ScriptableRendererFeature
    {
        // 用于后处理的Shader 
        public Shader shader;

        // 后处理Pass
        FogPass _fogPass;

        // 根据Shader生成的材质
        Material _Material = null;

        public override void Create()
        {
            if (_fogPass == null)
            {
                _fogPass = new FogPass();
            }
        }
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // 检测Shader是否存在
            if (shader == null)
                return;
            // 创建材质
            if (_Material == null)
                _Material = CoreUtils.CreateEngineMaterial(shader);
            // 设置调用后处理Pass
            this._fogPass.Setup(_Material);
            // 添加该Pass到渲染管线中
            renderer.EnqueuePass(_fogPass);
        }
    }
}