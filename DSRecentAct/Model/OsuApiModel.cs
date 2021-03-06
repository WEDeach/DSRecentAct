﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSRecentAct.utils;
using Newtonsoft.Json;

namespace DSRecentAct.Model
{
    class OsuApiModel
    {
        public string token;
        private WebClient client = new WebClient();

        public OsuApiModel(string a = null)
        {
            if (!string.IsNullOrEmpty(a)) token = a;
        }

        public OsuPlayerData GetOsuPlayer(string name)
        {
            if (string.IsNullOrEmpty(token))
            {
                //wait...
                Logger.Error($"無法取得UserData, 請設置Osu API Token!");
                return new OsuPlayerData();
            }
            var url = $"https://osu.ppy.sh/api/get_user?k={token}&u={name}";

            string b = client.DownloadString(url);
            if (b != null)
            {
                try
                {
                    List<OsuPlayerData> c = JsonConvert.DeserializeObject<List<OsuPlayerData>>(b);
                    return c[0];
                }
                catch (Exception)
                {
                    Logger.Error($"[無法解析成績]{b}");

                }

            }
            else
            {
                Logger.LogInfomation($"無法取得用戶資料(error-20)");
            }
            return new OsuPlayerData();
        }
        public Score GetUserBeatmapScore(string userName, int bid, int mode = 0)
        {
            if (string.IsNullOrEmpty(token))
            {
                Logger.Error($"無法取得UserBeatmapScore, 請設置Osu API Token!");
            }
            else if (string.IsNullOrEmpty(userName))
            {
                Logger.Error($"無法取得userName, 請嘗試設置名稱?");
            }
            else
            {
                try
                {
                    var url = $"https://osu.ppy.sh/api/get_scores?k={token}&m={mode}&u={userName}&b={bid}";
                    string response = client.DownloadString(url);
                    List<Score> u = JsonConvert.DeserializeObject<List<Score>>(response);
                    return u[0];
                }
                catch (WebException w)
                {
                    Logger.Error($"[無法獲取成績]{w.Message}");
                }
                catch (Exception e)
                {
                }
            }
            return new Score();
        }

        public class OsuPlayer
        {
            [JsonProperty("data")]
            public List<OsuPlayerData> Data { get; set; } = new List<OsuPlayerData>();

            [JsonProperty("data_version")]
            public string DataVersion { get; set; } = "v1.0.0";

            [JsonProperty("last_pp_rank_change")]
            public string LastPPRankChange { get; set; } = "0";

            [JsonProperty("last_pp_country_ranking_change")]
            public string LastPPCountryRankChange { get; set; } = "0";

            [JsonProperty("last_best_pp_rank")]
            public BestRank LastBestPPRank { get; set; } = new BestRank();

            [JsonProperty("last_best_pp_country_ranking")]
            public BestRank LastBestPPCountryRank { get; set; } = new BestRank();

        }

        public class BestRank
        {
            [JsonProperty("rank")]
            public int Rank { get; set; } = 0;

            [JsonProperty("created_time")]
            public DateTime CreatedTime { get; set; } = DateTime.Now;
        }

        public class OsuPlayerData
        {
            [JsonProperty("user_id")]
            public int userId { get; set; }

            [JsonProperty("username")]
            public string userName { get; set; }

            [JsonProperty("join_date")]
            public string joinDate { get; set; }

            [JsonProperty("count300")]
            public int count300 { get; set; }

            [JsonProperty("count100")]
            public int count100 { get; set; }

            [JsonProperty("count50")]
            public int count50 { get; set; }

            [JsonProperty("playcount")]
            public int playCount { get; set; }

            [JsonProperty("ranked_score")]
            public long rankedScore { get; set; }

            [JsonProperty("total_score")]
            public long totalScore { get; set; }

            [JsonProperty("pp_rank")]
            public int ppRank { get; set; }

            [JsonProperty("level")]
            public double Level { get; set; }

            [JsonProperty("pp_raw")]
            public double ppRaw { get; set; }

            [JsonProperty("accuracy")]
            public double Accuracy { get; set; }

            [JsonProperty("count_rank_ss")]
            public int countRankSS { get; set; }

            [JsonProperty("count_rank_ssh")]
            public int countRankSSH { get; set; }

            [JsonProperty("count_rank_s")]
            public int countRankS { get; set; }

            [JsonProperty("count_rank_sh")]
            public int countRankSH { get; set; }

            [JsonProperty("count_rank_a")]
            public int countRankA { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("pp_country_rank")]
            public int ppCountryRank { get; set; }

            [JsonProperty("events")]
            public List<Event> Events { get; set; }
        }

        public class Score
        {
            [JsonProperty("score")]
            public long totalScore { get; set; }

            [JsonProperty("username")]
            public string userName { get; set; }

            [JsonProperty("count300")]
            public int count300 { get; set; }

            [JsonProperty("count100")]
            public int count100 { get; set; }

            [JsonProperty("count50")]
            public int count50 { get; set; }

            [JsonProperty("countmiss")]
            public int countMiss { get; set; }

            [JsonProperty("maxcombo")]
            public int maxCombo { get; set; }

            [JsonProperty("countkatu")]
            public int countKatu { get; set; }

            [JsonProperty("countgeki")]
            public int countGeki { get; set; }

            [JsonProperty("perfect")]
            [JsonConverter(typeof(BoolConverter))]
            public bool Perfect { get; set; }

            [JsonProperty("enabled_mods")]
            public int enabledMods { get; set; }

            [JsonProperty("user_id")]
            public int userId { get; set; }

            [JsonProperty("date")]
            public DateTime Date { get; set; }

            [JsonProperty("rank")]
            public string Rank { get; set; }

            [JsonProperty("pp")]
            public double? pp { get; set; }
        }

        public class Event
        {
            [JsonProperty("display_html")]
            public string displayHtml { get; set; }

            [JsonProperty("beatmap_id")]
            public string beatmapId { get; set; }

            [JsonProperty("beatmapset_id")]
            public string beatmapsetId { get; set; }

            [JsonProperty("date")]
            public DateTime date { get; set; }

            [JsonProperty("epicfactor")]
            public int epicFactor { get; set; }
        }
    }
}
