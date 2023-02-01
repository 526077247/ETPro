using System;

namespace ET
{
    [ActorMessageHandler]
    public abstract class AMActorHandler<E, Message>: IMActorHandler where E : Entity where Message : class, IActorMessage
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

            await this.Run(e, msg);
        }

        public Type GetRequestType()
        {
            if (TypeInfo<IActorLocationMessage>.Type.IsAssignableFrom(TypeInfo<Message>.Type))
            {
                Log.Error($"message is IActorLocationMessage but handler is AMActorHandler: {TypeInfo<Message>.Type}");
            }

            return TypeInfo<Message>.Type;
        }

        public Type GetResponseType()
        {
            return null;
        }
    }
}