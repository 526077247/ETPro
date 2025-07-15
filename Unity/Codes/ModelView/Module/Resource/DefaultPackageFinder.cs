namespace ET
{
    public class DefaultPackageFinder:IPackageFinder
    {
        public string GetPackageName(string path)
        {
            return Define.DefaultName;
        }
    }
}