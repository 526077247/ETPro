using UnityEngine;

namespace ET
{
    public struct Ray
    {
        /// <summary>
        /// 方向
        /// </summary>
        public Vector3 Dir;
        /// <summary>
        /// 起点
        /// </summary>
        public Vector3 Start;
        /// <summary>
        /// 长度
        /// </summary>
        public float Distance;
        /// <summary>
        /// 长度
        /// </summary>
        public float SqrDistance;
        /// <summary>
        /// 世界空间转模型空间
        /// </summary>
        /// <param name="self"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Ray WorldToModel(Ray self, Quaternion b, Vector3 c)
        {
            return new Ray
            {
                Dir = Quaternion.Inverse(b) * self.Dir,
                Start = Quaternion.Inverse(b) * (self.Start - c),
                Distance = self.Distance,
                SqrDistance = self.Distance*self.Distance
            };
        }
        
    }
}