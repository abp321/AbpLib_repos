using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AbpLib.Threads
{
	public static class ThreadEX
	{
		public static void StartThread(this Action a)
		{
			Thread t = new Thread(a.Invoke) { IsBackground = true }; t.Start();
		}

		public static void StartThread<T>(this Action<T> a, T v1)
		{
			Thread t = new Thread(()=>a.Invoke(v1)) { IsBackground = true }; t.Start();
		}

		public static void TaskThreadStart(this Task task)
		{
			Thread t = new Thread(async () => await task) { IsBackground = true }; t.Start();
		}
	}
}
