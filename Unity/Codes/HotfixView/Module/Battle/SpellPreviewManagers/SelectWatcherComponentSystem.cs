using System;
using System.Collections.Generic;
namespace ET
{
	using OneTypeSystems = UnOrderMultiMap<Type, object>;

    [FriendClass(typeof(SelectWatcherComponent))]
    public static class SelectWatcherComponentSystem
    {
        public class SelectWatcherComponentAwakeSystem:AwakeSystem<SelectWatcherComponent>
        {
            public override void Awake(SelectWatcherComponent self)
            {
                SelectWatcherComponent.Instance = self;
                self.Init();
            }
        }
        
        public class SelectWatcherComponentLoadSystem:LoadSystem<SelectWatcherComponent>
        {
            public override void Load(SelectWatcherComponent self)
            {
                self.Init();
            }
        }
        
        private static void Init(this SelectWatcherComponent self)
        {
            self.typeSystems = new TypeSystems();
            foreach (Type type in EventSystem.Instance.GetTypes(typeof(SelectSystemAttribute)))
            {
                object obj = Activator.CreateInstance(type);
                if (obj is ISystemType iSystemType)
                {
                    OneTypeSystems oneTypeSystems = self.typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Add(iSystemType.SystemType(), obj);
                }
            }
        }

        public static async ETTask Show(this SelectWatcherComponent self,Entity component)
		{
			List<object> iShowSelectSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IShowSelectSystem));
			if (iShowSelectSystems == null)
			{
				return;
			}

			for (int i = 0; i < iShowSelectSystems.Count; i++)
			{
				IShowSelectSystem aShowSelectSystem = (IShowSelectSystem)iShowSelectSystems[i];
				if (aShowSelectSystem == null)
				{
					continue;
				}

				try
				{
					await aShowSelectSystem.Show(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
		
		public static async ETTask Show<T>(this SelectWatcherComponent self,Entity component,T t)
		{
			List<object> iShowSelectSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IShowSelectSystem<T>));
			if (iShowSelectSystems == null)
			{
				return;
			}

			for (int i = 0; i < iShowSelectSystems.Count; i++)
			{
				IShowSelectSystem<T> aShowSelectSystem = (IShowSelectSystem<T>)iShowSelectSystems[i];
				if (aShowSelectSystem == null)
				{
					continue;
				}

				try
				{
					await aShowSelectSystem.Show(component,t);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
		
		public static async ETTask Show<T,V>(this SelectWatcherComponent self,Entity component,T t,V v)
		{
			List<object> iShowSelectSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IShowSelectSystem<T,V>));
			if (iShowSelectSystems == null)
			{
				return;
			}

			for (int i = 0; i < iShowSelectSystems.Count; i++)
			{
				IShowSelectSystem<T,V> aShowSelectSystem = (IShowSelectSystem<T,V>)iShowSelectSystems[i];
				if (aShowSelectSystem == null)
				{
					continue;
				}

				try
				{
					await aShowSelectSystem.Show(component,t,v);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
		
		public static void Hide(this SelectWatcherComponent self,Entity component)
		{
			List<object> iShowSelectSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IHideSelectSystem));
			if (iShowSelectSystems == null)
			{
				return;
			}

			for (int i = 0; i < iShowSelectSystems.Count; i++)
			{
				IHideSelectSystem aShowSelectSystem = (IHideSelectSystem)iShowSelectSystems[i];
				if (aShowSelectSystem == null)
				{
					continue;
				}

				try
				{
					aShowSelectSystem.Hide(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
    }
}