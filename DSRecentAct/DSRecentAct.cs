﻿using DSRecentAct.Listener;
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
using static DSRecentAct.Model.OsuApiModel;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace DSRecentAct
{
    [SyncPluginID("607a03aa-23c5-4ba4-b28d-c63f4ba2d013", PLUGIN_VERSION)]
    public class DSRecentAct : Plugin
    {
        public const string PLUGIN_VERSION = "1.0.7";
        public OsuListener OsuListener;

        private string OsuUserName;

        private int beatmapID;
        private int beatmapSetID;

        public string OsuFilePath { get; private set; }

        private Beatmap current_beatmap;
        private OsuStatus currentStatus;
        private OsuStatus lastStatus;
        private bool isReplayMode = false;
        private bool isUnRankMode = false;
        private bool isSelectSongLast = false;
        private ModsInfo current_mod;

        private bool selfORTDP = true;

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
            ReflectorModel.OsuListener = OsuListener;
            OsuListener.OsuWorker.AddWork(TryFixStatusChange);

            GetPlayerData();
            ReflectorModel.mmf.SetupMmf();
        }



        private void InitSourceEvent(PluginEvents.InitSourceEvent e)
        {
            //Logger.LogInfomation($"{e.Sources}");
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

                    ReflectorModel.ORTDP.ListenerManager.OnPlayerChanged += OnPlayerChanged;
                    ReflectorModel.ORTDP.ListenerManager.OnStatusChanged += OnStatusChange;
                    ReflectorModel.ORTDP.ListenerManager.OnModsChanged += OnCurrentModsChange;
                    ReflectorModel.ORTDP.ListenerManager.OnBeatmapChanged += OnCurrentBeatmapChange;
                    ReflectorModel.ORTDP.ListenerManager.OnScoreChanged += OnScoreChanged;
                    OsuUserName = Setting.UserName;
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
            if (ReflectorModel.OPD.Data.Count == 0)
            {
                var od = ReflectorModel.OsuApi.GetOsuPlayer(OsuUserName);
                CheckPlayerData(od);
            }
        }

        private void OnScoreChanged(int score)
        {
            ReflectorModel.CustomData.currentMapTotalScore = score;
            if(ReflectorModel.CustomData.currentMapBestScore > 0)
            {
                if(currentStatus != OsuStatus.Playing)
                {
                    ReflectorModel.mmf.UpdateMmf(23, $"歷史高分: {ReflectorModel.CustomData.currentMapBestScore}");
                }
                else if(score > ReflectorModel.CustomData.currentMapBestScore)
                {
                    ReflectorModel.mmf.UpdateMmf(23, $"【新紀錄】{score}");
                }
                else
                {
                    ReflectorModel.mmf.UpdateMmf(23, $"{ReflectorModel.CustomData.currentMapBestScore}/{score}");
                }
            }
        }

        private void OnCurrentBeatmapChange(Beatmap beatmap)
        {
            if (beatmap == Beatmap.Empty || string.IsNullOrWhiteSpace(beatmap?.FilenameFull))
            {
                //fix empty beatmap
                return;
            }

            if (beatmapID != beatmap.BeatmapID)
            {
                beatmapID = beatmap.BeatmapID;
                var od = ReflectorModel.OsuApi.GetUserBeatmapScore(OsuUserName, beatmapID);
                ReflectorModel.CustomData.currentMapBestScore = od.totalScore;
                ReflectorModel.CustomData.currentMapBestPp = od.pp;
                ReflectorModel.CustomData.currentMapBestCombo = od.maxCombo;
                if(od.totalScore > 0)
                {
                    Logger.LogInfomation($"歷史高分: {od.totalScore}");
                    Logger.LogInfomation($"歷史PP: {od.pp}");
                    Logger.LogInfomation($"歷史Rank: {od.Rank}");
                    ReflectorModel.mmf.UpdateMmf(23, $"歷史高分: {od.totalScore}");
                }
                else
                {
                    ReflectorModel.mmf.UpdateMmf(23, "沒有遊玩紀錄");
                }
            }
            beatmapSetID = beatmap.BeatmapSetID;
            OsuFilePath = beatmap.FilenameFull;
            current_beatmap = beatmap;
        }

        public void TryFixStatusChange()
        {
            try
            {
                var now_status = ReflectorModel.ORTDP.ListenerManager.GetCurrentOsuStatus();
                selfORTDP = true;
                if (now_status != currentStatus)
                {
                    OnStatusChange(currentStatus, now_status);
                }
            }
            catch (Exception)
            {
                if (selfORTDP) {
                    Logger.Error($"由於不是使用專用的ORTDP插件, 可能無法正常運行");
                    selfORTDP = false;
                }
            }
            
        }
        private void OnStatusChange(OsuStatus last_status, OsuStatus status)
        {
            currentStatus = status;
            lastStatus = last_status;
            if (last_status == status) return;
            if(Setting.DebugMode) Logger.LogInfomation($"OnStatusChange: 狀態:{status} (前次狀態: {last_status})");
            if (
                (
                    (last_status == OsuStatus.Playing && status == OsuStatus.Rank) || (!selfORTDP && (
                        (last_status == OsuStatus.Rank && status == OsuStatus.SelectSong) || (last_status == OsuStatus.Rank && status == OsuStatus.MatchSetup)
                    ))
                ) && ((!isReplayMode && !isUnRankMode) || Setting.DebugMode)
            )
            {
                var loopc = 0;
                var loopmc = 5;
                if (!selfORTDP) loopmc = 0;
                update_s:
                System.Threading.Thread.Sleep(5000);
                var od = ReflectorModel.OsuApi.GetOsuPlayer(OsuUserName);
                if (ReflectorModel.OPD.Data.Count == 0 || od.totalScore != ReflectorModel.OPD.Data.Last().totalScore)
                {
                    if (ReflectorModel.OPD.Data.Count > 0) Logger.LogInfomation($"原分數:{ReflectorModel.OPD.Data.Last().totalScore}");
                    CheckPlayerData(od);
                }
                else
                {
                    loopc++;
                    if (!isReplayMode && !isUnRankMode && loopc <= loopmc) goto update_s;
                    if (Setting.DebugMode) Logger.Error($"無法獲得成績...");
                }
            }

            if (isSelectSongLast && (last_status == OsuStatus.Rank) && (status == OsuStatus.Playing))
            {
                isReplayMode = true;
                if (Setting.DebugMode) Logger.LogInfomation($"isReplayMode: On");
            }

            if (status == OsuStatus.SelectSong || status == OsuStatus.MatchSetup)
            {
                if (Setting.DebugMode && isReplayMode) Logger.LogInfomation($"isReplayMode: Off");
                isReplayMode = false;
                isSelectSongLast = true;
            }
            else
            {
                isSelectSongLast = false;
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
            var reply = "{CMD_DEF_REPLY_MSG}";
            if(args.Count > 0)
            {
                switch (args[0])
                {
                    case "ud":
                        ReflectorModel.mmf.UpdateMmf();
                        reply = "OK.";
                        break;
                    default:
                        reply = "未知指令";
                        break;
                }
            }

            Logger.LogInfomation(reply);

            return true;
        }

        private void CheckPlayerData(OsuPlayerData player)
        {
            if (Setting.DebugMode)
            {
                Logger.LogInfomation($"分數更新: {player.totalScore}");
                Logger.LogInfomation($"rank: {player.ppRank}");
            }
            ReflectorModel.OPD.Data.Add(player);
            ReflectorModel.mmf.UpdateMmf();
            SavePlayerData();
        }

        public static string JSON_DATA_VERSION = "v1.2.0";
        private void GetPlayerData()
        {
            if (File.Exists(@"../DSRA_PD.json"))
            {
                Logger.LogInfomation($"即將讀取JSON資料...");
                try
                {
                    ReflectorModel.OPD = JsonConvert.DeserializeObject<OsuPlayer>(File.ReadAllText(@"../DSRA_PD.json"));
                }
                catch
                {
                    try
                    {
                        ReflectorModel.OPD.Data = JsonConvert.DeserializeObject<List<OsuPlayerData>>(File.ReadAllText(@"../DSRA_PD.json")); // v1.0.0
                    }
                    catch (Exception e)
                    {
                        Logger.LogInfomation($"JSON資料讀取失敗: {e}");
                        ReflectorModel.OPD = new OsuPlayer()
                        {
                            DataVersion = JSON_DATA_VERSION
                        };
                    }
                }

            }
            else
            {
                ReflectorModel.OPD = new OsuPlayer()
                {
                    DataVersion = JSON_DATA_VERSION
                };
            }
            Logger.LogInfomation($"JSON資料: {ReflectorModel.OPD.DataVersion}");
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
            //SavePlayerData();
        }
    }
}
