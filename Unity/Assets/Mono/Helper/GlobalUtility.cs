using System;

namespace ET
{
    public static class GlobalUtility
    {
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp(this DateTime time)
        {
            TimeSpan ts = time - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }
        /// <summary>
        /// 获取不重复的随机数
        /// </summary>
        /// <returns></returns>
        public static long GetId(this Random random)
        {
            return DateTime.UtcNow.GetTimeStamp() * 100000 + DateTime.UtcNow.Millisecond * 100 + random.Next(0, 99);
        }
    }
}

