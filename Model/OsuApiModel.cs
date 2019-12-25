using System;
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

        public OsuApiModel(string a = null)
        {
            if (!string.IsNullOrEmpty(a)) token = a;
        }

        public OsuPlayerData GetOsuPlayer(string name)
        {
            if (string.IsNullOrEmpty(token)) {
                //wait...
                Logger.LogInfomation($"無法取得UserData, 請設置Osu API Token!");
                return new OsuPlayerData();
            }
            var url = $"https://osu.ppy.sh/api/get_user?k={token}&u={name}";
            WebClient a = new WebClient();

            string b = a.DownloadString(url);
            if (b != null)
            {
                List<OsuPlayerData> c = JsonConvert.DeserializeObject<List<OsuPlayerData>>(b);
                return c[0];
            }
            else
            {
                Logger.LogInfomation($"無法取得用戶資料(error-20)");
            }
            return new OsuPlayerData();
        }
    }

    public class OsuPlayer
    {
        [JsonProperty("data")]
        public List<OsuPlayerData> Data { get; set; }

        [JsonProperty("data_version")]
        public string DataVersion { get; set; } = "v1.0.0";

        [JsonProperty("last_pp_rank_change")]
        public string LastPPRankChange { get; set; } = "0";

        [JsonProperty("last_pp_country_ranking_change")]
        public string LastPPCountryRankChange { get; set; } = "0";

    }

    public class OsuPlayerData
    {
        [JsonProperty("user_id")]
        public int userId { get; set; }

        [JsonProperty("username")]
        public string userName { get; set; }

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
    public class Event
    {
        [JsonProperty("display_html")]
        public string displayHtml { get; set; }

        [JsonProperty("beatmap_id")]
        public int beatmapId { get; set; }

        [JsonProperty("beatmapset_id")]
        public int beatmapsetId { get; set; }

        [JsonProperty("date")]
        public DateTime date { get; set; }

        [JsonProperty("epicfactor")]
        public int epicFactor { get; set; }
    }
}
