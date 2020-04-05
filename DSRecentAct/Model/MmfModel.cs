using DSRecentAct.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DSRecentAct.Model
{
    class MmfModel
    {
        public static List<string> _mmfName = new List<string>()
            {
                "DSRA",
                "DSRA-ppRank",
                "DSRA-totalScore",
                "DSRA-acc",
                "DSRA-count50",
                "DSRA-count100",
                "DSRA-count300",
                "DSRA-countRankA",
                "DSRA-countRankS",
                "DSRA-countRankSS",
                "DSRA-playCount",
                "DSRA-ppCountryRank",
                "DSRA-pp",
                "DSRA-rankedScore",
                "DSRA-countRankSH",
                "DSRA-countRankSSH",
                "DSRA-RE",
                "DSRA-RE-A",
                "DSRA-RE-B",
                "DSRA-ppRank-g",
                "DSRA-ppCountryRank-g",
                "DSRA-ppRank-b",
                "DSRA-ppCountryRank-b"
            };
        public MemoryMappedFile[] _mmfs = new MemoryMappedFile[_mmfName.Count];
        public void SetupMmf()
        {
            foreach (var _mmfn in _mmfName)
            {
                _mmfs[_mmfName.IndexOf(_mmfn)] = MemoryMappedFile.CreateOrOpen(_mmfn, 1024);
            }
            UpdateMmf();
        }

        public void UpdateMmf()
        {
            Clear();
            StreamWriter[] streamWriters = new StreamWriter[_mmfs.Length];
            foreach (var a in _mmfName)
            {
                var b = _mmfName.IndexOf(a);
                streamWriters[b] = new StreamWriter(_mmfs[b].CreateViewStream());
            }

            if (Setting.DebugMode) Logger.LogInfomation($"更新MMF資料...");
            if (ReflectorModel.OPD.Data.Count > 0)
            {
                if (Setting.DebugMode) Logger.LogInfomation($"取得目前成績...");
                OsuPlayerData player = ReflectorModel.OPD.Data.Last(); //get last player data
                if (Setting.DebugMode) Logger.LogInfomation($"準備更新{streamWriters.Length}筆MMF資料...");
                streamWriters[1].Write(player.ppRank.ToString());
                streamWriters[2].Write(player.totalScore);
                streamWriters[3].Write(player.Accuracy);
                streamWriters[4].Write(player.count50);
                streamWriters[5].Write(player.count100);
                streamWriters[6].Write(player.count300);
                streamWriters[7].Write(player.countRankA);
                streamWriters[8].Write(player.countRankS);
                streamWriters[9].Write(player.countRankSS);
                streamWriters[10].Write(player.playCount);
                streamWriters[11].Write(player.ppCountryRank);
                streamWriters[12].Write(player.ppRaw);
                streamWriters[13].Write(player.rankedScore);
                streamWriters[14].Write(player.countRankSH);
                streamWriters[15].Write(player.countRankSSH);

                if(player.Events.Count > 0)
                {
                    if (Setting.DebugMode) Logger.LogInfomation($"取得RK資料... 資料共有{player.Events.Count}筆");
                    //var d = "";
                    var er = Regex.Match(player.Events[0].displayHtml, @"\<img src='\\/images\\/*(?<Rank>.*?)_small.png'\\/\>", RegexOptions.IgnoreCase).Groups["Rank"].Value;
                    var eu = Regex.Match(player.Events[0].displayHtml, @"\<a href='\/u\/[^']*'\>(?<Rank>.*?)\<\/a\>", RegexOptions.IgnoreCase).Groups["Rank"].Value;
                    var erk = Regex.Match(player.Events[0].displayHtml, @"achieved rank *(?<Rank>.*?) on ", RegexOptions.IgnoreCase).Groups["Rank"].Value;
                    var est = Regex.Match(player.Events[0].displayHtml, @"\<a href='\/b\/[^']*'\>(?<Rank>.*?)\<\/a\>", RegexOptions.IgnoreCase).Groups["Rank"].Value;
                    if (Setting.DebugMode) Logger.LogInfomation($"取得RK資料完成! 繼續更新mmf...");
                    streamWriters[16].Write($"{eu} achieved rank {erk} on {est}");
                    streamWriters[17].Write($"{eu} achieved rank {erk}"); // A
                    streamWriters[18].Write($"{est}"); // B
                    if (Setting.DebugMode) Logger.LogInfomation($"RK mmf資料更新完成! 繼續更新mmf...");
                }

                if (ReflectorModel.OPD.Data.Count > 1)
                {
                    if (Setting.DebugMode) Logger.LogInfomation($"比對前一筆成績...");
                    var f = ReflectorModel.OPD.Data[ReflectorModel.OPD.Data.Count - 2]; //get prev of last player data(?
                    var gap = (f.ppRank - player.ppRank).ToString();
                    var gap2 = (f.ppCountryRank - player.ppCountryRank).ToString();
                    if (f.ppRank - player.ppRank > 0) gap = $"+{gap}";
                    if (f.ppCountryRank - player.ppCountryRank > 0) gap2 = $"+{gap2}";
                    if (f.ppRank - player.ppRank != 0) ReflectorModel.OPD.LastPPRankChange = $"{gap}";
                    if (f.ppCountryRank - player.ppCountryRank != 0) ReflectorModel.OPD.LastPPCountryRankChange = $"{gap2}";
                    

                    if (Setting.DebugMode) Logger.LogInfomation($"比對完成");
                }

                if (ReflectorModel.OPD.LastBestPPRank.Rank <= 0 || player.ppRank < ReflectorModel.OPD.LastBestPPRank.Rank) ReflectorModel.OPD.LastBestPPRank = new BestRank() { Rank = player.ppRank };
                if (ReflectorModel.OPD.LastBestPPCountryRank.Rank <= 0 || player.ppCountryRank < ReflectorModel.OPD.LastBestPPCountryRank.Rank) ReflectorModel.OPD.LastBestPPCountryRank = new BestRank() { Rank = player.ppCountryRank };

                streamWriters[19].Write($"({ReflectorModel.OPD.LastPPRankChange})");
                streamWriters[20].Write($"({ReflectorModel.OPD.LastPPCountryRankChange})");
                if(ReflectorModel.OPD.LastBestPPRank.Rank > 0) streamWriters[21].Write($"#{ReflectorModel.OPD.LastBestPPRank.Rank}({ReflectorModel.OPD.LastBestPPRank.CreatedTime.ToString("MM/dd/yyyy")})");
                if (ReflectorModel.OPD.LastBestPPCountryRank.Rank > 0) streamWriters[22].Write($"#{ReflectorModel.OPD.LastBestPPCountryRank.Rank}({ReflectorModel.OPD.LastBestPPCountryRank.CreatedTime.ToString("MM/dd/yyyy")})");

                if (Setting.DebugMode) Logger.LogInfomation($"更新MMF完成");
            }
            else
            {
                Logger.LogInfomation($"資料數為0...");
                foreach(var sw in streamWriters)
                {
                    sw.Write(" ?");
                }
            }

            foreach (var c in streamWriters)
            {
                c.Write('\0');
                c.Dispose();
            }

            if (Setting.DebugMode) Logger.LogInfomation($"更新MMF完成");
        }

        public void Clear()
        {

            foreach (var mmf in _mmfs)
            {
                if (mmf != null)
                    using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                        stream.WriteByte(0);
            }
        }

        public void OnDisable()
        {
            Clear();
            foreach (var mmf in _mmfs)
            {
                mmf?.Dispose();
            }
        }
    }
}
