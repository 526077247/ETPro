using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class OBBComponentAwakeSystem : AwakeSystem<OBBComponent,Vector3>
    {
        public override void Awake(OBBComponent self, Vector3 a)
        {
            self.Scale = a;
        }
    }
    [ObjectSystem]
    public class OBBComponentDestroySystem : DestroySystem<OBBComponent>
    {
        public override void Destroy(OBBComponent self)
        {
            var trigger = self.GetParent<AOITrigger>();
            if(!trigger.IsCollider)
                trigger.GetParent<AOIUnitComponent>().RemoverTrigger(trigger);
            else
                trigger.GetParent<AOIUnitComponent>().RemoverCollider(trigger);
        }
    }
    [FriendClass(typeof(OBBComponent))]
    public static class OBBComponentSystem
    {
        /// <summary>
        /// 获取8个顶点，注意用完dispose
        /// </summary>
        /// <param name="self"></param>
        /// <param name="realPos"></param>
        /// <param name="realRot"></param>
        /// <returns></returns>
        public static ListComponent<Vector3> GetAllVertex(this OBBComponent self,Vector3 realPos,Quaternion realRot)
        {
            ListComponent<Vector3> res = ListComponent<Vector3>.Create();
            for (float i = -0.5f; i < 0.5f; i++)
            {
                for (float j = -0.5f; j < 0.5f; j++)
                {
                    for (float k = -0.5f; k < 0.5f; k++)
                    {
                        Vector3 temp = new Vector3(self.Scale.x*i,self.Scale.y*j,self.Scale.z*k);
                        temp = realPos + realRot * temp;
                        res.Add(temp);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// 获取12条边，注意用完dispose
        /// </summary>
        /// <param name="self"></param>
        /// <param name="realPos"></param>
        /// <param name="realRot"></param>
        /// <returns></returns>
        public static ListComponent<Ray> GetAllSide(this OBBComponent self, Vector3 realPos, Quaternion realRot)
        {
            ListComponent<Ray> res = ListComponent<Ray>.Create();
            Vector3 temp = realPos + realRot * new Vector3(self.Scale.x,self.Scale.y,self.Scale.z);
            Ray ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.left,
                Distance = self.Scale.x
            };
            res.Add(ray);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.down,
                Distance = self.Scale.y
            };
            res.Add(ray);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.back,
                Distance = self.Scale.z
            };
            res.Add(ray);
            
            temp = realPos + realRot * new Vector3(-self.Scale.x,-self.Scale.y,-self.Scale.z);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.right,
                Distance = self.Scale.x
            };
            res.Add(ray);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.up,
                Distance = self.Scale.y
            };
            res.Add(ray);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.forward,
                Distance = self.Scale.z
            };
            res.Add(ray);

            temp = realPos + realRot * new Vector3(-self.Scale.x,self.Scale.y,self.Scale.z);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.up,
                Distance = self.Scale.y
            };
            res.Add(ray);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.forward,
                Distance = self.Scale.z
            };
            res.Add(ray);

                
            temp = realPos + realRot * new Vector3(self.Scale.x,self.Scale.y,-self.Scale.z);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.right,
                Distance = self.Scale.x
            };
            res.Add(ray);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.up,
                Distance = self.Scale.y
            };
            res.Add(ray);

            temp = realPos + realRot * new Vector3(self.Scale.x,-self.Scale.y,self.Scale.z);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.right,
                Distance = self.Scale.x
            };
            res.Add(ray);
            ray = new Ray()
            {
                Start = temp,
                Dir = realRot * Vector3.forward,
                Distance = self.Scale.z
            };
            res.Add(ray);
            
            return res;
        }

        /// <summary>
        /// 判断触发器是否在触发器移到指定位置后之相交
        /// </summary>
        /// <param name="trigger1"></param>
        /// <param name="trigger2"></param>
        /// <param name="pos1"></param>
        /// <param name="rotation1"></param>
        /// <param name="pos2"></param>
        /// <param name="rotation2"></param>
        /// <returns></returns>
        public static bool IsInTrigger(this OBBComponent trigger1, OBBComponent trigger2, Vector3 pos1, 
            Quaternion rotation1, Vector3 pos2, Quaternion rotation2)
        {
            // Log.Info("判断OBB触发");
            //第一种情况一方有一个点在对方内部即为触发
            using (var list = trigger1.GetAllVertex(pos1,rotation1))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if(IsPointInTrigger(trigger2,list[i],pos2,rotation2))
                    {
                        return true;
                    }
                }
            }
            using (var list = trigger2.GetAllVertex(pos2,rotation2))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if(IsPointInTrigger(trigger1,list[i],pos1,rotation1))
                    {
                        return true;
                    }
                }
            }
            //第二种情况，没有点在对方内部，但边和对方面相交了
            using (var list = trigger1.GetAllSide(pos1,rotation1))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (trigger2.IsRayInTrigger(list[i], pos2, rotation2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 判断某个点是否在触发器移到指定位置后之内
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="position"></param>
        /// <param name="center"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static bool IsPointInTrigger(this OBBComponent trigger, Vector3 position,Vector3 center,Quaternion rotation)
        {
            return AOIHelper.IsPointInTrigger(position, center, rotation, trigger.Scale);
        }
        /// <summary>
        /// 判断射线是否在触发器移到指定位置后之内
        /// </summary>
        /// <param name="self"></param>
        /// <param name="ray"></param>
        /// <param name="center"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static bool IsRayInTrigger(this OBBComponent self, Ray ray, Vector3 center, Quaternion rotation)
        {
            var hit = Vector3.zero;
            //转换到模型空间
            ray = Ray.WorldToModel(ray,rotation,center);
            var xMax = self.Scale.x / 2;
            var yMax = self.Scale.y / 2;
            var zMax = self.Scale.z / 2;
            //起点在范围内
            if (-xMax <= ray.Start.x && ray.Start.x <= xMax && -yMax <= ray.Start.y && ray.Start.y <= yMax &&
                -zMax <= ray.Start.z && ray.Start.z <= zMax)
            {
                hit = rotation * ray.Start + center;
                return true;
            }

            #region 方向向量只有一个轴有值
            else if (ray.Dir.x == 0&&ray.Dir.y == 0&&ray.Dir.z != 0)
            {
                if (ray.Start.z < 0)
                {
                    if (ray.Dir.z < 0) return false;
                    hit = new Vector3(ray.Start.x, ray.Start.y, -zMax);
                }
                else
                {
                    if (ray.Dir.z > 0) return false;
                    hit = new Vector3(ray.Start.x, ray.Start.y, zMax);
                }
                hit = rotation * ray.Start + center;
                if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                return -xMax <= ray.Start.x && ray.Start.x <= xMax &&
                       -yMax <= ray.Start.y && ray.Start.y <= yMax;
            }
            else if (ray.Dir.x == 0&&ray.Dir.y != 0&&ray.Dir.z == 0)
            {
                if (ray.Start.y < 0)
                {
                    if (ray.Dir.y < 0) return false;
                    hit = new Vector3(ray.Start.x, -yMax, ray.Start.z);
                }
                else
                {
                    if (ray.Dir.y > 0) return false;
                    hit = new Vector3(ray.Start.x, yMax, ray.Start.z);
                }
                hit = rotation * ray.Start + center;
                if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                return -xMax <= ray.Start.x && ray.Start.x <= xMax &&
                       -zMax <= ray.Start.z && ray.Start.z <= zMax;
            }
            else if (ray.Dir.x != 0&&ray.Dir.y == 0&&ray.Dir.z == 0)
            {
                if (ray.Start.x < 0)
                {
                    if (ray.Dir.x < 0) return false;
                    hit = new Vector3(-xMax, ray.Start.y, ray.Start.z);
                }
                else
                {
                    if (ray.Dir.x > 0) return false;
                    hit = new Vector3(xMax, ray.Start.y, ray.Start.z);
                }
                hit = rotation * ray.Start + center;
                if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                return -yMax <= ray.Start.y && ray.Start.y <= yMax &&
                       -zMax <= ray.Start.z && ray.Start.z <= zMax;
            }
            #endregion

            #region 方向向量有两个轴有值
            else if (ray.Dir.x == 0&&ray.Dir.y != 0&&ray.Dir.z != 0)
            {
                //简化为平面直角坐标系
                if (-xMax <= ray.Start.x && ray.Start.x <= xMax)
                {
                    if (IsRayInTrigger2D(ray.Start.y,ray.Start.z,ray.Dir.y,ray.Dir.z,yMax,zMax,out var hit2d))
                    {
                        hit = new Vector3(ray.Start.x, hit2d.x, hit2d.y);
                        hit = rotation * ray.Start + center;
                        if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                        return true;
                    }
                }
                return false;
            }
            else if (ray.Dir.x != 0&&ray.Dir.y == 0&&ray.Dir.z != 0)
            {
                //简化为平面直角坐标系
                if (-yMax <= ray.Start.y && ray.Start.y <= yMax)
                {
                    if (IsRayInTrigger2D(ray.Start.x,ray.Start.z,ray.Dir.x,ray.Dir.z,xMax,zMax,out var hit2d))
                    {
                        hit = new Vector3(hit2d.x,ray.Start.y,hit2d.y);
                        hit = rotation * ray.Start + center;
                        if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                        return true;
                    }
                }
                return false;
            }
            else if (ray.Dir.x != 0&&ray.Dir.y != 0&&ray.Dir.z == 0)
            {
                //简化为平面直角坐标系
                if (-zMax <= ray.Start.z && ray.Start.z <= zMax)
                {
                    if (IsRayInTrigger2D(ray.Start.x,ray.Start.y,ray.Dir.x,ray.Dir.y,xMax,yMax,out var hit2d))
                    {
                        hit = new Vector3(hit2d.x,hit2d.y,ray.Start.z);
                        hit = rotation * ray.Start + center;
                        if (Vector3.SqrMagnitude(hit-ray.Start) > ray.SqrDistance) return false;
                        return true;
                    }
                }
                return false;
            }
            #endregion
            
            //正常情况
            //判断3个面的投影是否都相交
            if (!IsRayInTrigger2D(ray.Start.y,ray.Start.z,ray.Dir.y,ray.Dir.z,yMax,zMax,out var hit2d1))
            {
                return false;
            }
            if (!IsRayInTrigger2D(ray.Start.x,ray.Start.z,ray.Dir.x,ray.Dir.z,xMax,zMax,out var hit2d2))
            {
                return false;
            }
            if (!IsRayInTrigger2D(ray.Start.x,ray.Start.y,ray.Dir.x,ray.Dir.y,xMax,yMax,out var hit2d3))
            {
                return false;
            }
            if (Mathf.Abs(Mathf.Abs(hit2d1.y) - zMax) < Mathf.Abs(Mathf.Abs(hit2d1.x) - yMax))
            {
                if (Mathf.Abs(Mathf.Abs(hit2d3.y) - yMax) < Mathf.Abs(Mathf.Abs(hit2d3.x) - xMax))
                {
                    hit = new Vector3(hit2d3.x,hit2d1.y,hit2d1.x);
                }
                else
                {
                    hit = new Vector3(hit2d3.x,hit2d1.y,hit2d2.y);
                }
            }
            else
            {
                if (Mathf.Abs(Mathf.Abs(hit2d2.x) - xMax) < Mathf.Abs(Mathf.Abs(hit2d2.y) - zMax))
                {
                    hit = new Vector3(hit2d2.x,hit2d1.x,hit2d2.y);
                }
                else
                {
                    hit = new Vector3(hit2d2.x,hit2d3.y,hit2d2.y);
                }
            }
            hit = rotation * hit + center;
            if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
            return true;
        }
        
        /// <summary>
        /// 判断射线是否在触发器移到指定位置后之内
        /// </summary>
        /// <param name="self"></param>
        /// <param name="ray"></param>
        /// <param name="center"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static bool IsRayInTrigger(this OBBComponent self, Ray ray, Vector3 center, Quaternion rotation,out Vector3 hit)
        {
            hit = Vector3.zero;
            //转换到模型空间
            ray = Ray.WorldToModel(ray,rotation,center);
            var xMax = self.Scale.x / 2;
            var yMax = self.Scale.y / 2;
            var zMax = self.Scale.z / 2;
            //起点在范围内
            if (-xMax <= ray.Start.x && ray.Start.x <= xMax && -yMax <= ray.Start.y && ray.Start.y <= yMax &&
                -zMax <= ray.Start.z && ray.Start.z <= zMax)
            {
                hit = rotation * ray.Start + center;
                return true;
            }

            #region 方向向量只有一个轴有值
            else if (ray.Dir.x == 0&&ray.Dir.y == 0&&ray.Dir.z != 0)
            {
                if (ray.Start.z < 0)
                {
                    if (ray.Dir.z < 0) return false;
                    hit = new Vector3(ray.Start.x, ray.Start.y, -zMax);
                }
                else
                {
                    if (ray.Dir.z > 0) return false;
                    hit = new Vector3(ray.Start.x, ray.Start.y, zMax);
                }
                hit = rotation * ray.Start + center;
                if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                return -xMax <= ray.Start.x && ray.Start.x <= xMax &&
                       -yMax <= ray.Start.y && ray.Start.y <= yMax;
            }
            else if (ray.Dir.x == 0&&ray.Dir.y != 0&&ray.Dir.z == 0)
            {
                if (ray.Start.y < 0)
                {
                    if (ray.Dir.y < 0) return false;
                    hit = new Vector3(ray.Start.x, -yMax, ray.Start.z);
                }
                else
                {
                    if (ray.Dir.y > 0) return false;
                    hit = new Vector3(ray.Start.x, yMax, ray.Start.z);
                }
                hit = rotation * ray.Start + center;
                if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                return -xMax <= ray.Start.x && ray.Start.x <= xMax &&
                       -zMax <= ray.Start.z && ray.Start.z <= zMax;
            }
            else if (ray.Dir.x != 0&&ray.Dir.y == 0&&ray.Dir.z == 0)
            {
                if (ray.Start.x < 0)
                {
                    if (ray.Dir.x < 0) return false;
                    hit = new Vector3(-xMax, ray.Start.y, ray.Start.z);
                }
                else
                {
                    if (ray.Dir.x > 0) return false;
                    hit = new Vector3(xMax, ray.Start.y, ray.Start.z);
                }
                hit = rotation * ray.Start + center;
                if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                return -yMax <= ray.Start.y && ray.Start.y <= yMax &&
                       -zMax <= ray.Start.z && ray.Start.z <= zMax;
            }
            #endregion

            #region 方向向量有两个轴有值
            else if (ray.Dir.x == 0&&ray.Dir.y != 0&&ray.Dir.z != 0)
            {
                //简化为平面直角坐标系
                if (-xMax <= ray.Start.x && ray.Start.x <= xMax)
                {
                    if (IsRayInTrigger2D(ray.Start.y,ray.Start.z,ray.Dir.y,ray.Dir.z,yMax,zMax,out var hit2d))
                    {
                        hit = new Vector3(ray.Start.x, hit2d.x, hit2d.y);
                        hit = rotation * ray.Start + center;
                        if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                        return true;
                    }
                }
                return false;
            }
            else if (ray.Dir.x != 0&&ray.Dir.y == 0&&ray.Dir.z != 0)
            {
                //简化为平面直角坐标系
                if (-yMax <= ray.Start.y && ray.Start.y <= yMax)
                {
                    if (IsRayInTrigger2D(ray.Start.x,ray.Start.z,ray.Dir.x,ray.Dir.z,xMax,zMax,out var hit2d))
                    {
                        hit = new Vector3(hit2d.x,ray.Start.y,hit2d.y);
                        hit = rotation * ray.Start + center;
                        if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                        return true;
                    }
                }
                return false;
            }
            else if (ray.Dir.x != 0&&ray.Dir.y != 0&&ray.Dir.z == 0)
            {
                //简化为平面直角坐标系
                if (-zMax <= ray.Start.z && ray.Start.z <= zMax)
                {
                    if (IsRayInTrigger2D(ray.Start.x,ray.Start.y,ray.Dir.x,ray.Dir.y,xMax,yMax,out var hit2d))
                    {
                        hit = new Vector3(hit2d.x,hit2d.y,ray.Start.z);
                        hit = rotation * ray.Start + center;
                        if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
                        return true;
                    }
                }
                return false;
            }
            #endregion
            
            //正常情况
            //判断3个面的投影是否都相交
            if (!IsRayInTrigger2D(ray.Start.y,ray.Start.z,ray.Dir.y,ray.Dir.z,yMax,zMax,out var hit2d1))
            {
                return false;
            }
            if (!IsRayInTrigger2D(ray.Start.x,ray.Start.z,ray.Dir.x,ray.Dir.z,xMax,zMax,out var hit2d2))
            {
                return false;
            }
            if (!IsRayInTrigger2D(ray.Start.x,ray.Start.y,ray.Dir.x,ray.Dir.y,xMax,yMax,out var hit2d3))
            {
                return false;
            }
            if (Mathf.Abs(Mathf.Abs(hit2d1.y) - zMax) < Mathf.Abs(Mathf.Abs(hit2d1.x) - yMax))
            {
                if (Mathf.Abs(Mathf.Abs(hit2d3.y) - yMax) < Mathf.Abs(Mathf.Abs(hit2d3.x) - xMax))
                {
                    hit = new Vector3(hit2d3.x,hit2d1.y,hit2d1.x);
                }
                else
                {
                    hit = new Vector3(hit2d3.x,hit2d1.y,hit2d2.y);
                }
            }
            else
            {
                if (Mathf.Abs(Mathf.Abs(hit2d2.x) - xMax) < Mathf.Abs(Mathf.Abs(hit2d2.y) - zMax))
                {
                    hit = new Vector3(hit2d2.x,hit2d1.x,hit2d2.y);
                }
                else
                {
                    hit = new Vector3(hit2d2.x,hit2d3.y,hit2d2.y);
                }
            }
            hit = rotation * hit + center;
            if (Vector3.SqrMagnitude(hit- ray.Start) > ray.SqrDistance) return false;
            return true;
        }
        
        /// <summary>
        /// 平面直角坐标系，检测射线和矩形的是否相交
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="dirX"></param>
        /// <param name="dirY"></param>
        /// <param name="xMax"></param>
        /// <param name="yMax"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        public static bool IsRayInTrigger2D(float startX, float startY, float dirX, float dirY, float xMax, float yMax,
            out Vector2 hit)
        {
            hit = Vector2.zero;
            if (startX > 0 && startY > 0) //第一象限
            {
                if (dirX > 0 && dirY > 0) return false;
                if (startX > xMax && dirX > 0) return false;
                if (startY > yMax && dirY > 0) return false;
                //y=kx+b
                var k = dirY / dirX;
                var b = startY - k * startX;
                var z1 = k * xMax + b;
                if (z1 < -yMax) return false;
                var z2 = k * -xMax + b;
                if (z2 > yMax) return false;
                if (z1 <= yMax)
                    hit = new Vector2(xMax, z1);
                else
                    hit = new Vector2((yMax - b) / k, yMax);
                return true;
            }

            if (startX > 0 && startY < 0) //第二象限
            {
                if (dirX > 0 && dirY < 0) return false;
                if (startX > xMax && dirX > 0) return false;
                if (startY < -yMax && dirY < 0) return false;
                //y=kx+b
                var k = dirY / dirX;
                var b = startY - k * startX;
                var z1 = k * xMax + b;
                if (z1 > yMax) return false;
                var z2 = k * -xMax + b;
                if (z2 < -yMax) return false;
                if (z1 >= -yMax)
                    hit = new Vector2(xMax, z1);
                else
                    hit = new Vector2((-yMax - b) / k, -yMax);
                return true;
            }

            if (startX < 0 && startY < 0) //第三象限
            {
                if (dirX < 0 && dirY < 0) return false;
                if (startX < -xMax && dirX < 0) return false;
                if (startY < -yMax && dirY < 0) return false;
                //y=kx+b
                var k = dirY / dirX;
                var b = startY - k * startX;
                var z1 = k * -xMax + b;
                if (z1 > yMax) return false;
                var z2 = k * xMax + b;
                if (z2 < -yMax) return false;
                if (z1 >= -yMax)
                    hit = new Vector2(-xMax, z1);
                else
                    hit = new Vector2((-yMax - b) / k, -yMax);
                return true;
            }

            if (startX < 0 && startY > 0) //第四象限
            {
                if (dirX < 0 && dirY > 0) return false;
                if (startX < -xMax && dirX < 0) return false;
                if (startY > yMax && dirY > 0) return false;
                //y=kx+b
                var k = dirY / dirX;
                var b = startY - k * startX;
                var z1 = k * -xMax + b;
                if (z1 < -yMax) return false;
                var z2 = k * xMax + b;
                if (z2 > yMax) return false;
                if (z1 >= -yMax)
                    hit = new Vector2(-xMax, z1);
                else
                    hit = new Vector2((yMax - b) / k, yMax);
                return true;
            }
            return false;
        }
    }
}