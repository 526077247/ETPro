using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneCheckTool : AssetModificationProcessor
{
    static public void OnWillSaveAssets(string[] names)
    {

        foreach (string name in names)
        {
            if (name == null)
            {
                continue;
            }
            if (name.Contains("Init"))
            {
                //启动场景不检查
                continue;
            }
            if (name.EndsWith(".unity"))
            {
                Scene scene = SceneManager.GetSceneByPath(name);
                Debug.Log("检查场景：" + scene.name);
                Check();
            }
        }
    }


    public static void Check()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene == null)
        {
            Debug.LogError("没有场景");
        }
        string sceneName = scene.name;

        //检查节点
        string s = checkNodes(scene.path);

        if (!string.IsNullOrEmpty(s))
        {
            EditorUtility.DisplayDialog(string.Format("场景{0}检查不通过", sceneName), s, "知道了");
            return;
        }

        StringBuilder sb = new StringBuilder();
        //检查摄像机
        //sb.Append(checkCamera(sceneName));

        //检查光照
        sb.Append(checkLight());

        string msg = sb.ToString();
        if (!string.IsNullOrEmpty(msg))
        {
            EditorUtility.DisplayDialog(string.Format("场景{0}检查不通过", sceneName), msg, "知道了");
            return;
        }
    }

    private static string[] COMMON_NODE_PATH = { }; //{ "Main Camera" };
    static string checkNodes(string scenePath)
    {
        var nodes = COMMON_NODE_PATH;

        //检查节点是否对
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = GameObject.Find(nodes[i]);
            if (node == null)
            {
                sb.AppendLine("缺少节点:" + nodes[i]);
            }
        }
        return sb.ToString();
    }

    static string checkCamera(string sceneName)
    {
        //将Camera的tag 设置为untag
        var camera = GameObject.Find("Main Camera");
        if (!camera.CompareTag("Untagged"))
        {
            camera.tag = "Untagged";
        }

        return "";
    }

    static string checkLight()
    {
        string t = string.Empty;
        //找所有light
        Object[] lights = Resources.FindObjectsOfTypeAll(typeof(Light));

        StringBuilder sb = new StringBuilder();
        sb.Append("\n烘焙光照需要隐藏:");
        bool flag = false;
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i] as Light;
            GameObject obj = light.gameObject;
            if (light.gameObject.activeInHierarchy == false)
            {
                continue;
            }
            if (light.lightmapBakeType == LightmapBakeType.Baked)
            {
                sb.Append(obj.transform.parent?.name + "/" + obj.name + ";");
                flag = true;
            }
        }
        if (flag)
        {
            return sb.ToString();
        }
        return string.Empty;
    }
}
