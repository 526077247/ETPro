using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{

	//--[[
	//-- GameObject缓存池
	//-- 注意：
	//-- 1、所有需要预设都从这里加载，不要直接到ResourcesManager去加载，由这里统一做缓存管理
	//-- 2、缓存分为两部分：从资源层加载的原始GameObject(Asset)，从GameObject实例化出来的多个Inst

	//	原则: 有借有还，再借不难，借出去的东西回收时，请不要污染(pc上会进行检测，发现则报错)
	//	何为污染？
	//	1、不要往里面添加什么节点，借出来的是啥样子，返回来的就是啥样子


	//	GameObject内存管理，采用lru cache来管理prefab
	//	为了对prefab和其产生的go的内存进行管理，所以严格管理go生命周期 
	//	1、创建 -> GetGameObjectAsync
	//	2、回收 -> 绝大部分的时候应该采用回收(回收go不能被污染)，对象的销毁对象池会自动管理 RecycleGameObject
	//	3、销毁 -> 如果的确需要销毁，或一些用不到的数据想要销毁，也必须从这GameObjectPool中进行销毁，
	//			  严禁直接调用GameObject.Destroy方法来进行销毁，而应该采用GameObjectPool.DestroyGameObject方法

	//	不管是销毁还是回收，都不要污染go，保证干净
	//--]]

	[ComponentOf(typeof(Scene))]
	public class GameObjectPoolComponent : Entity,IAwake,IDestroy
	{
		public Transform __cacheTransRoot;
		public static GameObjectPoolComponent Instance { get; set; }
		public LruCache<string, GameObject> __goPool;
		public Dictionary<string, int> __goInstCountCache;//go: inst_count 用于记录go产生了多少个实例

		public Dictionary<string, int> __goChildsCountPool;//path: child_count 用于在editor模式下检测回收的go是否被污染 path:num

		public Dictionary<string, List<GameObject>> __instCache; //path: inst_array
		public Dictionary<GameObject, string> __instPathCache;// inst : prefab_path 用于销毁和回收时反向找到inst对应的prefab TODO:这里有优化空间path太占内存
		public Dictionary<string, bool> __persistentPathCache;//需要持久化的资源
		public Dictionary<string, Dictionary<string, int>> __detailGoChildsCount;//记录go子控件具体数量信息
		
	}
}
