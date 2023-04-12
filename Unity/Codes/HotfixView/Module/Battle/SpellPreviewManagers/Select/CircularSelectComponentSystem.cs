using System;
using UnityEngine;

namespace ET
{
    [FriendClass(typeof(CircularSelectComponent))]
    public static class CircularSelectComponentSystem
    {
        [ObjectSystem]
        public class CircularSelectComponentAwakeSystem : AwakeSystem<CircularSelectComponent>
        {
            public override void Awake(CircularSelectComponent self)
            {
                self.waiter = ETTask<GameObject>.Create(); 
                string path = "GameAssets/SkillPreview/Prefabs/PointSelectManager.prefab";
                GameObjectPoolComponent.Instance.GetGameObjectAsync(path, (obj) =>
                {
                    self.gameObject = obj;
                    self.RangeCircleObj = obj.transform.Find("RangeCircle").gameObject;
                    self.SkillPointObj= obj.transform.Find("SkillPointPreview").gameObject;
                    self.SkillPointObj.SetActive(false);
                    self.waiter.SetResult(obj);
                    self.waiter = null;
                    if (!self.IsShow) return;
                    {
                        self.gameObject.SetActive(false);
                        self.SkillPointObj.SetActive(true);
                    }
                }).Coroutine();
                self.HeroObj = UnitHelper.GetMyUnitFromZoneScene(self.ZoneScene()).GetComponent<GameObjectComponent>().GameObject;
                InputWatcherComponent.Instance.RegisterInputEntity(self);
            }
        }
        [ObjectSystem]
        public class CircularSelectComponentUpdateSystem : UpdateSystem<CircularSelectComponent>
        {
            public override void Update(CircularSelectComponent self)
            {
                if (self.RangeCircleObj == null||!self.IsShow) return;
                self.RangeCircleObj.transform.position = self.HeroObj.transform.position;
            }
        }
        [InputSystem((int)KeyCode.Mouse0,InputType.KeyDown,100)]
        public class CircularSelectComponentInputSystem_Load : InputSystem<CircularSelectComponent>
        {
            public override void Run(CircularSelectComponent self, int key, int type, ref bool stop)
            {
                if (self.RangeCircleObj == null||!self.IsShow) return;
                stop = self.RunCheck();
            }
        }
        [ObjectSystem]
        public class CircularSelectComponentDestroySystem : DestroySystem<CircularSelectComponent>
        {
            public override void Destroy(CircularSelectComponent self)
            {
                GameObjectPoolComponent.Instance?.RecycleGameObject(self.gameObject);
                InputWatcherComponent.Instance?.RemoveInputEntity(self);
            }
        }
        
        [SelectSystem]
        [FriendClass(typeof(CircularSelectComponent))]
        public class CircularSelectComponentShowSelectSystem : ShowSelectSystem<CircularSelectComponent,Action<Vector3>, int[]>
        {
            public override async ETTask OnShow(CircularSelectComponent self ,Action<Vector3> onSelectedCallback, int[] previewRange)
            {
                if (previewRange == null || previewRange.Length != 1)
                {
                    Log.Error("技能预览配置错误！！！");
                    return;
                }
                if (self.waiter != null) await self.waiter;
                self.distance = previewRange[0];
                self.gameObject.SetActive(true);
                self.RangeCircleObj.transform.localScale = Vector3.one*self.distance;
                self.OnSelectPointCallback = onSelectedCallback;
                self.IsShow = true;
                
            }
        }

        [SelectSystem]
        [FriendClass(typeof(CircularSelectComponent))]
        public class CircularSelectComponentHideSelectSystem : HideSelectSystem<CircularSelectComponent>
        {
            public override void OnHide(CircularSelectComponent self)
            {
                self.IsShow = false;
                if (self.waiter != null) return;
                self.gameObject.SetActive(false);
                self.SkillPointObj.SetActive(true);
            }
        }

        [SelectSystem]
        [FriendClass(typeof(CircularSelectComponent))]
        public class CircularSelectComponentAutoSpellSystem : AutoSpellSystem<CircularSelectComponent,Action<Vector3>, int[]>
        {
            public override void OnAutoSpell(CircularSelectComponent self ,Action<Vector3> onSelectedCallback, int[] previewRange)
            {
                if (previewRange == null || previewRange.Length != 1)
                {
                    Log.Error("技能预览配置错误！！！");
                    return;
                }
                self.distance = previewRange[0];
                self.OnSelectPointCallback = onSelectedCallback;
                self.RunCheck();
            }
        }

        public static bool RunCheck(this CircularSelectComponent self)
        {
            
            if (RaycastHelper.CastMapPoint(CameraManagerComponent.Instance.MainCamera(), out var hitPoint))
            {
                var nowpos = self.HeroObj.transform.position;
                if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(hitPoint.x, hitPoint.z)) >
                    self.distance)
                {
                    var dir =new Vector3(hitPoint.x - nowpos.x,0, hitPoint.z - nowpos.z).normalized;
                    hitPoint = nowpos + dir * self.distance;
                }
                SelectWatcherComponent.Instance.Hide(self);
                self.OnSelectPointCallback?.Invoke(nowpos);
                return true;
            }

            return false;
        }
    }
}