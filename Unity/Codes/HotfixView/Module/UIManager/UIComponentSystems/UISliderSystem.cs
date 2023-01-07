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
    [FriendClass(typeof (UISlider))]
    public class UISliderDestorySystem: OnDestroySystem<UISlider>
    {
        public override void OnDestroy(UISlider self)
        {
            self.RemoveOnValueChanged();
        }
    }

    [FriendClass(typeof (UISlider))]
    public static class UISliderSystem
    {
        static void ActivatingComponent(this UISlider self)
        {
            if (self.slider == null)
            {
                self.slider = self.GetGameObject().GetComponent<Slider>();
                if (self.slider == null)
                {
                    Log.Error($"添加UI侧组件UISlider时，物体{self.GetGameObject().name}上没有找到Slider组件");
                }
            }
        }

        public static void SetOnValueChanged(this UISlider self, UnityAction<float> callback)
        {
            self.ActivatingComponent();
            self.RemoveOnValueChanged();
            self.onValueChanged = callback;
            self.slider.onValueChanged.AddListener(self.onValueChanged);
        }

        public static void RemoveOnValueChanged(this UISlider self)
        {
            if (self.onValueChanged != null)
            {
                self.slider.onValueChanged.RemoveListener(self.onValueChanged);
                self.onValueChanged = null;
            }
        }

        public static void SetWholeNumbers(this UISlider self, bool wholeNumbers)
        {
            self.ActivatingComponent();
            self.slider.wholeNumbers = wholeNumbers;
            self.isWholeNumbers = true;
        }

        public static void SetMaxValue(this UISlider self, float value)
        {
            self.ActivatingComponent();
            self.slider.maxValue = value;
        }

        public static void SetMinValue(this UISlider self, float value)
        {
            self.ActivatingComponent();
            self.slider.minValue = value;
        }

        public static void SetValueList(this UISlider self, ArrayList value_list)
        {
            self.valueList = value_list;
            self.SetWholeNumbers(true);
            self.SetMinValue(0);
            self.SetMaxValue(value_list.Count - 1);
        }

        public static ArrayList GetValueList(this UISlider self)
        {
            return self.valueList;
        }

        public static object GetValue(this UISlider self)
        {
            self.ActivatingComponent();
            if (self.isWholeNumbers)
            {
                var index = (int) self.slider.value;
                return self.valueList[index];
            }
            else
            {
                return self.slider.normalizedValue;
            }
        }

        public static object GetNormalizedValue(this UISlider self)
        {
            self.ActivatingComponent();
            return self.slider.normalizedValue;
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value">wholeNumbers 时value是ui侧的index</param>
        public static void SetValue(this UISlider self, int value)
        {
            self.ActivatingComponent();
            self.slider.value = value;
        }

        public static void SetWholeNumbersValue(this UISlider self, object value)
        {
            self.ActivatingComponent();
            if (!self.isWholeNumbers)
            {
                Log.Warning("请先设置WholeNumbers为true");
                return;
            }

            for (int i = 0; i < self.valueList.Count; i++)
            {
                if (self.valueList[i] == value)
                {
                    self.slider.value = i;
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
            self.slider.normalizedValue = value;
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="value"></param>
        public static void SetValue(this UISlider self, float value)
        {
            self.ActivatingComponent();
            if (!self.isWholeNumbers)
                self.slider.value = value;
            else
            {
                Log.Warning("请先设置WholeNumbers为false");
                self.slider.value = (int) value;
            }
        }
    }
}