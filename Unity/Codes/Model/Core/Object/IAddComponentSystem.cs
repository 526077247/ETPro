using System;

namespace ET
{
	public interface IAddComponent
	{
	}
	
	public interface IAddComponentSystem: ISystemType
	{
		void Run(object o, Entity component);
	}

	[ObjectSystem]
	public abstract class AddComponentSystem<T> : IAddComponentSystem where T: IAddComponent
	{
		public void Run(object o, Entity component)
		{
			this.AddComponent((T)o, component);
		}
		
		public Type SystemType()
		{
			return TypeInfo<IAddComponentSystem>.Type;
		}

		public Type Type()
		{
			return TypeInfo<T>.Type;
		}

		public abstract void AddComponent(T self, Entity component);
	}
}
