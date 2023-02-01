using System;

namespace ET
{
    [ActorMessageHandler]
    public abstract class AMActorLocationHandler<E, Message>: IMActorHandler where E : Entity where Message : class, IActorLocationMessage
    {
        protected abstract ETTask Run(E entity, Message message);

        public async ETTask Handle(Entity entity, object actorMessage, Action<IActorResponse> reply)
        {
            Message msg = actorMessage as Message;
            if (msg == null)
            {
                Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {TypeInfo<Message>.TypeName}");
                return;
            }

            E e = entity as E;
            if (e == null)
            {
                Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {TypeInfo<E>.TypeName} --{TypeInfo<Message>.TypeName}");
                return;
            }

            IActorResponse response = (IActorResponse) Activator.CreateInstance(GetResponseType());
            response.RpcId = msg.RpcId;
            reply.Invoke(response);

            await this.Run(e, msg);
        }

        public Type GetRequestType()
        {
            return TypeInfo<Message>.Type;
        }

        public Type GetResponseType()
        {
            return TypeInfo<ActorResponse>.Type;
        }
    }
}