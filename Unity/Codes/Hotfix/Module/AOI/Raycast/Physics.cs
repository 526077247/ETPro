using UnityEngine;
using System;
namespace ET
{
    [FriendClass(typeof(AOISceneComponent))]
    [FriendClass(typeof(AOICell))]
    [FriendClass(typeof(AOITrigger))]
    [FriendClass(typeof(AOIUnitComponent))]
    public static class Physics
    {
        public static bool Raycast(AOISceneComponent scene,Ray ray,out RaycastHit hit,UnitType[] type = null)
        {
            hit = default;
            if (type == null) return false;
            using (DictionaryComponent<UnitType, bool> typeTemp = DictionaryComponent<UnitType, bool>.Create())
            {
                using (HashSetComponent<AOITrigger> temp = HashSetComponent<AOITrigger>.Create())
                {
                    for (int i = 0; i < type.Length; i++)
                    {
                        var item = type[i];
                        typeTemp.Add(item, true);
                    }
                    int xIndex = (int) Math.Floor(ray.Start.x / scene.gridLen);
                    int yIndex = (int) Math.Floor(ray.Start.z / scene.gridLen);
                    //z = kx+b
                    float k = 0;
                    float k_1 = 0;
                    float b = 0;
                    if (ray.Dir.x != 0 && ray.Dir.z != 0)
                    {
                        k = ray.Dir.z / ray.Dir.x;
                        k_1 = ray.Dir.x / ray.Dir.z;
                        b = ray.Start.z - k * ray.Start.x;
                    }

                    Vector3 inPoint = ray.Start;
                    while (true)
                    {
                        long cellId = AOIHelper.CreateCellId(xIndex, yIndex);
                        AOICell cell = scene.GetChild<AOICell>(cellId);
                        var xMin = xIndex * scene.gridLen;
                        var xMax = xMin + scene.gridLen;
                        var yMin = yIndex * scene.gridLen;
                        var yMax = yMin + scene.gridLen;
                        //Log.Info("Raycast Check "+xIndex+" "+yIndex);
                        if (cell != null)
                        {
                            ListComponent<RaycastHit> hits = ListComponent<RaycastHit>.Create();
                            RaycastHits(ray, cell, inPoint, hits, temp, typeTemp);
                            if (hits.Count > 0)
                            {
                                hits.KSsort((i1,i2)=> i1.Distance >= i2.Distance?1:-1);//从小到大
                                hit = hits[0];
                                //Log.Info("hits.Count > 0"+hit.Trigger.Parent.Parent.Id);
                                hits.Dispose();
                                return true;
                            }
                        }
                        //一般情况
                        if (ray.Dir.x != 0&& ray.Dir.z != 0)
                        {
                            if (ray.Dir.x > 0 && ray.Dir.z > 0)
                            {
                                var z1 = xMax * k + b;
                                if (z1 > yMin && z1 < yMax)
                                {
                                    xIndex++;
                                    inPoint = new Vector3(xMax, inPoint.y+(xMax-inPoint.x)*ray.Dir.y/ray.Dir.x, z1);
                                }
                                else
                                {
                                    yIndex++;
                                    inPoint = new Vector3((yMax - b) * k_1, inPoint.y+(yMax-inPoint.z)*ray.Dir.y/ray.Dir.z, yMax);
                                }
                            }
                            else if (ray.Dir.x > 0 && ray.Dir.z < 0)
                            {
                                var z1 = xMax * k + b;
                                if (z1 > yMin && z1 < yMax)
                                {
                                    xIndex++;
                                    inPoint = new Vector3(xMax, inPoint.y+(xMax-inPoint.x)*ray.Dir.y/ray.Dir.x, z1);
                                }
                                else
                                {
                                    yIndex--;
                                    inPoint = new Vector3((yMin - b) * k_1, inPoint.y+(yMin-inPoint.z)*ray.Dir.y/ray.Dir.z, yMin);
                                }
                            }
                            else if (ray.Dir.x < 0 && ray.Dir.z < 0)
                            {
                                var z1 = xMin * k + b;
                                if (z1 > yMin && z1 < yMax)
                                {
                                    xIndex--;
                                    inPoint = new Vector3(xMin, inPoint.y+(xMin-inPoint.x)*ray.Dir.y/ray.Dir.x, z1);
                                }
                                else
                                {
                                    yIndex--;
                                    inPoint = new Vector3((yMin - b) * k_1, inPoint.y+(yMin-inPoint.z)*ray.Dir.y/ray.Dir.z, yMin);
                                }
                            }
                            else if (ray.Dir.x < 0 && ray.Dir.z > 0)
                            {
                                var z1 = xMin * k + b;
                                if (z1 > yMin && z1 < yMax)
                                {
                                    xIndex--;
                                    inPoint = new Vector3(xMin, inPoint.y+(xMin-inPoint.x)*ray.Dir.y/ray.Dir.x, z1);
                                }
                                else
                                {
                                    yIndex++;
                                    inPoint = new Vector3((yMax - b) * k_1, inPoint.y+(yMax-inPoint.z)*ray.Dir.y/ray.Dir.z, yMax);
                                }
                            }
                            else
                            {
                                Log.Error("What's fuck???");
                            }
                        }
                        //平行于轴了
                        else if (ray.Dir.x == 0&& ray.Dir.z != 0)
                        {
                            if (ray.Dir.z > 0)
                            {
                                yIndex++;
                                inPoint = new Vector3(inPoint.x, inPoint.y+(yMax-inPoint.z)*ray.Dir.y/ray.Dir.z, yMax);
                            }
                            else
                            {
                                yIndex--;
                                inPoint = new Vector3(inPoint.x, inPoint.y+(yMin-inPoint.z)*ray.Dir.y/ray.Dir.z, yMin);
                            }
                        }
                        else if (ray.Dir.z == 0&& ray.Dir.x != 0)
                        {
                            if (ray.Dir.x > 0)
                            {
                                xIndex++;
                                inPoint = new Vector3(xMax, inPoint.y+(xMax-inPoint.x)*ray.Dir.y/ray.Dir.x, inPoint.z);
                            }
                            else
                            {
                                xIndex--;
                                inPoint = new Vector3(xMin, inPoint.y+(xMin-inPoint.x)*ray.Dir.y/ray.Dir.x, inPoint.z);
                            }
                        }
                        //垂直于地图
                        else
                            break;
                        if(Vector3.SqrMagnitude(inPoint-ray.Start)>ray.SqrDistance)
                            break;
                    }
                }
            }
            return false;
        }

