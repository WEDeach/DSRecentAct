using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSRecentAct.Model
{
    class ReflectorModel
    {
        public static OsuRTDataProvider.OsuRTDataProviderPlugin ORTDP;
        public static OsuApiModel OsuApi;
        public static Sync.Tools.PluginConfigurationManager config_manager;
        public static DSRASetting Setting = new DSRASetting();

        public static List<OsuPlayer> OPD = new List<OsuPlayer>();

        public static MmfModel mmf = new MmfModel();
    }
}
