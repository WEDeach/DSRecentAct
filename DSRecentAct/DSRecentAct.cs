using DSRecentAct.Listener;
using DSRecentAct.Model;
using DSRecentAct.utils;
using Newtonsoft.Json;
using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Listen;
using OsuRTDataProvider.Mods;
using Sync;
using Sync.Command;
using Sync.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace DSRecentAct
{
    public class DSRecentAct : Plugin
    {
        private OsuListener OsuListener;

        private string OsuUserName;

        private int beatmapID;
        private int beatmapSetID;

        public string OsuFilePath { get; private set; }

        private Beatmap current_beatmap;
        private OsuStatus currentStatus;
        private OsuStatus lastStatus;
        private bool isReplayMode = false;
        private bool isUnRankMode = false;
        private ModsInfo current_mod;


        public DSRecentAct() : base("DSRecentAct", "DeachSword")
        {

            EventBus.BindEvent<PluginEvents.InitCommandEvent>(OnCommandInit);

            EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(OnAllPluginLoadedFinish);


            EventBus.BindEvent<PluginEvents.InitSourceEvent>(InitSourceEvent);

            ReflectorModel.config_manager = new Sync.Tools.PluginConfigurationManager(this);
            ReflectorModel.config_manager.AddItem(ReflectorModel.Setting);
        }
        public override void OnEnable()
        {
            Logger.WriteColor(this.Name + " 初始化~   作者: " + this.Author, ConsoleColor.Yellow);
            OsuListener = new OsuListener();
            OsuListener.OsuWorker.AddWork(TryFixStatusChange);

            GetPlayerData();
            ReflectorModel.mmf.SetupMmf();
        }



        private void InitSourceEvent(PluginEvents.InitSourceEvent e)
        {
            Logger.LogInfomation($"{e.Sources}");
        }

        private void OnAllPluginLoadedFinish(PluginEvents.LoadCompleteEvent e)
        {
            TryRegisterSourceFromOsuRTDataProvider(e.Host);
        }
        public void TryRegisterSourceFromOsuRTDataProvider(SyncHost host)
        {
            foreach (var plugin in host.EnumPluings())
            {
                if (plugin.Name == "OsuRTDataProvider")
                {
                    Logger.LogInfomation("OsuRTDataProvider plugin found!");
                    OsuRTDataProvider.OsuRTDataProviderPlugin reader = plugin as OsuRTDataProvider.OsuRTDataProviderPlugin;
                    ReflectorModel.ORTDP = reader;

                    ReflectorModel.ORTDP.ListenerManager.OnBeatmapChanged += OnCurrentBeatmapChange;
                    ReflectorModel.ORTDP.ListenerManager.OnStatusChanged += OnStatusChange;
                    ReflectorModel.ORTDP.ListenerManager.OnModsChanged += OnCurrentModsChange;
                    ReflectorModel.ORTDP.ListenerManager.OnPlayerChanged += OnPlayerChanged;
                    return;
                }
            }

            Logger.Error("OsuRTDataProvider plugin not found!");
        }

        private void OnPlayerChanged(string player)
        {

            if (lastStatus == OsuStatus.Rank) return;
            if (isReplayMode || isUnRankMode) return;
            OsuUserName = player;
            Logger.LogInfomation($"OnPlayerChanged: {OsuUserName}");
            if (ReflectorModel.OPD.Count == 0)
            {
                var od = ReflectorModel.OsuApi.GetOsuPlayer(OsuUserName);
                CheckPlayerData(od);
            }
        }

        private void OnCurrentBeatmapChange(Beatmap beatmap)
        {
            if (beatmap == Beatmap.Empty || string.IsNullOrWhiteSpace(beatmap?.FilenameFull))
            {
                //fix empty beatmap
                return;
            }

            beatmapID = beatmap.BeatmapID;
            beatmapSetID = beatmap.BeatmapSetID;
            OsuFilePath = beatmap.FilenameFull;
            current_beatmap = beatmap;

            Logger.LogInfomation($"OnCurrentBeatmapChange: {beatmap.Title}");
        }

        public void TryFixStatusChange()
        {
            var now_status = ReflectorModel.ORTDP.ListenerManager.GetCurrentOsuStatus();
            if(now_status != currentStatus)
            {
                Logger.LogInfomation($"狀態不同步...");
                OnStatusChange(currentStatus, now_status);
            }
        }
        private void OnStatusChange(OsuStatus last_status, OsuStatus status)
        {
            currentStatus = status;
            lastStatus = last_status;
            if (last_status == status) return;
            Logger.LogInfomation($"OnStatusChange: 狀態:{status}");
            if ((last_status == OsuStatus.Playing) && (status == OsuStatus.Rank) && ((!isReplayMode && !isUnRankMode) || Setting.DebugMode))
            {
                update_s:
                System.Threading.Thread.Sleep(5000);
                var od = ReflectorModel.OsuApi.GetOsuPlayer(OsuUserName);
                Logger.LogInfomation($"分數:{od.totalScore}");
                if (ReflectorModel.OPD.Count == 0 || od.totalScore != ReflectorModel.OPD.Last().totalScore)
                {
                    CheckPlayerData(od);
                }
                else
                {
                    goto update_s;
                }
                Logger.LogInfomation($"成績目前共有 {ReflectorModel.OPD.Count} 個");
            }
            else if ((last_status == OsuStatus.Rank) && (status == OsuStatus.Playing))
            {
                isReplayMode = true;
            }
            else if (status == OsuStatus.SelectSong || status == OsuStatus.MatchSetup)
            {
                isReplayMode = false;
            }
            else
            {
                if (current_mod.Mod != ModsInfo.Mods.Unknown || current_mod.Mod != ModsInfo.Mods.None)
                {
                    
                    return;
                }

            }
        }
        private void OnCurrentModsChange(ModsInfo mod)
        {
            if (current_mod.Mod == mod.Mod) return;

            current_mod = mod;

            if (mod.Mod == ModsInfo.Mods.Unknown)
            {
                //Not Playing
            }
            else
            {
                switch (mod.Mod)
                {
                    case ModsInfo.Mods.Autoplay:
                    case ModsInfo.Mods.AutoPilot:
                    case ModsInfo.Mods.ScoreV2:
                    case ModsInfo.Mods.TouchDevice:
                    case ModsInfo.Mods.Cinema:
                        isUnRankMode = true;
                        break;
                    default:
                        isUnRankMode = false;
                        break;
                }
            }
        }

        private void OnCommandInit(PluginEvents.InitCommandEvent e)
        {
            //绑定一个命令
            e.Commands.Dispatch.bind("dsra", HandleCommands, "對於 DSRecentAct 的命令");
        }

        private bool HandleCommands(Arguments args)
        {
            foreach (string param in args)
            {
                //logger.LogInfomation("param:" + param);
            }

            //发送MySimpleEvent事件，将调用各个绑定到此事件的回调
            Logger.LogInfomation("nOT fOUNd");

            return true;
        }

        private void CheckPlayerData(OsuPlayer player)
        {
            Logger.LogInfomation($"分數更新:{player.totalScore}");
            Logger.LogInfomation($"rank:{player.ppRank}");
            ReflectorModel.OPD.Add(player);
            ReflectorModel.mmf.UpdateMmf();
            SavePlayerData();
        }

        private void GetPlayerData()
        {
            if(File.Exists(@"./DSRA_PD.json")) ReflectorModel.OPD = JsonConvert.DeserializeObject<List<OsuPlayer>>(File.ReadAllText(@"../DSRA_PD.json"));
        }
        private void SavePlayerData()
        {
            File.WriteAllText(@"../DSRA_PD.json", JsonConvert.SerializeObject(ReflectorModel.OPD));
        }
        public override void OnDisable()
        {
            ReflectorModel.mmf.OnDisable();
        }
        public override void OnExit()
        {
            ReflectorModel.mmf.OnDisable();
            SavePlayerData();
        }
    }
}
