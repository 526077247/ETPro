using System;
using System.Collections.Generic;
namespace ET
{
	using OneTypeSystems = UnOrderMultiMap<Type, object>;

	[ComponentOf(typeof(Scene))]
	public sealed class SelectWatcherComponent:Entity,IAwake,ILoad
	{
		public static SelectWatcherComponent Instance;
		public TypeSystems typeSystems;
	}
}