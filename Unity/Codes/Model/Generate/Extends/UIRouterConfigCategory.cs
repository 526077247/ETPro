namespace ET
{
    public partial class UIRouterConfigCategory
    {
        private UnOrderDoubleKeyDictionary<string, string, UIRouterConfig> routerMapPath;

        public override void AfterEndInit()
        {
            base.AfterEndInit();
            routerMapPath = new UnOrderDoubleKeyDictionary<string, string, UIRouterConfig>();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                routerMapPath.Add(item.From,item.To,item);
            }
        }
        
        /// <summary>
        /// 获取到目标界面最短路径,广度优先
        /// </summary>
        /// <param name="from"></param>
        /// <param name="aim"></param>
        /// <returns></returns>
        public UIRouterConfig GetNextWay(string from, string aim)
        {
            ListComponent<string> ways1 = ListComponent<string>.Create();
            ListComponent<string> ways2 = ListComponent<string>.Create();
            DictionaryComponent<string,UIRouterConfig> temp = DictionaryComponent<string, UIRouterConfig>.Create();
            bool isFirst = true;
            ways1.Add(from);
            while (ways1.Count>0)
            {
                ways2.Clear();
                for (int i = 0; i < ways1.Count; i++)
                {
                    if (routerMapPath.TryGetDic(ways1[i], out var dic))
                    {
                        foreach (var item in dic)
                        {
                            var key = "";
                            if (isFirst)
                            {
                                key = item.Key;
                                temp.Add(key,item.Value);
                            }
                            else
                            {
                                key = ways1[i] + item.Key;
                                temp.Add(key,temp[ways1[i]]);
                            }
                            if (item.Key != aim)
                            {
                                ways2.Add(item.Key);
                            }
                            else
                            {
                                ways1.Dispose();
                                ways2.Dispose();
                                var res = temp[key];
                                temp.Dispose();
                                return res;
                            }
                            
                        }
                    }
                }
                (ways2, ways1) = (ways1, ways2);
                isFirst = false;
            }
            ways1.Dispose();
            ways2.Dispose();
            temp.Dispose();
            return null;
            
        }
    }
}