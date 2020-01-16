using DSRecentAct.Model;
using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSRecentAct
{
    class DSRASetting : IConfigurable
    {
        public ConfigurationElement OsuApiToken
        {
                get => Setting.OsuApiToken.ToString();
                set => Setting.OsuApiToken = value.ToString();
            }

            [Bool(RequireRestart = true)]
        public ConfigurationElement DebugMode
        {
            get => Setting.DebugMode.ToString();
            set => Setting.DebugMode = bool.Parse(value);
        }

        //当PluginConfigurationManager.AddItem()钦定此实例时候会读取config.ini的配置文件,加载后会调用此方法
        public void onConfigurationLoad()
        {
            if(string.IsNullOrEmpty(OsuApiToken)) utils.Logger.LogInfomation($"尚未設置OsuApiToken");
            ReflectorModel.OsuApi = new OsuApiModel(OsuApiToken);
        }

        public void onConfigurationReload()
        {
            if (!string.IsNullOrEmpty(OsuApiToken))
            {
                utils.Logger.LogInfomation($"成功設置OsuApiToken >.<!!");
                ReflectorModel.OsuApi = new OsuApiModel(OsuApiToken);
            }
        }

        public void onConfigurationSave()
        {

        }
    }
    class Setting
    {
        public static string OsuApiToken = "";
        public static bool DebugMode = false;

        public static object SongsPath { get; internal set; }
        public static string ForceOsuSongsDirectory { get; internal set; }
    }
}
