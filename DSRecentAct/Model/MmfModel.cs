using DSRecentAct.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DSRecentAct.Model.OsuApiModel;

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
                "DSRA-ppCountryRank-b",
                "DSRA-currentMapTotalScore",
                "DSRA-currentMapPp",
                "DSRA-currentMapAcc",
                "DSRA-currentMapBestScore",
                "DSRA-currentMapBestPp",
                "DSRA-currentMapBestAcc"
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

            if (ReflectorModel.OPD.Data.Count > 0)
            {
                OsuPlayerData player = ReflectorModel.OPD.Data.Last(); //get last player data
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
                    //var d = "";
                    var er = Regex.Match(player.Events[0].displayHtml, @"\<img src='\\/images\\/*(?<Rank>.*?)_small.png'\\/\>", RegexOptions.IgnoreCase).Groups["Rank"].Value;
                    var eu = Regex.Match(player.Events[0].displayHtml, @"\<a href='\/u\/[^']*'\>(?<Rank>.*?)\<\/a\>", RegexOptions.IgnoreCase).Groups["Rank"].Value;
                    var erk = Regex.Match(player.Events[0].displayHtml, @"achieved rank *(?<Rank>.*?) on ", RegexOptions.IgnoreCase).Groups["Rank"].Value;
                    var est = Regex.Match(player.Events[0].displayHtml, @"\<a href='\/b\/[^']*'\>(?<Rank>.*?)\<\/a\>", RegexOptions.IgnoreCase).Groups["Rank"].Value;
                    streamWriters[16].Write($"{eu} achieved rank {erk} on {est}");
                    streamWriters[17].Write($"{eu} achieved rank {erk}"); // A
                    streamWriters[18].Write($"{est}"); // B
                }

                if (ReflectorModel.OPD.Data.Count > 1)
                {
                    var f = ReflectorModel.OPD.Data[ReflectorModel.OPD.Data.Count - 2]; //get prev of last player data(?
                    var gap = (f.ppRank - player.ppRank).ToString();
                    var gap2 = (f.ppCountryRank - player.ppCountryRank).ToString();
                    if (f.ppRank - player.ppRank > 0) gap = $"+{gap}";
                    if (f.ppCountryRank - player.ppCountryRank > 0) gap2 = $"+{gap2}";
                    if (f.ppRank - player.ppRank != 0) ReflectorModel.OPD.LastPPRankChange = $"{gap}";
                    if (f.ppCountryRank - player.ppCountryRank != 0) ReflectorModel.OPD.LastPPCountryRankChange = $"{gap2}";
                    

                }

                if (ReflectorModel.OPD.LastBestPPRank.Rank <= 0 || player.ppRank < ReflectorModel.OPD.LastBestPPRank.Rank) ReflectorModel.OPD.LastBestPPRank = new BestRank() { Rank = player.ppRank };
                if (ReflectorModel.OPD.LastBestPPCountryRank.Rank <= 0 || player.ppCountryRank < ReflectorModel.OPD.LastBestPPCountryRank.Rank) ReflectorModel.OPD.LastBestPPCountryRank = new BestRank() { Rank = player.ppCountryRank };

                streamWriters[19].Write($"({ReflectorModel.OPD.LastPPRankChange})");
                streamWriters[20].Write($"({ReflectorModel.OPD.LastPPCountryRankChange})");
                if(ReflectorModel.OPD.LastBestPPRank.Rank > 0) streamWriters[21].Write($"#{ReflectorModel.OPD.LastBestPPRank.Rank}({ReflectorModel.OPD.LastBestPPRank.CreatedTime.ToString("MM/dd/yyyy")})");
                if (ReflectorModel.OPD.LastBestPPCountryRank.Rank > 0) streamWriters[22].Write($"#{ReflectorModel.OPD.LastBestPPCountryRank.Rank}({ReflectorModel.OPD.LastBestPPCountryRank.CreatedTime.ToString("MM/dd/yyyy")})");

                if (ReflectorModel.CustomData.currentMapTotalScore > ReflectorModel.CustomData.currentMapBestScore)
                {
                    var _scoreDiff = ReflectorModel.CustomData.currentMapTotalScore - ReflectorModel.CustomData.currentMapBestScore;
                    streamWriters[23].Write($"【新紀錄】{ReflectorModel.CustomData.currentMapTotalScore} (+{_scoreDiff})");
                }
                else
                {
                    streamWriters[23].Write($"歷史高分: {ReflectorModel.CustomData.currentMapBestScore}");
                }

            }
            else
            {
                Logger.Error($"資料數為0...");
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
        }

        public void UpdateMmf(int index, string data)
        {
            Clear(index);
            StreamWriter[] streamWriters = new StreamWriter[1];
            foreach (var a in _mmfName)
            {
                var b = _mmfName.IndexOf(a);
                if (b == index)
                {
                    streamWriters[0] = new StreamWriter(_mmfs[b].CreateViewStream());
                    streamWriters[0].Write(data);
                    streamWriters[0].Write('\0');
                    streamWriters[0].Dispose();
                    return;
                }
            }
        }

        public void Clear(int? index = null)
        {
            var i = 0;
            foreach (var mmf in _mmfs)
            {
                if(index == null || i == index)
                    if (mmf != null)
                        using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                            stream.WriteByte(0);
                i++;
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
