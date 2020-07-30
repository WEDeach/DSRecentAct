using DSRecentAct.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSRecentAct.Model.OsuApiModel;

namespace DSRecentAct.Model
{
    class ReflectorModel
    {
        public static OsuRTDataProvider.OsuRTDataProviderPlugin ORTDP;

        public static Sync.Tools.PluginConfigurationManager config_manager;
        public static OsuListener OsuListener;

        public static DSRASetting Setting = new DSRASetting();

        public static OsuPlayer OPD = new OsuPlayer();

        public static OsuApiModel OsuApi;
        public static MmfModel mmf = new MmfModel();


        public static CustomData CustomData = new CustomData();
    }
    class CustomData
    {
        public long currentMapTotalScore { get; set; } = 0;
        public long currentMapBestScore { get; set; } = 0;

        public double? currentMapPp { get; set; } = 0;
        public double? currentMapBestPp { get; set; } = 0;
        public int currentMapAcc { get; set; } = 0;
        public int currentMapBestAcc { get; set; } = 0;
        public int currentMapCombo { get; set; } = 0;
        public int currentMapBestCombo { get; set; } = 0;

    }
}
