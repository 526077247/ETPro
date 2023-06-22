using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ET
{
    public class LruCache<TKey, TValue>: IEnumerable<KeyValuePair<TKey, TValue>>
    {
        const int DEFAULT_CAPACITY = 255;

        int capacity;
        ReaderWriterLockSlim locker;
        Dictionary<TKey, TValue> dictionary;
        LinkedList<TKey> linkedList;
        Func<TKey, TValue, bool> checkCanPopFunc;
        Action<TKey, TValue> popCb;

        public LruCache(): this(DEFAULT_CAPACITY)
        {
        }

        public LruCache(int capacity)
        {
            this.locker = new ReaderWriterLockSlim();
            this.capacity = capacity > 0? capacity : DEFAULT_CAPACITY;
            this.dictionary = new Dictionary<TKey, TValue>(DEFAULT_CAPACITY);
            this.linkedList = new LinkedList<TKey>();
        }

        public void SetCheckCanPopCallback(Func<TKey, TValue, bool> func)
        {
            this.checkCanPopFunc = func;
        }

        public void SetPopCallback(Action<TKey, TValue> func)
        {
            this.popCb = func;
        }

        public TValue this[TKey t]
        {
            get
            {
                if (TryGet(t, out var res))
                    return res;
                throw new ArgumentException();
            }
            set
            {
                Set(t, value);
            }
        }

        public void Set(TKey key, TValue value)
        {
            this.locker.EnterWriteLock();
            try
            {
                if (this.checkCanPopFunc != null)
                    this.MakeFreeSpace();
                this.dictionary[key] = value;
                this.linkedList.Remove(key);
                this.linkedList.AddFirst(key);
                if (this.checkCanPopFunc == null && this.linkedList.Count > this.capacity)
                {
                    this.dictionary.Remove(this.linkedList.Last.Value);
                    this.linkedList.RemoveLast();
                }
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        public Dictionary<TKey, TValue> GetAll()
        {
            return this.dictionary;
        }

        public void Remove(TKey key)
        {
            this.locker.EnterWriteLock();
            try
            {
                this.dictionary.Remove(key);
                this.linkedList.Remove(key);
            }
            finally
            {
                this.locker.ExitWriteLock();
            }
        }

        public bool TryOnlyGet(TKey key, out TValue value)
        {
            bool b = this.dictionary.TryGetValue(key, out value);
            return b;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            this.locker.EnterUpgradeableReadLock();
            try
            {
                bool b = this.dictionary.TryGetValue(key, out value);
                if (b)
                {
                    this.locker.EnterWriteLock();
                    try
                    {
                        this.linkedList.Remove(key);
                        this.linkedList.AddFirst(key);
                    }
                    finally
                    {
                        this.locker.ExitWriteLock();
                    }
                }

                return b;
            }
            catch
            {
                throw;
            }
            finally
            {
                this.locker.ExitUpgradeableReadLock();
            }
        }

        public bool ContainsKey(TKey key)
        {
            this.locker.EnterReadLock();
            try
            {
                return this.dictionary.ContainsKey(key);
            }
            finally
            {
                this.locker.ExitReadLock();
            }
        }

        public int Count
        {
            get
            {
                this.locker.EnterReadLock();
                try
                {
                    return this.dictionary.Count;
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
        }

        public int Capacity
        {
            get
            {
                this.locker.EnterReadLock();
                try
                {
                    return this.capacity;
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
            set
            {
                this.locker.EnterUpgradeableReadLock();
                try
                {
                    if (value > 0 && this.capacity != value)
                    {
                        this.locker.EnterWriteLock();
                        try
                        {
                            this.capacity = value;
                            while (this.linkedList.Count > this.capacity)
                            {
                                this.linkedList.RemoveLast();
                            }
                        }
                        finally
                        {
                            this.locker.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    this.locker.ExitUpgradeableReadLock();
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                this.locker.EnterReadLock();
                try
                {
                    return this.dictionary.Keys;
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                this.locker.EnterReadLock();
                try
                {
                    return this.dictionary.Values;
                }
                finally
                {
                    this.locker.ExitReadLock();
                }
            }
        }

        //remotes elements to provide enough memory
        //returns last removed element or nil
        void MakeFreeSpace()
        {
            var key = this.linkedList.Last;

            var max_check_free_times = 10; // max check free times for avoid no tuple can free cause iterator much times;
            var cur_check_free_time = 0;


            while (this.linkedList.Count + 1 > DEFAULT_CAPACITY)
            {
                if (key == null) break;

                var tuple_prev = key.Previous;
                if (this.checkCanPopFunc == null || this.checkCanPopFunc(key.Value, this.dictionary[key.Value]))
                {
                    //can pop
                    var value = this.dictionary[key.Value];
                    this.dictionary.Remove(key.Value);
                    this.linkedList.Remove(key.Value);
                    this.popCb?.Invoke(key.Value, value);
                }
                else
                {
                    //the host say cannot pop
                    cur_check_free_time++;
                    if (cur_check_free_time > max_check_free_times)
                    {
                        //lru cache detect check_free time is too much, please check code
                        break;
                    }
                }

                key = tuple_prev;
            }

        }

        public void Clear()
        {
            this.dictionary.Clear();
            this.linkedList.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in this.dictionary)
            {
                yield return item;
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach (var item in this.dictionary)
            {
                yield return item;
            }
        }
    }
}