using System;

namespace ET
{
    public class StepPara<T> : IDisposable
    {
        public T Value;

        public static StepPara<T> Create(T value)
        {
            var res = MonoPool.Instance.Fetch<StepPara<T>>();
            res.Value = value;
            return res;
        }

        public void Dispose()
        {
            Value = default;
            MonoPool.Instance.Recycle(this);
        }
    }
    
    public static class StepParaHelper
    {

        public static bool TryParseInt(ref object obj,out int value)
        {
            if (obj is StepPara<int> res)
            {
                value = res.Value;
                return true;
            }

            if (obj is int num)
            {
                obj = StepPara<int>.Create(num);
                value = num;
                return true;
            }

            if (obj is string str && int.TryParse(str, out value))
            {
                obj = StepPara<int>.Create(value);
                return true;
            }

            value = 0;
            return false;
        }
        public static bool TryParseFloat(ref object obj,out float value)
        {
            if (obj is StepPara<float> res)
            {
                value = res.Value;
                return true;
            }

            if (obj is float num)
            {
                obj = StepPara<float>.Create(num);
                value = num;
                return true;
            }

            if (obj is string str && float.TryParse(str, out value))
            {
                obj = StepPara<float>.Create(value);
                return true;
            }

            value = 0;
            return false;
        }
        public static bool TryParseString(ref object obj,out string value)
        {
            if (obj is StepPara<string> res)
            {
                value = res.Value;
                return true;
            }

            if (obj is string str)
            {
                obj = StepPara<string>.Create(str);
                value = str;
                return true;
            }
            
            value = obj.ToString();
            obj = StepPara<string>.Create(value);
            return true;
        }
    }
}