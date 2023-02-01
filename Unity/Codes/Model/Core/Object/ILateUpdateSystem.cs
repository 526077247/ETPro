using System;

namespace ET
{
	public interface ILateUpdate
	{
	}
	
	public interface ILateUpdateSystem: ISystemType
	{
		void Run(object o);
	}

	[ObjectSystem]
	public abstract class LateUpdateSystem<T> : ILateUpdateSystem where T: ILateUpdate
	{
		public void Run(object o)
		{
			this.LateUpdate((T)o);
		}

		public Type Type()
		{
			return TypeInfo<T>.Type;
		}
		
		public Type SystemType()
		{
			return TypeInfo<ILateUpdateSystem>.Type;
		}

		public abstract void LateUpdate(T self);
	}
}
