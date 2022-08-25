using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class InputComponent:Entity,IAwake,IUpdate
    {
        public static InputComponent Instance;
        public List<int> KeysForListen;
        public Dictionary<int,bool> IsKeyDown;
        public Dictionary<int,bool> IsKeyUp;
        public Dictionary<int,bool> IsKey;
        public bool HasKey => IsKeyDown.Count > 0 || IsKeyUp.Count > 0 || IsKey.Count > 0;
    }
}