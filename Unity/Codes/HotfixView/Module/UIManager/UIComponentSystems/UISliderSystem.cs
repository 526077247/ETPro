using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    [UISystem]
    [FriendClass(typeof(UISlider))]
    public class UISliderDestorySystem : OnDestroySystem<UISlider>
    {
        public override void OnDestroy(UISlider self)
        {
            self.RemoveOnValueChanged();
        }
    }
    [FriendClass(typeof(UISlider))]
    public static class UISliderSystem
    {
        static void ActivatingComponent(this UISlider self)
        {
            if (self.unity_uislider == null)
            {
                self.unity_uislider = self.GetGameObject().GetComponent<Slider>();
                if (self.unity_uislider == null)
                {
                    Log.Error($"添加UI侧组件UISlider时，物体{self.GetGameObject().name}上没有找到Slider组件");
                }
            }
        }
        public static void SetOnValueChanged(this UISlider self,UnityAction<float> callback)
        {
            self.ActivatingComponent();
            self.RemoveOnValueChanged();
            self.__onValueChanged = callback;
            self.unity_uislider.onValueChanged.AddListener(self.__onValueChanged);
        }

        public static void RemoveOnValueChanged(this UISlider self)
        {
            if (self.__onValueChanged != null)
            {
                self.unity_uislider.onValueChanged.RemoveListener(self.__onValueChanged);
                self.__onValueChanged = null;
            }
        }

        public static void SetWholeNumbers(this UISlider self, bool wholeNumbers)
        {
            self.ActivatingComponent();
            self.unity_uislider.wholeNumbers = wholeNumbers;
            self.isWholeNumbers = true;
        }

        public static void SetMaxValue(this UISlider self, float value)
        {
            self.ActivatingComponent();
            self.unity_uislider.maxValue = value;
        }

        public static void SetMinValue(this UISlider self, float value)
        {
            self.ActivatingComponent();
            self.unity_uislider.minValue = value;
        }

        public static void SetValueList(this UISlider self, ArrayList value_list)
        {
            self.value_list = value_list;
            self.SetWholeNumbers(true);
            self.SetMinValue(0);
            self.SetMaxValue(value_list.Count - 1);
        }
   
        public static ArrayList GetValueList(this UISlider self)
        {
            return self.value_list;
        }

        public static object GetValue(this UISlider self)
        {
            self.ActivatingComponent();
            if (self.isWholeNumbers)
            {
                var index = (int)self.unity_uislider.value;
                return self.value_list[index];
            }
            else
            {
                return self.unity_uislider.normalizedValue;
            }
        }
        public static object GetNormalizedValue(this UISlider self)
        {
            self.ActivatingComponent();
            return self.unity_uislider.normalizedValue;
        }
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value">wholeNumbers 时value是ui侧的index</param>
        public static void SetValue(this UISlider self, int value)
        {
            self.ActivatingComponent();
            self.unity_uislider.value = value;
        }
        
        public static void SetWholeNumbersValue(this UISlider self, object value)
        {
            self.ActivatingComponent();
            if (!self.isWholeNumbers)
            {
                Log.Warning("请先设置WholeNumbers为true");
                return;
            }

            for (int i = 0; i < self.value_list.Count; i++)
            {
                if (self.value_list[i] == value)
                {
                    self.unity_uislider.value = i;
                    return;
                }
            }
            
        }
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value">wholeNumbers 时value是ui侧的index</param>
        public static void SetNormalizedValue(this UISlider self, float value)
        {
            self.ActivatingComponent();
            self.unity_uislider.normalizedValue = value;
        }
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value"></param>
        public static void SetValue(this UISlider self, float value)
        {
            self.ActivatingComponent();
            if (!self.isWholeNumbers)
                self.unity_uislider.value = value;
            else
            {
                Log.Warning("请先设置WholeNumbers为false");
                self.unity_uislider.value = (int)value;
            }
        }

    }
}
