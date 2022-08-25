using System.Collections.Generic;
using ProtoBuf;
namespace ET
{
    [ProtoContract]
    internal class AssetsRoot
    {
        [ProtoMember(1)]
        public List<AssetsScene> Scenes;
    }
}