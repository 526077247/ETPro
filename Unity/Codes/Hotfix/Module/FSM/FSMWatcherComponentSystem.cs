using System;
using System.Collections.Generic;
namespace ET
{
	using OneTypeSystems = UnOrderMultiMap<Type, object>;
    [FriendClass(typeof(FSMWatcherComponent))]
    public static class FSMWatcherComponentSystem
    {
        public class FSMWatcherComponentAwakeSystem:AwakeSystem<FSMWatcherComponent>
        {
            public override void Awake(FSMWatcherComponent self)
            {
	            FSMWatcherComponent.Instance = self;
	            self.Init();
            }
        }
        
        public class FSMWatcherComponentLoadSystem:LoadSystem<FSMWatcherComponent>
        {
            public override void Load(FSMWatcherComponent self)
            {
	            self.Init();
            }
        }
        
        public static void Init(this FSMWatcherComponent self)
        {
	        self.typeSystems = new TypeSystems();
			foreach (Type type in EventSystem.Instance.GetTypes(typeof(FSMSystemAttribute)))
			{
				object obj = Activator.CreateInstance(type);

				if (obj is ISystemType iSystemType)
				{
					OneTypeSystems oneTypeSystems = self.typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
					oneTypeSystems.Add(iSystemType.SystemType(), obj);
				}
			}
		}
		#region FSMOnEnter
		public static async ETTask FSMOnEnter(this FSMWatcherComponent self,Entity component)
		{
			List<object> iFSMOnEnterSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IFSMOnEnterSystem));
			if (iFSMOnEnterSystems == null)
			{
				return;
			}

			for (int i = 0; i < iFSMOnEnterSystems.Count; i++)
			{
				IFSMOnEnterSystem aFSMOnEnterSystem = (IFSMOnEnterSystem)iFSMOnEnterSystems[i];
				if (aFSMOnEnterSystem == null)
				{
					continue;
				}

				try
				{
					await aFSMOnEnterSystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static async ETTask FSMOnEnter<P1>(this FSMWatcherComponent self,Entity component, P1 p1)
		{
			List<object> iFSMOnEnterSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IFSMOnEnterSystem<P1>));
			if (iFSMOnEnterSystems == null)
			{
				return;
			}

			for (int i = 0; i < iFSMOnEnterSystems.Count; i++)
			{
				IFSMOnEnterSystem<P1> aFSMOnEnterSystem = (IFSMOnEnterSystem<P1>)iFSMOnEnterSystems[i];
				if (aFSMOnEnterSystem == null)
				{
					continue;
				}

				try
				{
					await aFSMOnEnterSystem.Run(component, p1);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static async ETTask FSMOnEnter<P1, P2>(this FSMWatcherComponent self,Entity component, P1 p1, P2 p2)
		{
			List<object> iFSMOnEnterSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IFSMOnEnterSystem<P1, P2>));
			if (iFSMOnEnterSystems == null)
			{
				return;
			}

			for (int i = 0; i < iFSMOnEnterSystems.Count; i++)
			{
				IFSMOnEnterSystem<P1, P2> aFSMOnEnterSystem = (IFSMOnEnterSystem<P1, P2>)iFSMOnEnterSystems[i];
				if (aFSMOnEnterSystem == null)
				{
					continue;
				}

				try
				{
					await aFSMOnEnterSystem.Run(component, p1, p2);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static async ETTask FSMOnEnter<P1, P2, P3>(this FSMWatcherComponent self,Entity component, P1 p1, P2 p2, P3 p3)
		{
			List<object> iFSMOnEnterSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IFSMOnEnterSystem<P1, P2, P3>));
			if (iFSMOnEnterSystems == null)
			{
				return;
			}

			for (int i = 0; i < iFSMOnEnterSystems.Count; i++)
			{
				IFSMOnEnterSystem<P1, P2, P3> aFSMOnEnterSystem = (IFSMOnEnterSystem<P1, P2, P3>)iFSMOnEnterSystems[i];
				if (aFSMOnEnterSystem == null)
				{
					continue;
				}

				try
				{
					await aFSMOnEnterSystem.Run(component, p1, p2, p3);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public static async ETTask FSMOnEnter<P1, P2, P3, P4>(this FSMWatcherComponent self,Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
		{
			List<object> iFSMOnEnterSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IFSMOnEnterSystem<P1, P2, P3, P4>));
			if (iFSMOnEnterSystems == null)
			{
				return;
			}

			for (int i = 0; i < iFSMOnEnterSystems.Count; i++)
			{
				IFSMOnEnterSystem<P1, P2, P3, P4> aFSMOnEnterSystem = (IFSMOnEnterSystem<P1, P2, P3, P4>)iFSMOnEnterSystems[i];
				if (aFSMOnEnterSystem == null)
				{
					continue;
				}

				try
				{
					await aFSMOnEnterSystem.Run(component, p1, p2, p3, p4);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		#endregion

		#region FSMOnExit
		public static async ETTask FSMOnExit(this FSMWatcherComponent self,Entity component)
		{

			List<object> iFSMOnExitSystems = self.typeSystems.GetSystems(component.GetType(), typeof(IFSMOnExitSystem));
			if (iFSMOnExitSystems != null)
			{
				for (int i = 0; i < iFSMOnExitSystems.Count; i++)
				{
					IFSMOnExitSystem aFSMOnExitSystem = (IFSMOnExitSystem)iFSMOnExitSystems[i];
					if (aFSMOnExitSystem == null)
					{
						continue;
					}

					try
					{
						await aFSMOnExitSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
		}

		#endregion


    }
}