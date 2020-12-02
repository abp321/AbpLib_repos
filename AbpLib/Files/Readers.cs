using System.IO;
using System.Threading;

namespace AbpLib.Files
{
    public static class Readers
    {
        private static readonly EventWaitHandle waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "SHARED_BY_ALL_PROCESSES");
        public static string ReadFromFile(this string path, FileMode mode = FileMode.OpenOrCreate)
        {
            waitHandle.WaitOne();
            if (!File.Exists(path))
            {
                File.Create(path);
                waitHandle.Set();
                return "";
            }

            using FileStream fs = new FileStream(path, mode, FileAccess.Read);
            using StreamReader sr = new StreamReader(fs);

            waitHandle.Set();
            return sr.ReadToEnd();
        }
    }
}
