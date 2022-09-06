using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class ScriptMenuAddOptions
    {

        [MenuItem("CONTEXT/Component/Go To Script's Directory")]
        private static void GoToBehaviourDirectory(MenuCommand menuCommand)
        {
            MonoBehaviour targetComponent = (MonoBehaviour)menuCommand.context;

            if (targetComponent)
            {
                MonoScript script = MonoScript.FromMonoBehaviour(targetComponent);
                if (script) EditorGUIUtility.PingObject(script);
            }
        }

        [MenuItem("CONTEXT/Component/Open Script's File")]
        private static void OpenBehaviourFile(MenuCommand menuCommand)
        {
            MonoBehaviour targetComponent = (MonoBehaviour)menuCommand.context;

            if (targetComponent)
            {
                MonoScript script = MonoScript.FromMonoBehaviour(targetComponent);
                if (script) AssetDatabase.OpenAsset(script);
            }
        }


        [MenuItem("CONTEXT/Component/Go To Script's Directory", true)]
        private static bool IsMonoBeh(MenuCommand menuCommand)
        {
            object comp = (Component)menuCommand.context;

            try
            {
                if (comp == null) return false;
                else
                    return (MonoBehaviour)comp;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        [MenuItem("CONTEXT/Component/Open Script's File", true)]
        private static bool IsMonoBehOpen(MenuCommand menuCommand)
        {
            return IsMonoBeh(menuCommand);
        }
    }
}
