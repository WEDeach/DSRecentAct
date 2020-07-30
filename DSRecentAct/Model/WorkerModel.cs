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
