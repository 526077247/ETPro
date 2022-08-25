using UnityEngine;
using System;
namespace ET
{
    [ObjectSystem]
    public class PointSelectManagerAwakeSystem : AwakeSystem<PointSelectComponent>
    {
        public override void Awake(PointSelectComponent self)
        {
            self.waiter = ETTask<GameObject>.Create(); 
            string path = "GameAssets/SkillPreview/Prefabs/PointSelectManager.prefab";
            GameObjectPoolComponent.Instance.GetGameObjectAsync(path, (obj) =>
            {
                self.gameObject = obj;
                self.RangeCircleObj = obj.transform.Find("RangeCircle").gameObject;
                self.SkillPointObj= obj.transform.Find("SkillPointPreview").gameObject;
                self.waiter.SetResult(obj);
                self.waiter = null;
            }).Coroutine();
            self.HeroObj = UnitHelper.GetMyUnitFromZoneScene(self.ZoneScene()).GetComponent<GameObjectComponent>().GameObject;
            InputWatcherComponent.Instance.RegisterInputEntity(self);
        }
    }
    [ObjectSystem]
    public class PointSelectManagerUpdateSystem : UpdateSystem<PointSelectComponent>
    {
        public override void Update(PointSelectComponent self)
        {
            if (self.RangeCircleObj == null||!self.IsShow) return;
            self.RangeCircleObj.transform.position = self.HeroObj.transform.position;
            if (RaycastHelper.CastMapPoint(out var hitPoint))
            {
                var nowpos = self.HeroObj.transform.position;
                if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(hitPoint.x, hitPoint.z)) >
                    self.distance)
                {
                    var dir =new Vector3(hitPoint.x - nowpos.x,0, hitPoint.z - nowpos.z).normalized;
                    hitPoint = nowpos + dir * self.distance;
                }
                self.SkillPointObj.transform.position = new Vector3(hitPoint.x, hitPoint.y, hitPoint.z);
            }
        }
    }
    [InputSystem((int)KeyCode.Mouse0,InputType.KeyDown,100)]
    public class PointSelectComponentInputSystem_Load : InputSystem<PointSelectComponent>
    {
        public override void Run(PointSelectComponent self, int key, int type, ref bool stop)
        {
            if (self.RangeCircleObj == null||!self.IsShow) return;
            if (RaycastHelper.CastMapPoint(out var hitPoint))
            {
                var nowpos = self.HeroObj.transform.position;
                if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(hitPoint.x, hitPoint.z)) >
                    self.distance)
                {
                    var dir =new Vector3(hitPoint.x - nowpos.x,0, hitPoint.z - nowpos.z).normalized;
                    hitPoint = nowpos + dir * self.distance;
                }
                SelectWatcherComponent.Instance.Hide(self);
                self.OnSelectPointCallback?.Invoke(hitPoint);
                stop = true;
            }
           
        }
    }
    [ObjectSystem]
    public class PointSelectManagerDestroySystem : DestroySystem<PointSelectComponent>
    {
        public override void Destroy(PointSelectComponent self)
        {
            GameObjectPoolComponent.Instance?.RecycleGameObject(self.gameObject);
            InputWatcherComponent.Instance?.RemoveInputEntity(self);
        }
    }
    
    [SelectSystem]
    [FriendClass(typeof(PointSelectComponent))]
    public class PointSelectComponentShowSelectSystem : ShowSelectSystem<PointSelectComponent,Action< Vector3>, int[]>
    {
        public override async ETTask OnShow(PointSelectComponent self ,Action< Vector3> onSelectedCallback, int[] previewRange)
        {
            if (previewRange == null || previewRange.Length != 2)
            {
                Log.Error("技能预览配置错误！！！");
                return;
            }
            if (self.waiter != null) await self.waiter;
            self.distance = previewRange[0];
            self.range = previewRange[1];
            self.gameObject.SetActive(true);
            self.RangeCircleObj.transform.localScale = Vector3.one*self.distance;
            self.SkillPointObj.transform.localScale = Vector3.one*self.range;
            self.OnSelectPointCallback = onSelectedCallback;
            self.IsShow = true;
            
        }
    }

    [SelectSystem]
    [FriendClass(typeof(PointSelectComponent))]
    public class PointSelectComponentHideSelectSystem : HideSelectSystem<PointSelectComponent>
    {
        public override void OnHide(PointSelectComponent self)
        {
            self.IsShow = false;
            if (self.waiter != null) return;
            self.gameObject.SetActive(false);
        }
    }


    public static class PointSelectComponentSystem
    {
        
    }
}