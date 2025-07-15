using System.Collections.Generic;
using System.IO;

namespace ET
{
    public class ConfigLoader: IConfigLoader
    {
        public async ETTask GetAllConfigBytes(Dictionary<string, byte[]> output)
        {
            await ETTask.CompletedTask;
            foreach (string file in Directory.GetFiles($"../Config", "*.bytes"))
            {
                string key = Path.GetFileNameWithoutExtension(file);
                output[key] = File.ReadAllBytes(file);
            }
            output["StartMachineConfigCategory"] = File.ReadAllBytes($"../Config/{Game.Options.StartConfig}/StartMachineConfigCategory.bytes");
            output["StartProcessConfigCategory"] = File.ReadAllBytes($"../Config/{Game.Options.StartConfig}/StartProcessConfigCategory.bytes");
            output["StartSceneConfigCategory"] = File.ReadAllBytes($"../Config/{Game.Options.StartConfig}/StartSceneConfigCategory.bytes");
            output["StartZoneConfigCategory"] = File.ReadAllBytes($"../Config/{Game.Options.StartConfig}/StartZoneConfigCategory.bytes");
        }
        
        public async ETTask<byte[]> GetOneConfigBytes(string configName)
        {
            await ETTask.CompletedTask;
            byte[] configBytes = File.ReadAllBytes($"../Config/{configName}.bytes");
            return configBytes;
        }
    }
}