        private static void RaycastHits(Ray ray, AOICell cell,Vector3 inPoint,ListComponent<RaycastHit> hits,
            HashSetComponent<AOITrigger> triggers, DictionaryComponent<UnitType, bool> type)
        {
            for (int i = cell.Colliders.Count-1; i >=0 ; i--)
            {
                var item = cell.Colliders[i];
                if (item.IsDisposed)
                {
                    cell.Colliders.RemoveAt(i);
                    Log.Warning("自动移除不成功");
                    continue;
                }
                if (item.IsCollider &&!triggers.Contains(item)&& type.ContainsKey(UnitType.ALL) ||
                    type.ContainsKey(item.GetParent<AOIUnitComponent>().Type))
                {
                    if (item.IsPointInTrigger(inPoint, item.GetRealPos(), item.GetRealRot()))
                    {
                        triggers.Add(item);
                        hits.Add(new RaycastHit
                        {
                            Hit = inPoint,
                            Trigger = item,
                            Distance = Vector3.Distance(inPoint,ray.Start)
                        });
                    }
                    else if (item.IsRayInTrigger(ray,item.GetRealPos(),item.GetRealRot(),out var hit))
                    {
                        triggers.Add(item);
                        hits.Add(new RaycastHit
                        {
                            Hit = hit,
                            Trigger = item,
                            Distance = Vector3.Distance(hit,ray.Start)
                        });
                    }
                }
            }
        }
        
        //todo:
        private static RaycastHit[] RaycastAll(AOISceneComponent scene,Ray ray,UnitType[] type = null)
        {
            return null;
        }
    }
}