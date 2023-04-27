using System;

// 通用渲染管线程序集
namespace UnityEngine.Rendering.Universal
{
    // 实例化类     添加到Volume组件菜单中
    [Serializable, VolumeComponentMenu("Addition-Post-processing/DepthFog")]
    // 继承VolumeComponent组件和IPostProcessComponent接口，用以继承Volume框架
    public class DepthFog : VolumeComponent, IPostProcessComponent
    {
        // 在框架下的属性与Unity常规属性不一样，例如 Int 由 ClampedIntParameter 取代。
        public BoolParameter Enable = new BoolParameter(false);
        
        public ColorParameter FogColor = new ColorParameter(Color.white);
       
        public ClampedFloatParameter FogDensity = new ClampedFloatParameter(0.5f,0,1);
        public BoolParameter EnableFarFog = new BoolParameter(false);
        public ClampedFloatParameter FogStart = new ClampedFloatParameter(0.5f,0,1);
        public ClampedFloatParameter FogEnd = new ClampedFloatParameter(0.5f,0,1);
        public BoolParameter EnableDeepFog = new BoolParameter(true);
        public FloatParameter FogDeepStart = new FloatParameter(0);
        public FloatParameter FogDeepEnd = new FloatParameter(1);
        
        // 实现接口
        public bool IsActive()
        {
            return Enable.value;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }
}