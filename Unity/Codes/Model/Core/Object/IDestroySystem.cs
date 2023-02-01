using System;

namespace ET
{
	public interface IDestroy
	{
		
	}
	
	public interface IDestroySystem: ISystemType
	{
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class DestroySystem<T> : IDestroySystem where T: IDestroy
	{
		public void Run(object o)
		{
			this.Destroy((T)o);
		}
		
		public Type SystemType()
		{
			return TypeInfo<IDestroySystem>.Type;
		}

		public Type Type()
		{
			return TypeInfo<T>.Type;
		}

		public abstract void Destroy(T self);
	}
}
