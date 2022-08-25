using System.Collections.Generic;
using ProtoBuf;
namespace ET
{
    [ProtoContract]
    public class AssetsRoot
    {
        [ProtoMember(1)]
        public List<AssetsScene> Scenes;
    }
}