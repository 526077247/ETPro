using System;
using System.Collections.Generic;
using ProtoBuf;
namespace ET
{
    [ProtoContract]
    internal class AssetsScene
    {
        [ProtoContract]
        internal class IntList
        {
            [ProtoMember(1)]
            public List<int> Value;
        }
        [ProtoMember(1)]
        public string Name;
        [ProtoMember(2)]
        public int CellLen;
        [ProtoMember(3)]
        public List<AssetsObject> Objects;

        [ProtoMember(4)]
        public List<long> CellIds;
        [ProtoMember(5)]
        public List<IntList> MapObjects;

        [ProtoIgnore]
        public Dictionary<long, List<int>> CellMapObjects;

    }
}