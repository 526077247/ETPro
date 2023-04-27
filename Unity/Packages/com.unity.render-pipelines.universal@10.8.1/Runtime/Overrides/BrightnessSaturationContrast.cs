using System;

// 通用渲染管线程序集
namespace UnityEngine.Rendering.Universal
{
    // 实例化类     添加到Volume组件菜单中
    [Serializable, VolumeComponentMenu("Addition-Post-processing/BrightnessSaturationContrast")]
    // 继承VolumeComponent组件和IPostProcessComponent接口，用以继承Volume框架
    public class BrightnessSaturationContrast : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter Enable = new BoolParameter(false);
        // 在框架下的属性与Unity常规属性不一样，例如 Int 由 ClampedIntParameter 取代。
        public ClampedFloatParameter brightness = new ClampedFloatParameter(0f, 0, 3);
        public ClampedFloatParameter saturation = new ClampedFloatParameter(0f, 0, 3);
        public ClampedFloatParameter contrast = new ClampedFloatParameter(0f, 0, 3);
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