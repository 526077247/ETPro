using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ET
{
	[ObjectSystem]
	public class ConfigAwakeSystem : AwakeSystem<ConfigComponent>
	{
		public override void Awake(ConfigComponent self)
		{
			ConfigComponent.Instance = self;
		}
	}
	[ObjectSystem]
	public class ConfigDestroySystem : DestroySystem<ConfigComponent>
	{
		public override void Destroy(ConfigComponent self)
		{
			ConfigComponent.Instance = null;
		}
	}
	[FriendClass(typeof(ConfigComponent))]
	public static class ConfigComponentSystem
	{
		public static async ETTask<T> LoadOneConfig<T>(this ConfigComponent self,string name = "", bool cache = false) where T: ProtoObject
		{
			Type configType = TypeInfo<T>.Type;
			if (string.IsNullOrEmpty(name))
				name = configType.FullName;
			byte[] oneConfigBytes = await self.ConfigLoader.GetOneConfigBytes(name);

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			if(cache)
				self.AllConfig[configType] = category;

			return category as T;
		}
		public static async ETTask LoadOneConfig(this ConfigComponent self, Type configType)
		{
			byte[] oneConfigBytes = await self.ConfigLoader.GetOneConfigBytes(configType.Name);

			object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

			self.AllConfig[configType] = category;
		}

		public static async ETTask Load(this ConfigComponent self)
		{
			self.AllConfig.Clear();
			List<Type> types = Game.EventSystem.GetTypes(TypeInfo<ConfigAttribute>.Type);

			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			await self.ConfigLoader.GetAllConfigBytes(configBytes);

			foreach (Type type in types)
			{
				self.LoadOneInThread(type, configBytes);
			}
		}

		public static async ETTask LoadAsync(this ConfigComponent self)
		{
			self.AllConfig.Clear();
			List<Type> types = Game.EventSystem.GetTypes(TypeInfo<ConfigAttribute>.Type);

			Dictionary<string, byte[]> configBytes = new Dictionary<string, byte[]>();
			await self.ConfigLoader.GetAllConfigBytes(configBytes);

			using (ListComponent<Task> listTasks = ListComponent<Task>.Create())
			{
				foreach (Type type in types)
				{
					Task task = Task.Run(() => self.LoadOneInThread(type, configBytes));
					listTasks.Add(task);
				}

				await Task.WhenAll(listTasks.ToArray());
			}
		}

		private static void LoadOneInThread(this ConfigComponent self, Type configType, Dictionary<string, byte[]> configBytes)
		{

			if (configBytes.TryGetValue(configType.Name, out var oneConfigBytes))
			{
				object category = ProtobufHelper.FromBytes(configType, oneConfigBytes, 0, oneConfigBytes.Length);

				lock (self)
				{
					self.AllConfig[configType] = category;
				}
			}
			else
			{
				Log.Error(configType.Name+" 未找到配置");
			}
		}

		public static void ReleaseConfig<T>(this ConfigComponent self) where T :ProtoObject, IMerge
		{
			var configType = TypeInfo<T>.Type;
			self.AllConfig.Remove(configType);
		}
	}
}