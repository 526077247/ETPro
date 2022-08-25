

using UnityEngine;

namespace ET
{
	[FriendClass(typeof(ToastComponent))]
	public class ShowToastEvent_CreateToastUI : AEventAsync<UIEventType.ShowToast>
	{
		protected override async ETTask Run(UIEventType.ShowToast args)
		{
			await Show(args.Text);
		}

        async ETTask Show(string Content, int seconds = 3)
        {
            GameObject gameObject = await GameObjectPoolComponent.Instance.GetGameObjectAsync("UI/UIToast/Prefabs/UIToast.prefab");
            UIToast ui = ToastComponent.Instance.AddChild<UIToast>();
            var transform = gameObject.transform;
            ui.AddUIComponent<UITransform,Transform>("", transform);
            transform = gameObject.transform;
            transform.SetParent(ToastComponent.Instance.root);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = new Vector3(1, 1, 1);
            UIWatcherComponent.Instance.OnCreate(ui);
            UIWatcherComponent.Instance.OnEnable(ui,Content);
            await TimerComponent.Instance.WaitAsync(seconds*1000);
            ui.BeforeOnDestroy();
            UIWatcherComponent.Instance.OnDestroy(ui);
            GameObjectPoolComponent.Instance.RecycleGameObject(gameObject);
        }

    }
}
