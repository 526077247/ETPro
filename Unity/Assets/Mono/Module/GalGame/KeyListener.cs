using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class KeyListener : MonoBehaviour
    {
        public event Action<KeyCode> OnKey;
        public event Action<KeyCode> OnKeyDown;
        public event Action<KeyCode> OnKeyUp;
        HashSet<KeyCode> codes = new HashSet<KeyCode>();
        // Update is called once per frame
        void Update()
        {
            if (Input.anyKey)
            {
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKey(keyCode))
                    {
                        OnKey?.Invoke(keyCode);
                        break;
                    }
                }
            }
            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKey(keyCode))
                    {
                        codes.Add(keyCode);
                        OnKeyDown?.Invoke(keyCode);
                        break;
                    }
                }
            }
            foreach (KeyCode keyCode in codes)
            {
                if (Input.GetKeyUp(keyCode))
                {
                    OnKeyUp?.Invoke(keyCode);
                    break;
                }
            }

        }
    }
}
