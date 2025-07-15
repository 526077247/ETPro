using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class I18NComponentAwakeSystem : AwakeSystem<I18NComponent>
    {
        public override void Awake(I18NComponent self)
        {
            I18NComponent.Instance = self;
            I18NBridge.Instance.GetValueByKey = self.I18NGetText;
            var lang = PlayerPrefs.GetInt(CacheKeys.CurLangType, -1);
            if (lang < 0)
            {
                self.CurLangType = Application.systemLanguage == SystemLanguage.Chinese ||
                        Application.systemLanguage == SystemLanguage.ChineseSimplified ||
                        Application.systemLanguage == SystemLanguage.ChineseTraditional
                                ? LangType.Chinese
                                : LangType.English;
            }
            else
            {
                self.CurLangType = (LangType)lang;
            }
            self.I18nTextKeyDic = new Dictionary<int, string>();
            self.I18NEntity = new Dictionary<long, Entity>();
            InitAsync(self).Coroutine();
#if !UNITY_WEBGL
            self.AddSystemFonts();
#endif
        }
        
        private async ETTask InitAsync(I18NComponent self)
        {
            var res = await ConfigComponent.Instance.LoadOneConfig<I18NConfigCategory>(self.CurLangType.ToString());
            for (int i = 0; i <res.GetAllList().Count; i++)
            {
                var item = res.GetAllList()[i];
                self.I18nTextKeyDic.Add(item.Id, item.Value);
            }
        }
    }
    [ObjectSystem]
    public class I18NComponentDestroySystem : DestroySystem<I18NComponent>
    {
        public override void Destroy(I18NComponent self)
        {
            I18NComponent.Instance = null;
            I18NBridge.Instance.GetValueByKey = null;
            self.I18nTextKeyDic.Clear();
            self.I18nTextKeyDic = null;
        }
    }
    [FriendClass(typeof(I18NComponent))]
    public static class I18NComponentSystem
    {
        /// <summary>
        /// 取不到返回key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string I18NGetText(this I18NComponent self, string key)
        {
            if (!I18NKey.TryParse(key,out I18NKey i18nKey) || !self.I18nTextKeyDic.TryGetValue((int)i18nKey, out var result))
            {
                Log.Error("多语言key未添加！ " + key);
                result = key;
                return result;
            }

            return result;
        }
        public static string I18NGetText(this I18NComponent self, I18NKey key)
        {
            if (!self.I18nTextKeyDic.TryGetValue((int)key, out var value))
            {
                Log.Info("多语言key未添加！ " + key);
                return key.ToString();
            }
            return value;
        }

        /// <summary>
        /// 根据key取多语言取不到返回key
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static string I18NGetParamText(this I18NComponent self, I18NKey key, params object[] paras)
        {
            if (!self.I18nTextKeyDic.TryGetValue((int)key, out var value))
            {
                Log.Error("多语言key未添加！ " + key);
                return key.ToString();
            }
            if (paras != null)
                return string.Format(value, paras);
            else
                return value;
        }
        /// <summary>
        /// 取不到返回key
        /// </summary>
        /// <param name="self"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool I18NTryGetText(this I18NComponent self, I18NKey key, out string result)
        {
            if (!self.I18nTextKeyDic.TryGetValue((int)key, out result))
            {
                Log.Info("多语言key未添加！ " + key);
                result = key.ToString();
                return false;
            }

            return true;
        }
        
        public static bool I18NTryGetText(this I18NComponent self, string key, out string result)
        {
            if (!I18NKey.TryParse(key,out I18NKey i18nKey) || !self.I18nTextKeyDic.TryGetValue((int)i18nKey, out result))
            {
                Log.Info("多语言key未添加！ " + key);
                result = key;
                return false;
            }

            return true;
        }
        /// <summary>
        /// 切换语言,外部接口
        /// </summary>
        /// <param name="langType"></param>
        public static async ETTask SwitchLanguage(this I18NComponent self, LangType langType)
        {
            //修改当前语言
            PlayerPrefs.SetInt(CacheKeys.CurLangType, (int)langType);
            self.CurLangType = langType;
            var res = await ConfigComponent.Instance.LoadOneConfig<I18NConfigCategory>(self.CurLangType.ToString());
            self.I18nTextKeyDic.Clear();
            for (int i = 0; i <res.GetAllList().Count; i++)
            {
                var item = res.GetAllList()[i];
                self.I18nTextKeyDic.Add(item.Id, item.Value);
            }

            I18NBridge.Instance.OnLanguageChangeEvt?.Invoke();
        }

        public static void RegisterI18NEntity(this I18NComponent self,Entity entity)
        {
            if(!self.I18NEntity.ContainsKey(entity.Id))
                self.I18NEntity.Add(entity.Id,entity);
        }
        
        public static void RemoveI18NEntity(this I18NComponent self,Entity entity)
        {
            self.I18NEntity.Remove(entity.Id);
        }

        public static LangType GetCurLanguage(this I18NComponent self)
        {
            return self.CurLangType;
        }

        #region 添加系统字体

        /// <summary>
        /// 需要就添加
        /// </summary>
        public static void AddSystemFonts(this I18NComponent self)
        {
#if UNITY_EDITOR||UNITY_STANDALONE_WIN
            string[] fonts = new[] { "msyhl" };//微软雅黑细体
#elif UNITY_ANDROID
            string[] fonts = new[] {
                "notosanscjksc-regular",
                "notosanscjk-regular",
            };
#elif UNITY_IOS
            string[] fonts = new[] {
                "pingfang" // 注意内存占用70m+
            };
#else
            string[] fonts = new string[0];
#endif
            TextMeshFontAssetManager.Instance.AddWithOSFont(fonts);
        }

        #endregion
    }
}
