using UnityEngine;
using System;
namespace ET
{
    [ObjectSystem]
    public class DirectRectSelectComponentAwakeSystem : AwakeSystem<DirectRectSelectComponent>
    {
        public override void Awake(DirectRectSelectComponent self)
        {
            self.waiter = ETTask<GameObject>.Create(); 
            string path = "GameAssets/SkillPreview/Prefabs/DirectRectSelectManager.prefab";
            GameObjectPoolComponent.Instance.GetGameObjectAsync(path, (obj) =>
            {
                self.gameObject = obj;
                self.DirectObj = obj.transform.GetChild(0).gameObject;
                self.AreaObj = self.DirectObj.transform.GetChild(0).gameObject;
                self.waiter.SetResult(obj);
                self.waiter = null;
            }).Coroutine();
            self.HeroObj = UnitHelper.GetMyUnitFromZoneScene(self.ZoneScene()).GetComponent<GameObjectComponent>().GameObject;
            InputWatcherComponent.Instance.RegisterInputEntity(self);
        }
    }
    [ObjectSystem]
    public class DirectRectSelectComponentUpdateSystem : UpdateSystem<DirectRectSelectComponent>
    {
        public override void Update(DirectRectSelectComponent self)
        {
            if (self.DirectObj == null||!self.IsShow) return;
            self.DirectObj.transform.position = new Vector3( self.HeroObj.transform.position.x, self.HeroObj.transform.position.y,  self.HeroObj.transform.position.z);
            if (RaycastHelper.CastMapPoint(out var hitPoint))
            {
                self.DirectObj.transform.forward = new Vector3(hitPoint.x, hitPoint.y, hitPoint.z) -  self.DirectObj.transform.position;
            }
        }
    }
    [ObjectSystem]
    public class DirectRectSelectComponentDestroySystem : DestroySystem<DirectRectSelectComponent>
    {
        public override void Destroy(DirectRectSelectComponent self)
        {
            GameObjectPoolComponent.Instance?.RecycleGameObject(self.gameObject);
            InputWatcherComponent.Instance?.RemoveInputEntity(self);
        }
    }
        
    [InputSystem((int)KeyCode.Mouse0,InputType.KeyDown,100)]
    public class DirectRectSelectComponentInputSystem_Load : InputSystem<DirectRectSelectComponent>
    {
        public override void Run(DirectRectSelectComponent self, int key, int type, ref bool stop)
        {
            if (self.DirectObj == null||!self.IsShow) return;
            if (RaycastHelper.CastMapPoint(out var hitPoint))
            {
                SelectWatcherComponent.Instance.Hide(self);
                self.OnSelectedCallback?.Invoke(hitPoint);
                stop = true;
            }
           
        }
    }
    [SelectSystem]
    [FriendClass(typeof(DirectRectSelectComponent))]
    public class DirectRectSelectComponentShowSelectSystem : ShowSelectSystem<DirectRectSelectComponent,Action<Vector3>, int[]>
    {
        public override async ETTask OnShow(DirectRectSelectComponent self ,Action<Vector3> onSelectedCallback, int[] previewRange)
        {
            if (previewRange == null || previewRange.Length != 2)
            {
                Log.Error("技能预览配置错误！！！");
                return;
            }
            if (self.waiter != null) await self.waiter;
            self.distance = previewRange[0];
            self.width = previewRange[1];
            self.gameObject.SetActive(true);
            self.OnSelectedCallback = onSelectedCallback;
            self.SetArea(self.distance, self.width);
            self.IsShow = true;
        }
    }

    [SelectSystem]
    [FriendClass(typeof(DirectRectSelectComponent))]
    public class DirectRectSelectComponentHideSelectSystem : HideSelectSystem<DirectRectSelectComponent>
    {
        public override void OnHide(DirectRectSelectComponent self)
        {
            self.IsShow = false;
            if (self.waiter != null) return;
            self.gameObject.SetActive(false);
        }
    }
    [FriendClass(typeof(DirectRectSelectComponent))]
    public static class DirectRectSelectComponentSystem
    {
        public static void SetArea(this DirectRectSelectComponent self,float length, float width)
        {
            self.AreaObj.transform.localScale = new Vector3(width, length, 10);
            self.AreaObj.transform.localPosition = new Vector3(0, 0, length/2);
        }
    }
}