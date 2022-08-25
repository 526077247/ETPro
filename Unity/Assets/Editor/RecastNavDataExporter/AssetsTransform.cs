using UnityEngine;
using ProtoBuf;
namespace ET
{
    [ProtoContract]
    internal class AssetsTransform
    { 
        [ProtoIgnore]
        public Vector3 Position
        {
            set
            {
                this.position = new[] { value.x, value.y, value.z };
            }
            get
            {
                if (this.position == null || this.position.Length != 3)
                {
                    return Vector3.zero;
                }

                return new Vector3(this.position[0], this.position[1], this.position[2]);
            }
        }
        [ProtoIgnore]
        public Quaternion Rotation
        {
            set
            {
                this.rotation = new[] { value.x, value.y, value.z,value.w };
            }
            get
            {
                if (this.rotation == null || this.rotation.Length != 4)
                {
                    return Quaternion.identity;
                }

                return new Quaternion(this.rotation[0], this.rotation[1], this.rotation[2],this.rotation[3]);
            }
        }
        [ProtoIgnore]
        public Vector3 Scale
        {
            set
            {
                this.scale = new[] { value.x, value.y, value.z };
            }
            get
            {
                if (this.scale == null || this.scale.Length != 3)
                {
                    return Vector3.zero;
                }

                return new Vector3(this.scale[0], this.scale[1], this.scale[2]);
            }
        }

        [ProtoMember(1)]
        public float[] position;
        [ProtoMember(2)]
        public float[] rotation;
        [ProtoMember(3)]
        public float[] scale;
    }
}