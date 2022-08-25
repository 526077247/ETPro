using UnityEngine;
namespace ET
{
    public class UITransform:Entity,IAwake,IOnCreate,IOnCreate<Transform>,IOnEnable
    {

        public Transform transform;

        public Transform ParentTransform;
        
    }
}