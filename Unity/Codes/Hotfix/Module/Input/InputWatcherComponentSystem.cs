using System;
using System.Collections.Generic;

namespace ET
{
    using OneTypeSystems = UnOrderMultiMap<Type, object>;
    [FriendClass(typeof(InputWatcherComponent))]
    public static class InputWatcherComponentSystem
    {
        [ObjectSystem]
        public class InputWatcherComponentAwakeSystem : AwakeSystem<InputWatcherComponent>
        {
            public override void Awake(InputWatcherComponent self)
            {
                InputWatcherComponent.Instance = self;
                self.InputEntitys = new List<Entity>();
                self.Init();
            }
        }

	
        public class InputWatcherComponentLoadSystem : LoadSystem<InputWatcherComponent>
        {
            public override void Load(InputWatcherComponent self)
            {
                self.Init();
            }
        }

        private static void Init(this InputWatcherComponent self)
        {
            self.typeSystems = new TypeSystems();
            self.typeMapAttr = new UnOrderMultiMap<object, InputSystemAttribute>();
            self.sortList = new LinkedList<Tuple<object,Entity,int,int[],int[]>>();
            List<Type> types = Game.EventSystem.GetTypes(typeof(InputSystemAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(InputSystemAttribute), false);
                if(attrs.Length<=0) return;
                var obj = Activator.CreateInstance(type);
                if (obj is ISystemType iSystemType)
                {
                    bool has = false;
                    for (int i = 0; i < attrs.Length; i++)
                    {
                        var attr = attrs[i] as InputSystemAttribute;
                        if (!Define.Debug && attr.Priority <= -10000)
                        {
                            continue;
                        }
                        self.typeMapAttr.Add(obj, attr);
                        for (int j = 0; j < attr.KeyCode.Length; j++)
                        {
                            InputComponent.Instance.AddListenter(attr.KeyCode[j]);
                        }

                        has = true;
                    }

                    if (has)
                    {
                        OneTypeSystems oneTypeSystems = self.typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                        oneTypeSystems.Add(iSystemType.SystemType(), obj);
                    }
                }
                
            }
        }

