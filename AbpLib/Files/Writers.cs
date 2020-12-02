using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AbpLib.Files
{
    public static class Writers
    {
        private static readonly EventWaitHandle waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "SHARED_BY_ALL_PROCESSES");

        public static void WriteToFile(this string path, string content, bool append = false)
        {
            waitHandle.WaitOne();
            StreamWriter sw = new StreamWriter(path, append);
            try
            {
                sw.WriteLine(content);
            }
            finally
            {
                sw.Dispose();
                waitHandle.Set();
            }
        }

        public static async Task WriteToFileAsync(this string path, string content, bool WithCheck = false)
        {
            StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8, 8192);

            if (WithCheck && sw.BaseStream.CanRead)
            {
                StreamReader sr = new StreamReader(path);
                if (await sr.ReadToEndAsync() == content)
                {
                    await sw.DisposeAsync();
                    sr.Dispose();
                    return;
                }
            }
            await sw.WriteAsync(content);
            await sw.DisposeAsync();
        }

        public static void AppendToFile(this string path, string content, FileMode mode = FileMode.Append)
        {
            FileStream fs = new FileStream(path, mode, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(content);
            fs.Dispose();
        }

        public static async Task AppendToFileAsync(this string path, string content, FileMode mode = FileMode.Append)
        {
            using FileStream fs = new FileStream(path, mode, FileAccess.Write);
            using StreamWriter sw = new StreamWriter(fs);
            await sw.WriteLineAsync(content);
        }
    }
}
