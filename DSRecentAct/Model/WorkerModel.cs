using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSRecentAct.Listener;
using DSRecentAct.utils;

namespace DSRecentAct.Model
{
    public class WorkerModel
    {
        public OsuListener m_OsuListener;

        private int OSU_PID = 0;
        public Process OSU_PROCESS = null;
        
        private long FIND_OSU_PROCESS_TIMER = 0;
        private const long FIND_OSU_RETRY_INTERVAL = 2000;

        static private Task WORKER_TASK;
        static private List<Action> WORKER_TASKS = new List<Action>();
        static private bool STOP_FLAG = false;

        public WorkerModel()
        {
            STOP_FLAG = false;

            WORKER_TASK = Task.Run(() =>
            {
                Thread.CurrentThread.Name = "DSRAThread";
                Thread.Sleep(2000);
                while (!STOP_FLAG)
                {
                    for (int i = 0; i < WORKER_TASKS.Count; i++)
                    {
                        var action = WORKER_TASKS[i];
                        action();
                    }

                    //Thread.Sleep();
                }
            });
            AddWork(FindOsuProcess);
        }
        private void FindOsuProcess()
        {
            if (OSU_PROCESS == null && FIND_OSU_PROCESS_TIMER > FIND_OSU_RETRY_INTERVAL)
            {
                FIND_OSU_PROCESS_TIMER = 0;
                Process[] process_list;

                process_list = Process.GetProcessesByName("osu!");

                if (STOP_FLAG) return;
                if (process_list.Length != 0)
                {
                    OSU_PROCESS = process_list[0];

                    if (OSU_PROCESS != null)
                    {
                        Logger.LogInfomation(string.Format("找到OSU! ({0})", OSU_PROCESS.Id));
                        OsuRTDataProvider.Memory.SigScan _sigScan = new OsuRTDataProvider.Memory.SigScan(OSU_PROCESS);
                        //IntPtr pAddr = _sigScan.FindPattern(new byte[]{ 0x88, 0xc3, 0xa2, 0x00, 0x00, 0x00, 0x00, 0x03, 0x24 }, "x????xx", 174);
                        IntPtr pAddr = _sigScan.FindPattern(Memory.SigScan.StringToByte("\x80\xb8\x0\x0\x0\x0\x0\x75\x19\xa1\x0\x0\x0\x0\x83\xf8\x0b\x74\x0b"), "xx????xxxx????xxxxx", 10);
                        Logger.LogInfomation($"Game Status Address (0):0x{(int)pAddr:X8}");
                        return;
                    }
                }
                FIND_OSU_PROCESS_TIMER = 0;
                Logger.Error("找不到OSU!");
            }
            FIND_OSU_PROCESS_TIMER += 1000;
        }

        public void Stop()
        {
            STOP_FLAG = true;
            WORKER_TASK.Wait();
        }


        public void AddWork(Action action)
        {
            WORKER_TASKS.Add(action);
        }
    }
}
