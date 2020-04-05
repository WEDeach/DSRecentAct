using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSRecentAct.Memory;
using DSRecentAct.Model;
using DSRecentAct.utils;

namespace DSRecentAct.Listener
{
    public class OsuListener
    {
        public WorkerModel OsuWorker;

        public enum OsuStatus : int
        {
            Unkonwn = -1,
            NoFoundProcess = 0,
            Playing = 1,
            SelectSong = 3,
            Editing = 4,
            MatchSetup = 7,
            Rank = 10,
            Lobby = 15,
            Idle = 20,
        }

        public OsuListener()
        {
            OsuWorker = new WorkerModel();
        }

    }
}
