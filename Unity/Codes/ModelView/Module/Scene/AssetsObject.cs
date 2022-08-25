using ProtoBuf;
using UnityEngine;

namespace ET
{
    [ProtoContract]
    public class AssetsObject
    {
        [ProtoMember(1)]
        public string Name;
        [ProtoMember(2)]
        public string Type;

        [ProtoMember(3)]
        public float[] size;//网格长宽

        [ProtoIgnore]
        public Vector3 Size
        {
            set
            {
                size = new[] { value.x, value.y, value.z };
            }
            get
            {
                if (size == null || size.Length != 3)
                {
                    return Vector3.zero;
                }

                return new Vector3(size[0], size[1], size[2]);
            }
        }
        [ProtoMember(4)]
        public AssetsTransform Transform;

        #region Type-Terrain
        [ProtoMember(5)]
        public AssetsTerrain Terrain;
        
        #endregion

        #region Type-Prefab
        [ProtoMember(6)]
        public string PrefabPath;

        #endregion
        

    }
}