        public static void RunCheck(this InputWatcherComponent self)
        {
            //优先级高的在后面
            for (var node = self.sortList.Last; node!=null&& InputComponent.Instance.HasKey; node=node.Previous)
            {
                var component = node.Value.Item2;
                var system = node.Value.Item1;
                var codes = node.Value.Item4;
                var types = node.Value.Item5;
                try
                {
                    if(system is IInputSystem inputSystem)
                    {
                        for (int i = 0; i < codes.Length; i++)
                        {
                            var type = types[i];
                            var code = codes[i];
                            if (type == InputType.Key)
                            {
                                if (InputComponent.Instance.GetKey(code))
                                {
                                    bool stop = false;
                                    inputSystem.Run(component, code, type, ref stop);
                                    if (stop)
                                    {
                                        InputComponent.Instance.StopKey(code);
                                    }
                                }
                            }

                            if (type == InputType.KeyDown)
                            {
                                if (InputComponent.Instance.GetKeyDown(code))
                                {
                                    bool stop = false;
                                    inputSystem.Run(component, code, type, ref stop);
                                    if (stop)
                                    {
                                        InputComponent.Instance.StopKeyDown(code);
                                    }
                                }
                            }

                            if (type == InputType.KeyUp)
                            {
                                if (InputComponent.Instance.GetKeyUp(code))
                                {
                                    bool stop = false;
                                    inputSystem.Run(component, code, type, ref stop);
                                    if (stop)
                                    {
                                        InputComponent.Instance.StopKeyUp(code);
                                    }
                                }
                            }
                        }
                    }
                    else if (system is IInputGroupSystem mulInputSystem)
                    {
                        using (ListComponent<int> _code = ListComponent<int>.Create())
                        {
                            using (ListComponent<int> _type = ListComponent<int>.Create())
                            {
                                for (int i = 0; i < codes.Length; i++)
                                {
                                    var type = types[i];
                                    var code = codes[i];
                                   
                                    if (type == InputType.Key)
                                    {
                                        if (InputComponent.Instance.GetKey(code))
                                        {
                                            _code.Add(code);
                                            _type.Add(type);
                                        }
                                    }

                                    if (type == InputType.KeyDown)
                                    {
                                        if (InputComponent.Instance.GetKeyDown(code))
                                        {
                                            _code.Add(code);
                                            _type.Add(type);
                                        }
                                    }

                                    if (type == InputType.KeyUp)
                                    {
                                        if (InputComponent.Instance.GetKeyUp(code))
                                        {
                                            _code.Add(code);
                                            _type.Add(type);
                                        }
                                    }
                                }
                                bool stop = false;
                                mulInputSystem.Run(component, _code, _type, ref stop);
                                if (stop)
                                {
                                    for (int i = 0; i < _code.Count; i++)
                                    {
                                        var type = _type[i];
                                        var code = _code[i];
                                        if (type == InputType.Key)
                                        {
                                            InputComponent.Instance.StopKey(code);
                                        }

                                        if (type == InputType.KeyDown)
                                        {
                                            InputComponent.Instance.StopKeyDown(code);
                                        }

                                        if (type == InputType.KeyUp)
                                        {
                                            InputComponent.Instance.StopKeyUp(code);
                                        }
                                    }
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        Log.Error("未处理此类型"+nameof(system));
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        
        public static void RegisterInputEntity(this InputWatcherComponent self,Entity entity)
        {
            if (!self.InputEntitys.Contains(entity))
            {
                self.InputEntitys.Add(entity);
                List<object> iInputSystems = self.typeSystems.GetSystems(entity.GetType(), typeof(IInputSystem));
                if (iInputSystems != null)
                {
                    for (int i = 0; i < iInputSystems.Count; i++)
                    {
                        self.AddInputSystem(entity,iInputSystems[i]);
                    }
                }
                iInputSystems = self.typeSystems.GetSystems(entity.GetType(), typeof(IInputGroupSystem));
                if (iInputSystems != null)
                {
                    for (int i = 0; i < iInputSystems.Count; i++)
                    {
                        self.AddInputSystem(entity,iInputSystems[i]);
                    }
                }
            }

        }

        public static void AddInputSystem(this InputWatcherComponent self,Entity entity,object inputSystem)
        {
            if (!(inputSystem is IInputSystem || inputSystem is IInputGroupSystem))
            {
                return;
            }

            if (self.typeMapAttr.TryGetValue(inputSystem, out var attrs))
            {
                for (int j = 0; j < attrs.Count; j++)
                {
                    var attr = attrs[j];
                    var code = attr.KeyCode;
                    var type = attr.InputType;
                    var priority = attr.Priority;
                            
                    bool isAdd = false;
                    for (var node = self.sortList.Last; node!=null; node=node.Previous)
                    {
                        if (node.Value.Item3 <= priority)
                        {
                            self.sortList.AddAfter(node,new Tuple<object, Entity, int,int[],int[]>(inputSystem, entity, priority,code,type));
                            isAdd = true;
                            break;
                        }
                    }
                    if (!isAdd)
                    {
                        self.sortList.AddFirst(new Tuple<object, Entity, int,int[],int[]>(inputSystem, entity, priority,code,type));
                    }
                            
                }
            }
            else
            {
                Log.Error("RegisterInputEntity attr miss! type="+inputSystem.GetType().Name);
            }
        }
        
        public static void RemoveInputEntity(this InputWatcherComponent self,Entity entity)
        {
            self.InputEntitys.Remove(entity);
            List<object> iInputSystems = self.typeSystems.GetSystems(entity.GetType(), typeof(IInputSystem));
            if (iInputSystems != null)
            {
                for (int i = 0; i < iInputSystems.Count; i++)
                {
                    self.RemoveInputSystem(iInputSystems[i]);

                }
            }
            iInputSystems = self.typeSystems.GetSystems(entity.GetType(), typeof(IInputGroupSystem));
            if (iInputSystems != null)
            {
                for (int i = 0; i < iInputSystems.Count; i++)
                {
                    self.RemoveInputSystem(iInputSystems[i]);

                }
            }
        }

        public static void RemoveInputSystem(this InputWatcherComponent self, object inputSystem)
        {
            if (!(inputSystem is IInputSystem || inputSystem is IInputGroupSystem))
            {
                return;
            }

            if (self.typeMapAttr.TryGetValue(inputSystem, out var attrs))
            {
                for (int j = 0; j < attrs.Count; j++)
                {
                    var attr = attrs[j];
                    var code = attr.KeyCode;
                    var type = attr.InputType;
                    for (var node = self.sortList.Last; node!=null;node = node.Previous)
                    {
                        if (node.Value.Item1 == inputSystem&&node.Value.Item4 == code&&node.Value.Item5 == type)
                        {
                            self.sortList.Remove(node);
                            break;
                        }
                    }
                }
            }
            else
            {
                Log.Error("RemoveInputEntity attr miss! type="+inputSystem.GetType().Name);
            }
        }
    }
}