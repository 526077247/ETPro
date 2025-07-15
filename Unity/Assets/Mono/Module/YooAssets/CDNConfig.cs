using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Serializable][CreateAssetMenu(menuName = "CDNConfig")]
    public class CDNConfig:ScriptableObject
    {
        /// <summary>
        /// 渠道
        /// </summary>
        public string Channel;
        /// <summary>
        /// 资源地址
        /// </summary>
        public string DefaultHostServer;
        /// <summary>
        /// 资源地址备用
        /// </summary>
        public string FallbackHostServer;
        
        /// <summary>
        /// 更新列表
        /// </summary>
        public string UpdateListUrl;
        /// <summary>
        /// 白名单更新列表地址
        /// </summary>
        public string TestUpdateListUrl;

        /// <summary>
        /// 是否打AOT
        /// </summary>
        public bool BuildHotfixAssembliesAOT;
    }
}