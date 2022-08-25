namespace ET
{
    [FriendClass(typeof(UIRouterComponent))]
    public static class UIRouterComponentSystem
    {
        
        public class AkwakeSystem:AwakeSystem<UIRouterComponent>
        {
            public override void Awake(UIRouterComponent self)
            {
                UIRouterComponent.Instance = self;
                self.RouterMapPath = new MultiDictionary<string, string, UIRouterConfig>();
                for (int i = 0; i < UIRouterConfigCategory.Instance.GetAllList().Count; i++)
                {
                    var item = UIRouterConfigCategory.Instance.GetAllList()[i];
                    self.RouterMapPath.Add(item.From,item.To,item);
                }
            }
        }


        /// <summary>
        /// 获取到目标界面最短路径,广度优先
        /// </summary>
        /// <param name="self"></param>
        /// <param name="from"></param>
        /// <param name="aim"></param>
        /// <returns></returns>
        public static UIRouterConfig GetNextWay(this UIRouterComponent self,string from, string aim)
        {
            ListComponent<string> Ways1 = ListComponent<string>.Create();
            ListComponent<string> Ways2 = ListComponent<string>.Create();
            DictionaryComponent<string,UIRouterConfig> Temp = DictionaryComponent<string, UIRouterConfig>.Create();
            bool isFirst = true;
            Ways1.Add(from);
            while (Ways1.Count>0)
            {
                Ways2.Clear();
                for (int i = 0; i < Ways1.Count; i++)
                {
                    if (self.RouterMapPath.TryGetDic(Ways1[i], out var dic))
                    {
                        foreach (var item in dic)
                        {
                            var key = "";
                            if (isFirst)
                            {
                                key = item.Key;
                                Temp.Add(key,item.Value);
                            }
                            else
                            {
                                key = Ways1[i] + item.Key;
                                Temp.Add(key,Temp[Ways1[i]]);
                            }
                            if (item.Key != aim)
                            {
                                Ways2.Add(item.Key);
                            }
                            else
                            {
                                Ways1.Dispose();
                                Ways2.Dispose();
                                var res = Temp[key];
                                Temp.Dispose();
                                return res;
                            }
                            
                        }
                    }
                }
                ObjectHelper.Swap(ref Ways2,ref Ways1);
                isFirst = false;
            }
            Ways1.Dispose();
            Ways2.Dispose();
            Temp.Dispose();
            return null;
            
        }
    }
}