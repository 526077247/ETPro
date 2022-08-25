using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [ObjectSystem]
    public class InfoComponentAwakeSystem : AwakeSystem<InfoComponent>
    {
        public override void Awake(InfoComponent self)
        {
            self.Awake().Coroutine();
        }
    }
    [ObjectSystem]
    public class InfoComponentDestroySystem : DestroySystem<InfoComponent>
    {
        public override void Destroy(InfoComponent self)
        {
            GameObjectPoolComponent.Instance?.RecycleGameObject(self.obj.gameObject);
            self.obj = null;
        }
    }
    [ObjectSystem]
    public class InfoComponentUpdateSystem : UpdateSystem<InfoComponent>
    {
        public override void Update(InfoComponent self)
        {
            self.Update();
        }
    }
    [FriendClass(typeof(InfoComponent))]
    public static class InfoComponentSystem
    {
        public static async ETTask Awake(this InfoComponent self)
        {
            Unit parent = self.GetParent<Unit>();
            var obj = await GameObjectPoolComponent.Instance.GetGameObjectAsync("GameAssets/Info/Prefabs/Info.prefab");
            self.obj = obj.transform as RectTransform;
            self.obj.parent = UIManagerComponent.Instance.GetLayer(UILayerNames.GameLayer).transform;
            self.obj.localScale = Vector3.one;
            self.obj.localPosition = Vector3.zero;
            self.head = parent.GetComponent<GameObjectComponent>().GetCollectorObj<GameObject>("Head").transform;
            self.Num = self.obj.Find("Hp/HPNum").GetComponent<TMPro.TMP_Text>();
            self.HpBg = self.obj.Find("Hp").GetComponent<Image>();
            self.RefreshUI();
            self.Update();
        }

        public static void Update(this InfoComponent self)
        {
            if(self.obj==null) return;
            Vector2 pt = Camera.main.WorldToScreenPoint(self.head.position + new Vector3(0,0.3f,0))*UIManagerComponent.Instance.ScreenSizeflag;
            self.obj.anchoredPosition = pt;
        }
        public static void RefreshUI(this InfoComponent self)
        {
            Unit parent = self.GetParent<Unit>();
            NumericComponent nc = parent.GetComponent<NumericComponent>();
            if (nc == null)
            {
                Log.Info("RefreshHP " + parent.Id + " 上没有添加 NumericComponent组件");
                return;
            }
            self.Num.text = nc.GetAsInt(NumericType.Hp).ToString();
            float fCurrentHpPercent = nc.GetAsFloat(NumericType.Hp) / nc.GetAsFloat(NumericType.MaxHp);
            self.HpBg.fillAmount = fCurrentHpPercent;
        }
        
    }
}