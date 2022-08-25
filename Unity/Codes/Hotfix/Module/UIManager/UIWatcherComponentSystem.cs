using System;
using System.Collections.Generic;

namespace ET
{
	using OneTypeSystems = UnOrderMultiMap<Type, object>;
	[FriendClass(typeof(UIWatcherComponent))]
    public static class UIWatcherComponentSystem
    {
	    public class UIWatcherComponentAwakeSystem:AwakeSystem<UIWatcherComponent>
	    {
		    public override void Awake(UIWatcherComponent self)
		    {
			    UIWatcherComponent.Instance = self;
			    self.Init();
		    }
	    }
	    public class UIWatcherComponentLoadSystem : LoadSystem<UIWatcherComponent>
	    {
		    public override void Load(UIWatcherComponent self)
		    {
			    self.Init();
		    }
	    }

	    public static void Init(this UIWatcherComponent self)
	    {
		    self.typeSystems = new TypeSystems();
		    foreach (Type type in Game.EventSystem.GetTypes(typeof(UISystemAttribute)))
		    {
			    object obj = Activator.CreateInstance(type);

			    if (obj is ISystemType iSystemType)
			    {
				    OneTypeSystems oneTypeSystems = self.typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
				    oneTypeSystems.Add(iSystemType.SystemType(), obj);
			    }
		    }
	    }
	    
