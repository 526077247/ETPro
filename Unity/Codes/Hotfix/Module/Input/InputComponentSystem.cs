using System.Collections.Generic;
using UnityEngine;
namespace ET
{
    [FriendClass(typeof(InputComponent))]
    public static class InputComponentSystem
    {
        public class InputComponentAwakeSystem:AwakeSystem<InputComponent>
        {
            public override void Awake(InputComponent self)
            {
                InputComponent.Instance = self;
                self.KeysForListen = new List<int>();
                self.IsKeyDown = new Dictionary<int,bool>();
                self.IsKeyUp = new Dictionary<int,bool>();
                self.IsKey = new Dictionary<int,bool>();
            }
        }
        
        public class InputComponentUpdateSystem:UpdateSystem<InputComponent>
        {
            public override void Update(InputComponent self)
            {
                self.IsKeyDown.Clear();
                self.IsKeyUp.Clear();
                self.IsKey.Clear();
                for (int i= 0; i< self.KeysForListen.Count; ++i)
                {
                    int key = self.KeysForListen[i];
                    key = self.ReplaceKey(key);
                    if (key >= 0)
                    {
                        if (Input.GetKeyDown((KeyCode)key))
                            self.IsKeyDown[key] = true;
                        if (Input.GetKeyUp((KeyCode)key))
                            self.IsKeyUp[key] = true;
                        if (Input.GetKey((KeyCode)key))
                            self.IsKey[key] = true;
                    }
                }
                InputWatcherComponent.Instance.RunCheck();
            }

        }
        
        public static void AddListenter(this InputComponent self, int key)
        {
            if (!self.KeysForListen.Contains(key))
            {
                self.KeysForListen.Add(key);
            }
        }

        public static bool GetKeyDown(this InputComponent self, int key)
        {
            key = self.ReplaceKey(key);
            if (self.IsKeyDown.TryGetValue(key, out var res))
            {
                return res;
            }
            return false;
        }
        public static void StopKeyDown(this InputComponent self, int key)
        {
            key = self.ReplaceKey(key);
            self.IsKeyDown.Remove(key);
        }
        public static bool GetKeyUp(this InputComponent self, int key)
        {
            key = self.ReplaceKey(key);
            if (self.IsKeyUp.TryGetValue(key, out var res))
            {
                return res;
            }
            return false;
        }
        public static void StopKeyUp(this InputComponent self, int key)
        {
            key = self.ReplaceKey(key);
            self.IsKeyUp.Remove(key);
        }
        public static bool GetKey(this InputComponent self, int key)
        {
            key = self.ReplaceKey(key);
            if (self.IsKey.TryGetValue(key, out var res))
            {
                return res;
            }
            return false;
        }
        
        public static void StopKey(this InputComponent self, int key)
        {
            key = self.ReplaceKey(key);
            self.IsKey.Remove(key);
        }

        public static int ReplaceKey(this InputComponent self,int key)
        {
            if (key < 0&&KeyCodeComponent.Instance!=null)
            {
                if(KeyCodeComponent.Instance.KeyMap.TryGetValue(key,out var res))
                {
                    return res;
                }
            }

            return key;
        }
    }
}