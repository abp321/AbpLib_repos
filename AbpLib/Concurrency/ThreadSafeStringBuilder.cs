using System;
using System.Text;
using System.Threading;

namespace AbpLib.Concurrency
{
    public class ThreadSafeStringBuilder
    {
        private readonly StringBuilder core = new StringBuilder();
        private readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim();

        public void Append<T>(T s)
        {
            try
            {
                sync.EnterWriteLock();
                core.Append(s);
            }
            finally
            {
                sync.ExitWriteLock();
            }
        }

        public void AppendLine<T>(T s)
        {
            try
            {
                sync.EnterWriteLock();
                core.Append(s).AppendLine();
            }
            finally
            {
                sync.ExitWriteLock();
            }
        }

        public void Clear()
        {
            try
            {
                sync.EnterWriteLock();
                core.Clear();
            }
            finally
            {
                sync.ExitWriteLock();
            }
        }

        public override string ToString()
        {
            try
            {
                sync.EnterReadLock();
                return core.ToString();
            }
            catch (Exception)
            {
                return "";
            }
            finally
            {
                sync.ExitReadLock();
            }
        }

        public int Length()
        {
            int length;
            try
            {
                sync.EnterWriteLock();
                length = core.Length;
            }
            finally
            {
                sync.ExitWriteLock();
            }
            return length;
        }

        public int ToInt()
        {
            int n;
            try
            {
                sync.EnterWriteLock();
                n = Convert.ToInt32(core.ToString());
            }
            finally
            {
                sync.ExitWriteLock();
            }
            return n;
        }
    }
}
