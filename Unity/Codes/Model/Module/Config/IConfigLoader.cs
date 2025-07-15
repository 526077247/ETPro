using System.Collections.Generic;

namespace ET
{
    public interface IConfigLoader
    {
        ETTask GetAllConfigBytes(Dictionary<string, byte[]> output);
        ETTask<byte[]> GetOneConfigBytes(string configName);
    }
}