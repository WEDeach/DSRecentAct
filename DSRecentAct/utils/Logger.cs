using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSRecentAct.utils
{
    class Logger
    {
        public static Logger<DSRecentAct> logger = new Logger<DSRecentAct>();

        public static void LogInfomation(string msg) => logger.LogInfomation(msg);
        public static void WriteColor(string msg, ConsoleColor color) => IO.CurrentIO.WriteColor(msg, color);
        public static void Error(string message) => logger.LogError(message);
    }
}
