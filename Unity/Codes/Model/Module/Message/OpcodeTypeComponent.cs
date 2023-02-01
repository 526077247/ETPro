using System;
using System.Collections.Generic;

namespace ET
{
    [FriendClass(typeof(OpcodeTypeComponent))]
    public static class OpcodeTypeComponentSystem
    {
        [ObjectSystem]
        public class OpcodeTypeComponentAwakeSystem: AwakeSystem<OpcodeTypeComponent>
        {
            public override void Awake(OpcodeTypeComponent self)
            {
                OpcodeTypeComponent.Instance = self;
                
                self.opcodeTypes.Clear();
                self.typeOpcodes.Clear();
                self.requestResponse.Clear();

                List<Type> types = Game.EventSystem.GetTypes(TypeInfo<MessageAttribute>.Type);
                foreach (Type type in types)
                {
                    object[] attrs = type.GetCustomAttributes(TypeInfo<MessageAttribute>.Type, false);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }

                    MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
                    if (messageAttribute == null)
                    {
                        continue;
                    }
                

                    self.opcodeTypes.Add(messageAttribute.Opcode, type);
                    self.typeOpcodes.Add(type, messageAttribute.Opcode);

                    if (OpcodeHelper.IsOuterMessage(messageAttribute.Opcode) && TypeInfo<IActorMessage>.Type.IsAssignableFrom(type))
                    {
                        self.outrActorMessage.Add(messageAttribute.Opcode);
                    }
                
                    // 检查request response
                    if (TypeInfo<IRequest>.Type.IsAssignableFrom(type))
                    {
                        if (TypeInfo<IActorLocationMessage>.Type.IsAssignableFrom(type))
                        {
                            self.requestResponse.Add(type, TypeInfo<ActorResponse>.Type);
                            continue;
                        }
                    
                        attrs = type.GetCustomAttributes(TypeInfo<ResponseTypeAttribute>.Type, false);
                        if (attrs.Length == 0)
                        {
                            Log.Error($"not found responseType: {type}");
                            continue;
                        }

                        ResponseTypeAttribute responseTypeAttribute = attrs[0] as ResponseTypeAttribute;
                        self.requestResponse.Add(type, Game.EventSystem.GetType($"ET.{responseTypeAttribute.Type}"));
                    }
                }
            }
        }

        [ObjectSystem]
        public class OpcodeTypeComponentDestroySystem: DestroySystem<OpcodeTypeComponent>
        {
            public override void Destroy(OpcodeTypeComponent self)
            {
                OpcodeTypeComponent.Instance = null;
            }
        }

        public static bool IsOutrActorMessage(this OpcodeTypeComponent self, ushort opcode)
        {
            return self.outrActorMessage.Contains(opcode);
        }

        public static ushort GetOpcode(this OpcodeTypeComponent self, Type type)
        {
            return self.typeOpcodes[type];
        }

        public static Type GetType(this OpcodeTypeComponent self, ushort opcode)
        {
            return self.opcodeTypes[opcode];
        }

        public static Type GetResponseType(this OpcodeTypeComponent self, Type request)
        {
            if (!self.requestResponse.TryGetValue(request, out Type response))
            {
                throw new Exception($"not found response type, request type: {request.GetType().Name}");
            }
            return response;
        }
    }
    
    [ComponentOf(typeof(Scene))]
    public class OpcodeTypeComponent: Entity, IAwake, IDestroy
    {
        public static OpcodeTypeComponent Instance;
        
        public HashSet<ushort> outrActorMessage = new HashSet<ushort>();
        
        public readonly Dictionary<ushort, Type> opcodeTypes = new Dictionary<ushort, Type>();
        public readonly Dictionary<Type, ushort> typeOpcodes = new Dictionary<Type, ushort>();
        
        public readonly Dictionary<Type, Type> requestResponse = new Dictionary<Type, Type>();
    }
}