using System;

namespace ET
{
	public interface IUpdate
	{
	}
	
	public interface IUpdateSystem: ISystemType
	{
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class UpdateSystem<T> : IUpdateSystem where T: IUpdate
	{
		public void Run(object o)
		{
			this.Update((T)o);
		}

		public Type Type()
		{
			return TypeInfo<T>.Type;
		}
		
		public Type SystemType()
		{
			return TypeInfo<IUpdateSystem>.Type;
		}

		public abstract void Update(T self);
	}
}