		#region OnCreate
		public static void OnCreate(this UIWatcherComponent self,Entity component)
		{
			self.RegisterI18N(component);
			List<object> iOnCreateSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnCreateSystem));
			if (iOnCreateSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnCreateSystems.Count; i++)
			{
				IOnCreateSystem aOnCreateSystem = (IOnCreateSystem)iOnCreateSystems[i];
				if (aOnCreateSystem == null)
				{
					continue;
				}

				try
				{
					aOnCreateSystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void OnCreate<P1>(this UIWatcherComponent self,Entity component, P1 p1)
		{
			self.RegisterI18N(component);
			List<object> iOnCreateSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnCreateSystem<P1>));
			if (iOnCreateSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnCreateSystems.Count; i++)
			{
				IOnCreateSystem<P1> aOnCreateSystem = (IOnCreateSystem<P1>)iOnCreateSystems[i];
				if (aOnCreateSystem == null)
				{
					continue;
				}

				try
				{
					aOnCreateSystem.Run(component, p1);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void OnCreate<P1, P2>(this UIWatcherComponent self,Entity component, P1 p1, P2 p2)
		{
			self.RegisterI18N(component);
			List<object> iOnCreateSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnCreateSystem<P1, P2>));
			if (iOnCreateSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnCreateSystems.Count; i++)
			{
				IOnCreateSystem<P1, P2> aOnCreateSystem = (IOnCreateSystem<P1, P2>)iOnCreateSystems[i];
				if (aOnCreateSystem == null)
				{
					continue;
				}

				try
				{
					aOnCreateSystem.Run(component, p1, p2);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void OnCreate<P1, P2, P3>(this UIWatcherComponent self,Entity component, P1 p1, P2 p2, P3 p3)
		{
			self.RegisterI18N(component);
			List<object> iOnCreateSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnCreateSystem<P1, P2, P3>));
			if (iOnCreateSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnCreateSystems.Count; i++)
			{
				IOnCreateSystem<P1, P2, P3> aOnCreateSystem = (IOnCreateSystem<P1, P2, P3>)iOnCreateSystems[i];
				if (aOnCreateSystem == null)
				{
					continue;
				}

				try
				{
					aOnCreateSystem.Run(component, p1, p2, p3);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void OnCreate<P1, P2, P3, P4>(this UIWatcherComponent self,Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			self.RegisterI18N(component);
			List<object> iOnCreateSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnCreateSystem<P1, P2, P3, P4>));
			if (iOnCreateSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnCreateSystems.Count; i++)
			{
				IOnCreateSystem<P1, P2, P3, P4> aOnCreateSystem = (IOnCreateSystem<P1, P2, P3, P4>)iOnCreateSystems[i];
				if (aOnCreateSystem == null)
				{
					continue;
				}

				try
				{
					aOnCreateSystem.Run(component, p1, p2, p3, p4);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		#endregion

		#region OnEnable
		public static void OnEnable(this UIWatcherComponent self,Entity component)
		{
			
			List<object> iOnEnableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnEnableSystem));
			if (iOnEnableSystems != null)
			{
				for (int i = 0; i < iOnEnableSystems.Count; i++)
				{
					IOnEnableSystem aOnEnableSystem = (IOnEnableSystem)iOnEnableSystems[i];
					if (aOnEnableSystem == null)
					{
						continue;
					}

					try
					{
						aOnEnableSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
		}

		public static void OnEnable<P1>(this UIWatcherComponent self,Entity component, P1 p1)
		{
			List<object> iOnEnableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnEnableSystem<P1>));
			if (iOnEnableSystems != null)
			{
				for (int i = 0; i < iOnEnableSystems.Count; i++)
				{
					IOnEnableSystem<P1> aOnEnableSystem = (IOnEnableSystem<P1>)iOnEnableSystems[i];
					if (aOnEnableSystem == null)
					{
						continue;
					}

					try
					{
						aOnEnableSystem.Run(component, p1);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
		}

		public static void OnEnable<P1, P2>(this UIWatcherComponent self,Entity component, P1 p1, P2 p2)
		{
			List<object> iOnEnableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnEnableSystem<P1, P2>));
			if (iOnEnableSystems != null)
			{
				for (int i = 0; i < iOnEnableSystems.Count; i++)
				{
					IOnEnableSystem<P1, P2> aOnEnableSystem = (IOnEnableSystem<P1, P2>)iOnEnableSystems[i];
					if (aOnEnableSystem == null)
					{
						continue;
					}

					try
					{
						aOnEnableSystem.Run(component, p1, p2);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
		}

		public static void OnEnable<P1, P2, P3>(this UIWatcherComponent self,Entity component, P1 p1, P2 p2, P3 p3)
		{
			List<object> iOnEnableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnEnableSystem<P1, P2, P3>));
			if (iOnEnableSystems != null)
			{
				for (int i = 0; i < iOnEnableSystems.Count; i++)
				{
					IOnEnableSystem<P1, P2, P3> aOnEnableSystem = (IOnEnableSystem<P1, P2, P3>)iOnEnableSystems[i];
					if (aOnEnableSystem == null)
					{
						continue;
					}

					try
					{
						aOnEnableSystem.Run(component, p1, p2, p3);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
		}

		public static void OnEnable<P1, P2, P3, P4>(this UIWatcherComponent self,Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			List<object> iOnEnableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnEnableSystem<P1, P2, P3, P4>));
			if (iOnEnableSystems != null)
			{
				for (int i = 0; i < iOnEnableSystems.Count; i++)
				{
					IOnEnableSystem<P1, P2, P3, P4> aOnEnableSystem = (IOnEnableSystem<P1, P2, P3, P4>)iOnEnableSystems[i];
					if (aOnEnableSystem == null)
					{
						continue;
					}

					try
					{
						aOnEnableSystem.Run(component, p1, p2, p3, p4);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
		}

		#endregion

		#region OnDisable
		public static void OnDisable(this UIWatcherComponent self,Entity component)
		{
			List<object> iOnDisableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnDisableSystem));
			if (iOnDisableSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnDisableSystems.Count; i++)
			{
				IOnDisableSystem aOnDisableSystem = (IOnDisableSystem)iOnDisableSystems[i];
				if (aOnDisableSystem == null)
				{
					continue;
				}

				try
				{
					aOnDisableSystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void OnDisable<P1>(this UIWatcherComponent self,Entity component, P1 p1)
		{
			List<object> iOnDisableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnDisableSystem<P1>));
			if (iOnDisableSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnDisableSystems.Count; i++)
			{
				IOnDisableSystem<P1> aOnDisableSystem = (IOnDisableSystem<P1>)iOnDisableSystems[i];
				if (aOnDisableSystem == null)
				{
					continue;
				}

				try
				{
					aOnDisableSystem.Run(component, p1);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void OnDisable<P1, P2>(this UIWatcherComponent self,Entity component, P1 p1, P2 p2)
		{
			List<object> iOnDisableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnDisableSystem<P1, P2>));
			if (iOnDisableSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnDisableSystems.Count; i++)
			{
				IOnDisableSystem<P1, P2> aOnDisableSystem = (IOnDisableSystem<P1, P2>)iOnDisableSystems[i];
				if (aOnDisableSystem == null)
				{
					continue;
				}

				try
				{
					aOnDisableSystem.Run(component, p1, p2);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void OnDisable<P1, P2, P3>(this UIWatcherComponent self,Entity component, P1 p1, P2 p2, P3 p3)
		{
			List<object> iOnDisableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnDisableSystem<P1, P2, P3>));
			if (iOnDisableSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnDisableSystems.Count; i++)
			{
				IOnDisableSystem<P1, P2, P3> aOnDisableSystem = (IOnDisableSystem<P1, P2, P3>)iOnDisableSystems[i];
				if (aOnDisableSystem == null)
				{
					continue;
				}

				try
				{
					aOnDisableSystem.Run(component, p1, p2, p3);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static void OnDisable<P1, P2, P3, P4>(this UIWatcherComponent self,Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			List<object> iOnDisableSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnDisableSystem<P1, P2, P3, P4>));
			if (iOnDisableSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnDisableSystems.Count; i++)
			{
				IOnDisableSystem<P1, P2, P3, P4> aOnDisableSystem = (IOnDisableSystem<P1, P2, P3, P4>)iOnDisableSystems[i];
				if (aOnDisableSystem == null)
				{
					continue;
				}

				try
				{
					aOnDisableSystem.Run(component, p1, p2, p3, p4);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		#endregion

		#region OnDestroy
		public static void OnDestroy(this UIWatcherComponent self,Entity component)
		{
			self.RemoveI18N(component);
			List<object> iOnDestroySystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnDestroySystem));
			if (iOnDestroySystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnDestroySystems.Count; i++)
			{
				IOnDestroySystem aOnDestroySystem = (IOnDestroySystem)iOnDestroySystems[i];
				if (aOnDestroySystem == null)
				{
					continue;
				}

				try
				{
					aOnDestroySystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		#endregion
		
		#region I18N

		

		public static void RegisterI18N(this UIWatcherComponent self,Entity component)
		{
			if (self.CheckIsI18N(component))
			{
				EventSystem.Instance.Publish(new UIEventType.RegisterI18NEntity() {entity = component});
			}
		}
		public static void RemoveI18N(this UIWatcherComponent self,Entity component)
		{
			if (self.CheckIsI18N(component))
			{
				EventSystem.Instance.Publish(new UIEventType.RemoveI18NEntity {entity = component});
			}
		}
		public static bool CheckIsI18N(this UIWatcherComponent self,Entity component)
		{
			var type = component.GetType();
			if (self.I18NCheckRes.ContainsKey(type)) return self.I18NCheckRes[type];
			if (!(component is II18N))
			{
				self.I18NCheckRes[type] = false;
				return false;
			}
			List<object> iI18NSystems = self.typeSystems.GetSystems(type, typeof(II18NSystem));
			if (iI18NSystems == null)
			{
				self.I18NCheckRes[type] = false;
				return false;
			}
			for (int i = 0; i < iI18NSystems.Count; i++)
			{
				II18NSystem aI18NSystem = (II18NSystem)iI18NSystems[i];
				if (aI18NSystem != null)
				{
					self.I18NCheckRes[type] = true;
					return true;
				}
			}
			self.I18NCheckRes[type] = false;
			return false;
		}
		public static void OnLanguageChange(this UIWatcherComponent self,Entity component)
		{
			List<object> iI18NSystems = self.typeSystems.GetSystems(component.GetType(), typeof(II18NSystem));
			if (iI18NSystems == null)
			{
				return;
			}

			for (int i = 0; i < iI18NSystems.Count; i++)
			{
				II18NSystem aI18NSystem = (II18NSystem)iI18NSystems[i];
				if (aI18NSystem == null)
				{
					continue;
				}

				try
				{
					aI18NSystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
		#endregion
		public static async ETTask OnViewInitializationSystem(this UIWatcherComponent self,Entity component)
		{
			List<object> iOnViewInitializationSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnViewInitializationSystem));
			if (iOnViewInitializationSystems == null)
			{
				return;
			}

			for (int i = 0; i < iOnViewInitializationSystems.Count; i++)
			{
				IOnViewInitializationSystem aOnViewInitializationSystem = (IOnViewInitializationSystem)iOnViewInitializationSystems[i];
				if (aOnViewInitializationSystem == null)
				{
					continue;
				}

				try
				{
					await aOnViewInitializationSystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
		
		public static void OnChangeRedDotActive(this UIWatcherComponent self,Entity component,int count)
		{
			List<object> iRedDotSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IRedDotSystem));
			if (iRedDotSystems == null)
			{
				return;
			}

			for (int i = 0; i < iRedDotSystems.Count; i++)
			{
				IRedDotSystem aRedDotSystem = (IRedDotSystem)iRedDotSystems[i];
				if (aRedDotSystem == null)
				{
					continue;
				}

				try
				{
					aRedDotSystem.Run(component,count);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
		
		public static bool OnWidthPaddingChange(this UIWatcherComponent self,IOnWidthPaddingChange component)
		{
			
			List<object> iOnWidthPaddingChangeSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IOnWidthPaddingChangeSystem));
			if (iOnWidthPaddingChangeSystems == null)
			{
				return false;
			}
			bool res = false;
			for (int i = 0; i < iOnWidthPaddingChangeSystems.Count; i++)
			{
				IOnWidthPaddingChangeSystem aOnWidthPaddingChangeSystem = (IOnWidthPaddingChangeSystem)iOnWidthPaddingChangeSystems[i];
				if (aOnWidthPaddingChangeSystem == null)
				{
					continue;
				}

				try
				{
					res = true;
					aOnWidthPaddingChangeSystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}

			return res;
		}
    }
}