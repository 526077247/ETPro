using System;

namespace ET
{
    [FriendClass(typeof(AreaConfigComponent))]
    public static class AreaConfigComponentSystem
    {
        public class AwakeSystem: AwakeSystem<AreaConfigComponent>
        {
            public override void Awake(AreaConfigComponent self)
            {
                AreaConfigComponent.Instance = self;
            }
        }

        public static AreaConfigCategory Get(this AreaConfigComponent self, string name)
        {
            AreaConfigCategory ptr;
            if (self.AreaConfigCategorys.TryGetValue(name, out ptr))
            {
                return ptr;
            }

            byte[] buffer = self.Loader.GetOneConfigBytes(name);
            if (buffer==null||buffer.Length == 0)
            {
                throw new Exception($"no AreaConfig data: {name}");
            }

            ptr = ProtobufHelper.FromBytes(typeof(AreaConfigCategory),buffer, 0, buffer.Length) as AreaConfigCategory;
            self.AreaConfigCategorys[name] = ptr;

            return ptr;
        }
    }
}