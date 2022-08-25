using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [ObjectSystem]
    public class TargetSelectComponentAwakeSystem : AwakeSystem<TargetSelectComponent>
    {
        public override void Awake(TargetSelectComponent self)
        {
            //CursorImage = GetComponent<Image>();
            self.CursorColor = Color.white;
            self.waiter = ETTask<GameObject>.Create(); 
            
            self.Init().Coroutine();
             
            self.HeroObj = UnitHelper.GetMyUnitFromZoneScene(self.ZoneScene()).GetComponent<GameObjectComponent>().GameObject;
            InputWatcherComponent.Instance.RegisterInputEntity(self);
        }
    }
    [ObjectSystem]
    [FriendClass(typeof(AOIUnitComponent))]
    public class TargetSelectComponentUpdateSystem : UpdateSystem<TargetSelectComponent>
    {
        public override void Update(TargetSelectComponent self)
        {
            if (self.RangeCircleObj == null||!self.IsShow) return;
            self.RangeCircleObj.transform.position = self.HeroObj.transform.position;
            self.CursorImage.rectTransform.anchoredPosition = Input.mousePosition*UIManagerComponent.Instance.ScreenSizeflag;
            
            if (RaycastHelper.CastUnitObj(out var obj))
            {
                var uidC = obj.GetComponentInParent<UnitIdComponent>();
                if (uidC != null)
                {
                    var unit = self.ZoneScene().CurrentScene().GetComponent<UnitComponent>()?.GetChild<Unit>(uidC.UnitId);
                    var canUse = self.CanSkillToUnit(unit);
                    if (canUse)
                    {
                        if (self.TargetLimitType == SkillAffectTargetType.EnemyTeam)
                        {
                            self.CursorImage.color = Color.red;
                        }
                        else if (self.TargetLimitType == SkillAffectTargetType.SelfTeam||self.TargetLimitType == SkillAffectTargetType.Self)
                        {
                            self.CursorImage.color = Color.green;
                        }
                        return;
                    }
                }
            }
            self.CursorImage.color = self.CursorColor;
            
        }
    }
    [InputSystem((int)KeyCode.Mouse0,InputType.KeyDown,100)]
    public class TargetSelectComponentInputSystem_Load : InputSystem<TargetSelectComponent>
    {
        public override void Run(TargetSelectComponent self, int key, int type, ref bool stop)
        {
            if (self.RangeCircleObj == null||!self.IsShow) return;
            if (RaycastHelper.CastUnitObj(out var obj))
            {
                var uidC = obj.GetComponentInParent<UnitIdComponent>();
                if (uidC != null)
                {
                    var unit = self.ZoneScene().CurrentScene().GetComponent<UnitComponent>()?.GetChild<Unit>(uidC.UnitId);
                    var canUse = self.CanSkillToUnit(unit);
                    if (canUse)
                    {
                        SelectWatcherComponent.Instance.Hide(self);
                        self.OnSelectTargetCallback?.Invoke(unit);
                        stop = true;
                        return;
                    }
                }
            }
            SelectWatcherComponent.Instance.Hide(self);
        }
    }
    [ObjectSystem]
    public class TargetSelectComponentDestroySystem : DestroySystem<TargetSelectComponent>
    {
        public override void Destroy(TargetSelectComponent self)
        {
            GameObjectPoolComponent.Instance?.RecycleGameObject(self.gameObject);
            GameObjectPoolComponent.Instance?.RecycleGameObject(self.CursorImage.gameObject);
            InputWatcherComponent.Instance?.RemoveInputEntity(self);
        }
    }
    [SelectSystem]
    [FriendClass(typeof(TargetSelectComponent))]
    public class TargetSelectComponentShowSelectSystem : ShowSelectSystem<TargetSelectComponent,Action<Unit>, int[]>
    {
        public override async ETTask OnShow(TargetSelectComponent self ,Action<Unit> onSelectedCallback, int[] previewRange)
        {
            if (previewRange == null || previewRange.Length != 1)
            {
                Log.Error("技能预览配置错误！！！");
                return;
            }
            if (self.waiter != null) await self.waiter;
            self.distance = previewRange[0];
            self.gameObject.SetActive(true);
            Cursor.visible = false;
            self.CursorImage.gameObject.SetActive(true);
            self.RangeCircleObj.transform.localScale = Vector3.one*self.distance;
            self.OnSelectTargetCallback = onSelectedCallback;
            self.IsShow = true;
        }
    }

    [SelectSystem]
    [FriendClass(typeof(TargetSelectComponent))]
    public class TargetSelectComponentHideSelectSystem : HideSelectSystem<TargetSelectComponent>
    {
        public override void OnHide(TargetSelectComponent self)
        {
            self.IsShow = false;
            if (self.waiter != null) return;
            Cursor.visible = true;
            self.CursorImage.gameObject.SetActive(false);
            self.gameObject.SetActive(false);
        }
    }
    [FriendClass(typeof(TargetSelectComponent))]
    public static class TargetSelectComponentSystem
    {
        public static async ETTask Init(this TargetSelectComponent self)
        {
            string path = "GameAssets/SkillPreview/Prefabs/TargetSelectManager.prefab";
            string targetPath = "GameAssets/SkillPreview/Prefabs/TargetIcon.prefab";
            using (ListComponent<ETTask<GameObject>> tasks = ListComponent<ETTask<GameObject>>.Create())
            {
                tasks.Add(GameObjectPoolComponent.Instance.GetGameObjectAsync(targetPath, (obj) =>
                {
                    self.CursorImage = obj.GetComponent<Image>();
                    self.CursorImage.transform.parent =
                            UIManagerComponent.Instance.GetLayer(UILayerNames.TipLayer).transform;
                    self.CursorImage.transform.localPosition = Vector3.zero;
                    self.CursorImage.rectTransform.anchoredPosition = Input.mousePosition;
                }));
                tasks.Add(GameObjectPoolComponent.Instance.GetGameObjectAsync(path, (obj) =>
                {

                    self.RangeCircleObj = obj.transform.Find("RangeCircle").gameObject;
                    self.gameObject = obj;
                }));
                await ETTaskHelper.WaitAll(tasks);
                self.waiter.SetResult(self.gameObject);
                self.waiter = null;
            }

        }

        public static Ray GetRay(this TargetSelectComponent self,float dis = 100f)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return new Ray
            {
                Dir = ray.direction,
                Start = ray.origin,
                Distance = dis
            };
        }

        public static bool CanSkillToUnit(this TargetSelectComponent self,Unit unit)
        {
            // 根据UnitType判断
            // var aoiU = unit?.GetComponent<AOIUnitComponent>();
            // if (aoiU == null) return false;
            //
            // UnitType[] res = null;
            // if (self.TargetLimitType == SkillAffectTargetType.EnemyTeam)
            //     res = new []{ UnitType.Monster};
            // else if (self.TargetLimitType == SkillAffectTargetType.SelfTeam||self.TargetLimitType == SkillAffectTargetType.Self)
            //     res = new []{ UnitType.Player};
            // for (int i = 0; i < res.Length; i++)
            // {
            //     if (res[i] == aoiU.Type || res[i] == UnitType.ALL)
            //     {
            //         if (self.Mode == 0)
            //         {
            //             var pos1 = new Vector2(unit.Position.x, unit.Position.z);
            //             var pos2 = new Vector2(self.HeroObj.transform.position.x, self.HeroObj.transform.position.z);
            //             if (Vector2.Distance(pos1, pos2) >= self.distance)
            //             {
            //                 return false;
            //             }
            //         }
            //         return true;
            //     }
            // }
            // return false;
            
            //测试，只要不是自己就是敌人
            if (self.Mode == 0)
            {
                var pos1 = new Vector2(unit.Position.x, unit.Position.z);
                var pos2 = new Vector2(self.HeroObj.transform.position.x, self.HeroObj.transform.position.z);
                if (Vector2.Distance(pos1, pos2) >= self.distance)
                {
                    return false;
                }
            }

            if (self.TargetLimitType == SkillAffectTargetType.EnemyTeam)
                return unit.Id != self.Id;
            if (self.TargetLimitType == SkillAffectTargetType.SelfTeam||self.TargetLimitType == SkillAffectTargetType.Self)
                return unit.Id != self.Id;
            return false;
        }
    }
}