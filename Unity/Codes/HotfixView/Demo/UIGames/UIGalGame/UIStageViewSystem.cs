using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace ET
{
	[UISystem]
	[FriendClass(typeof(UIStageView))]
	public class UIStageViewOnCreateSystem:OnCreateSystem<UIStageView>
	{
		public override void OnCreate(UIStageView self)
		{
			self.infos = new Dictionary<string, UIStageRoleInfo>();
		}
	}
	[UISystem]
	[FriendClass(typeof(UIStageView))]
	public class UIStageViewOnEnableSystem:OnEnableSystem<UIStageView,GalGameEngineComponent>
	{
		public override void OnEnable(UIStageView self,GalGameEngineComponent t)
		{
			self.Engine = t;
			self.Refresh().Coroutine();
		}
	}
	[UISystem]
	[FriendClass(typeof(UIStageView))]
	public class UIStageViewOnEnableSystem2:OnEnableSystem<UIStageView,GalGameEngineComponent,GalGameEnginePara>
	{
		public override void OnEnable(UIStageView self,GalGameEngineComponent t,GalGameEnginePara p )
		{
			self.Engine = t;
			self.Refresh(p).Coroutine();
		}
	}
	[UISystem]
	[FriendClass(typeof(UIStageView))]
	public class UIStageViewOnDestroySystem: OnDestroySystem<UIStageView>
	{
		public override void OnDestroy(UIStageView self)
		{
			foreach (var item in self.infos)
			{
				//销毁前先回收掉
				GameObjectPoolComponent.Instance?.RecycleUIGameObject(item.Value);
			}
		}
	}
	[FriendClass(typeof(UIStageView))]
	[FriendClass(typeof(UIStageRoleInfo))]
	[FriendClass(typeof(GalGameEngineComponent))]
	public static class UIStageViewSystem
	{
		public static async ETTask Refresh(this UIStageView self,GalGameEnginePara chapter = default)
		{
			foreach (var item in self.infos)//先全部隐藏
			{
				item.Value.active = false;
			}

			var keys = self.Engine.StageRoleMap.Keys.ToList();
			for (int i = 0; i < keys.Count; i++)
			{
				var key = keys[i];
				if (!self.Engine.StageRoleMap.TryGetValue(key, out var value))
				{
					continue;
				}
				var Expression = self.Engine.RoleExpressionMap[value];
				//todo: 角色上场
				if (self.Engine.RoleExpressionPathMap.TryGetValue(value, out var ExpressionPath))
				{
					if (!ExpressionPath.TryGetValue(Expression, out var path))
					{
						foreach (var exp in ExpressionPath)
						{
							Log.Debug("ExpressionPath not found:" + value + Expression + ", use " + exp.Value + " replace");
							path = exp.Value;
							break;
						}
					}
					if (!string.IsNullOrEmpty(path))
					{
						if (!self.infos.TryGetValue(key, out var info))
						{
							info = await GameObjectPoolComponent.Instance.GetUIGameObjectAsync<UIStageRoleInfo>("UIGames/UIGalGame/Prefabs/Character.prefab");
							if (self.infos.ContainsKey(key))//异步加载回来已经有了
							{
								GameObjectPoolComponent.Instance?.RecycleUIGameObject(info);
								info = self.infos[key];
							}
							else
							{
								var pos = self.Engine.StagePosMap[key];
								info.GetTransform().parent = self.GetTransform();
								info.GetTransform().localPosition = pos;
								info.GetTransform().localScale = new Vector3(1, 1, 1);
								self.infos[key] = info;
							}
							info.active = true;
						}
						else
						{
							info.active = true;
							if (info.path == path) continue; //没变化直接跳过
						}
						info.image.SetSpritePath(path).Coroutine();
						info.path = path;
						if (string.Equals(chapter.Command, "CharacterOn", StringComparison.OrdinalIgnoreCase))//入场
						{
							string newUser = chapter.Arg1;
							if (string.Equals(newUser, value, StringComparison.OrdinalIgnoreCase))
							{
								string type = chapter.Arg5;
								if (string.Equals(type, "FadeIn", StringComparison.OrdinalIgnoreCase))
								{
									info.image.SetImageColor(Color.clear);
									float time = float.Parse(chapter.Arg6);
									DOTween.To(() => info.image.GetImageColor(),
										x => info.image.SetImageColor(x), Color.white, time);
								}
							}
						}
						else if (string.Equals(chapter.Command, "CharacterOff", StringComparison.OrdinalIgnoreCase))//入场
						{
							string newUser = chapter.Arg1;
							if (string.Equals(newUser, value, StringComparison.OrdinalIgnoreCase))
							{
								string type = chapter.Arg5;
								if (string.Equals(type, "FadeOut", StringComparison.OrdinalIgnoreCase))
								{
									info.image.SetImageColor(Color.white);
									float time = float.Parse(chapter.Arg6);
									DOTween.To(() => info.image.GetImageColor(),
										x => info.image.SetImageColor(x), Color.clear, time);
								}
							}
						}
					}
					else
						Log.Debug("path is null:" + value + Expression);
					
				}
				else
					Log.Debug("RoleExpressionPathMap not found:" + value);
			}
			foreach (var item in self.infos)//修改显隐状态
			{
				item.Value.SetActive(item.Value.active);
			}
		}
	}
}
