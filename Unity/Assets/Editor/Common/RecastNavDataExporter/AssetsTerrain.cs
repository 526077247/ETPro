using ProtoBuf;
namespace ET
{
    [ProtoContract]
    internal class AssetsTerrain
    {
        [ProtoMember(1)]
        public string TerrainPath;
        [ProtoMember(2)]
        public string MaterialPath;
    }
}