using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public static class AOIHelper
    {
        public static long CreateCellId(int x, int y)
        {
            return (long) ((ulong) x << 32) | (uint) y;
        }

        public static long CreateCellId(Vector3 pos,int gridLen)
        {
            int x = (int)pos.x / gridLen;
            int y = (int)pos.z / gridLen;
            return CreateCellId(x,y);
        }

        /// <summary>
        /// 获取x，z平面投影与球的关系：-1无关 0相交或包括碰撞器 1在碰撞器内部
        /// </summary>
        public static int GetGridRelationshipWithSphere(Vector3 position,float radius, 
            int gridLen, int xMin, int yMin,float sqrRadius)
        {
            int yMax;
            
            Vector2 point = new Vector2(position.x,position.z) ;
            //圆心在格子外 0或-1
            if (point.x <= xMin) //圆心在格子左方
            {
                if (point.y <= yMin) //圆心在格子左下方
                {
                    if (Vector2.SqrMagnitude(point - new Vector2(xMin, yMin)) > sqrRadius)
                        return -1;
                }
                else
                {
                    yMax = yMin + gridLen;
                    if (point.y >= yMax) //圆心在格子左上方
                    {
                        if (Vector2.SqrMagnitude(point - new Vector2(xMin, yMax)) > sqrRadius)
                            return -1;
                    }
                    else //圆心在格子左侧方
                    {
                        if ((xMin - point.x) > radius)
                            return -1;
                    }
                }
            }
            else
            {
                var xMax = xMin + gridLen;
                yMax = yMin + gridLen;
                if (point.x >= xMax) //圆心在格子右方
                {
                    if (point.y > yMax) //圆心在格子右上方
                    {
                        if (Vector2.SqrMagnitude(point-new Vector2(xMax, yMax)) > sqrRadius)
                            return -1;
                    }
                    else if (point.y < yMin) //圆心在格子右下方
                    {
                        if (Vector2.SqrMagnitude(point- new Vector2(xMax, yMin)) > sqrRadius)
                            return -1;
                    }
                    else //圆心在格子右侧方
                    {
                        if ((point.x - xMax) > radius)
                            return -1;
                    }
                }
                else if (point.y > yMax) //圆心在格子上方
                {
                    if (point.x > xMin && point.x < xMax) //圆心在格子上侧方
                        if ((point.x - yMax) > radius)
                            return -1;
                }
                else if (point.y < yMin) //圆心在格子下方
                {
                    if (point.x > xMin && point.x < xMax) //圆心在格子下侧方
                        if ((yMin - point.y) > radius)
                            return -1;
                }
                else
                {
                    var cenx = xMin + gridLen / 2;
                    var ceny = yMin + gridLen / 2;
                    //圆心在格子内 0或1
                    if (point.x > cenx && point.y > ceny) //圆心在格子内右上方
                    {
                        if (Vector2.SqrMagnitude(point - new Vector2(xMin, yMin)) < sqrRadius)
                            return 1;
                    }
                    else if (point.x > cenx && point.y < ceny) //圆心在格子内右下方
                    {
                        if (Vector2.SqrMagnitude(point - new Vector2(xMin, yMax)) < sqrRadius)
                            return 1;
                    }
                    else if (point.x < cenx && point.y > ceny) //圆心在格子内左上方
                    {
                        if (Vector2.SqrMagnitude(point - new Vector2(xMax, yMin)) < sqrRadius)
                            return 1;
                    }
                    else if (point.x < cenx && point.y < ceny) //圆心在格子内左下方
                    {
                        if (Vector2.SqrMagnitude(point - new Vector2(xMax, yMax)) < sqrRadius)
                            return 1;
                    }
                    //圆心在格子内中心 0或1
                    else
                    {
                        if (gridLen * 0.7072f < radius) return 1;
                    }
                }
            }
            // Log.Info("0");
            return 0;
        }

        /// <summary>
        /// 获取x，z平面投影与OBB的关系：-1无关 0相交或包括碰撞器 1在碰撞器内部
        /// </summary>
        public static int GetGridRelationshipWithOBB(Vector3 position ,Quaternion rotation ,Vector3 scale,int gridLen,
            int xMin,int yMin,float radius,float sqrRadius)
        {
            var res = GetGridRelationshipWithSphere(position, radius,gridLen,xMin,yMin,sqrRadius);
            if (res>=0)
            {
                //判断格子4个顶点是否在碰撞体内
                if (!IsPointInTrigger(new Vector3(xMin, position.y, yMin),position,rotation,scale))
                {
                    return 0;
                }
                var xMax = xMin + gridLen;
                if (!IsPointInTrigger(new Vector3(xMax, position.y, yMin),position,rotation,scale))
                {
                    return 0;
                }
                var yMax = yMin + gridLen;
                if (!IsPointInTrigger(new Vector3(xMin, position.y, yMax),position,rotation,scale))
                {
                    return 0;
                }
                if (!IsPointInTrigger(new Vector3(xMax, position.y, yMax),position,rotation,scale))
                {
                    return 0;
                }

                return 1;
            }

            return res;
        }
        
        /// <summary>
        /// 获取x，z平面投影与球的关系：false无关 true相交
        /// </summary>
        public static bool IsGridIntersectWithSphere(Vector3 position, float radius,
            int gridLen, int xMin, int yMin,float sqrRadius)
        {
            var xMax = xMin + gridLen;
            int yMax = yMin + gridLen;
            Vector2 point = new Vector2(position.x,position.z);
            if (point.x < xMin - radius || point.x > xMax + radius || point.y < yMin - radius ||
                point.y > yMax + radius)
                return false;
            //圆心在格子内 0或1
            if (point.x >= xMin || point.x <= xMax || point.y >= yMin || point.y <= yMax)
                return true;
            //圆心在格子外 0或-1
            if (point.x <= xMin) //圆心在格子左方
            {
                if (point.y <= yMin) //圆心在格子左下方
                {
                    if (Vector2.SqrMagnitude(point - new Vector2(xMin, yMin)) > sqrRadius)
                        return false;
                }
                else
                {
                    yMax = yMin + gridLen;
                    if (point.y >= yMax) //圆心在格子左上方
                    {
                        if (Vector2.SqrMagnitude(point - new Vector2(xMin, yMax)) > sqrRadius)
                            return false;
                    }
                    else //圆心在格子左侧方
                    {
                        if ((xMin - point.x) > radius)
                            return false;
                    }
                }
            }
            else
            {
                
                if (point.x >= xMax) //圆心在格子右方
                {
                    if (point.y > yMax) //圆心在格子右上方
                    {
                        if (Vector2.SqrMagnitude(point-new Vector2(xMax, yMax)) > sqrRadius)
                            return false;
                    }
                    else if (point.y < yMin) //圆心在格子右下方
                    {
                        if (Vector2.SqrMagnitude(point- new Vector2(xMax, yMin)) > sqrRadius)
                            return false;
                    }
                    else //圆心在格子右侧方
                    {
                        if ((point.x - xMax) > radius)
                            return false;
                    }
                }
                else if (point.y > yMax) //圆心在格子上方
                {
                    if (point.x > xMin && point.x < xMax) //圆心在格子上侧方
                        if ((point.x - yMax) > radius)
                            return false;
                }
                else if (point.y < yMin) //圆心在格子下方
                {
                    if (point.x > xMin && point.x < xMax) //圆心在格子下侧方
                        if ((yMin - point.y) > radius)
                            return false;
                }
                //圆心在格子内 0或1
                else
                {
                }
            }
            // Log.Info("0");
            return true;
        }

        /// <summary>
        /// 当触发器在指定位置旋转到指定角度时，检测点是否在触发器内
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="pos"></param>
        ///  <param name="center"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public static bool IsPointInTrigger(Vector3 pos,Vector3 center,Quaternion rot,Vector3 scale)
        {
            Vector3 temp = Quaternion.Inverse(rot)*(pos - center); //转换到触发器模型空间坐标
            var xMax = scale.x / 2;
            var yMax = scale.y / 2;
            var zMax = scale.z / 2;
            return -xMax <= temp.x && temp.x <= xMax && -yMax <= temp.y && temp.y <= yMax && -zMax <= temp.z &&
                    temp.z <= zMax;
        }
        public static void KSsort<T>(this List<T> a, Func<T, T, int> compare, int start = -1, int end = -1)
        {
            if (start < 0) start = 0;
            if (end < 0) end = a.Count - 1;
            bool flag = true;
            T temp;
            while (end != start)
            {
                if (flag)
                {
                    int tempend = a.Count - 1;
                    while (start < tempend)
                    {
                        if (compare(a[start] , a[tempend])>0)//右侧找比自己小的数
                        {
                            temp = a[start];
                            a[start] = a[tempend];
                            a[tempend] = temp;
                            flag = false;
                            break;
                        }
                        else
                        {
                            tempend--;
                            if (start == tempend)
                            {
                                start++;
                                flag = false;
                            }
                        }
                    }
                }
                else
                {
                    int tempstart = 0;
                    while (tempstart < end)
                    {
                        if (compare(a[tempstart], a[end]) > 0)//右侧找比自己小的数
                        {
                            temp = a[tempstart];
                            a[tempstart] = a[end];
                            a[end] = temp;
                            flag = true;
                            break;
                        }
                        else
                        {
                            tempstart++;
                            if (tempstart == end)
                            {
                                end--;
                                flag = true;
                            }
                        }
                    }
                }
            }
        }
    }
}