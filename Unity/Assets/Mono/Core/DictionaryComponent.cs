using System;
using System.Collections.Generic;

namespace ET
{
    public class DictionaryComponent<T,V>: Dictionary<T,V>, IDisposable
    {
        public static DictionaryComponent<T,V> Create()
        {
            return MonoPool.Instance.Fetch(TypeInfo<DictionaryComponent<T,V>>.Type) as DictionaryComponent<T,V>;
        }

        public void Dispose()
        {
            this.Clear();
            MonoPool.Instance.Recycle(this);
        }
    }
}