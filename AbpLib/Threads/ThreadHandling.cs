using System;
using System.Threading;

namespace AbpLib.Threads
{
    public class ThreadHandling
    {
        public static bool MainThreadStarted { get; set; }
        public ManualResetEvent MRSE = new ManualResetEvent(false);

        private void Pause() => MRSE.Reset();
        private void Resume() => MRSE.Set();

        public void ThreadStart(Action method, bool start = false)
        {
            if (!MainThreadStarted && start)
            {
                Thread statusthread = new Thread(method.Invoke) { IsBackground = true };
                statusthread.Start();
                MainThreadStarted = true;
            }

            Action a = start ? new Action(Resume) : new Action(Pause);

            Thread thread = new Thread(a.Invoke) { IsBackground = true };
            thread.Start();
        }
    }
}
