using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
	using OneTypeSystems = UnOrderMultiMap<Type, object>;
	[ComponentOf(typeof(Scene))]
	public sealed class FSMWatcherComponent:Entity,IAwake,ILoad
	{
		public static FSMWatcherComponent Instance;

		public TypeSystems typeSystems;
		
	}
}
