using System;

namespace ET
{
	public interface ILoad
	{
	}
	
	public interface ILoadSystem: ISystemType
	{
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class LoadSystem<T> : ILoadSystem where T: ILoad
	{
		public void Run(object o)
		{
			this.Load((T)o);
		}
		
		public Type Type()
		{
			return TypeInfo<T>.Type;
		}
		
		public Type SystemType()
		{
			return TypeInfo<ILoadSystem>.Type;
		}

		public abstract void Load(T self);
	}
}
