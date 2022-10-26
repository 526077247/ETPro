using UnityEngine;
namespace ET
{
    [UIComponent]
    public class UITransform:Entity,IAwake,IOnCreate,IOnCreate<Transform>,IOnEnable
    {

        public Transform transform;

        public Transform ParentTransform;
        
    }
}