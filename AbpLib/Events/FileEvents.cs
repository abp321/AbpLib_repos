using System;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace AbpLib.Events
{
    public class FileEvents
    {
        private static readonly EventWaitHandle waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "SHARED_BY_ALL_PROCESSES");
        
        private bool IncludeSubs = true;
        private readonly bool exists;
        private readonly string p;
        private readonly FileSystemWatcher watcher;
        private readonly NotifyFilters DefaultFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.CreationTime;

        private readonly Action<FileSystemEventArgs> OnChangedAction;
        private readonly Action<RenamedEventArgs> OnRenamedAction;
        private readonly Action<ErrorEventArgs> OnErrorsAction;

        public FileEvents(string path, Action<FileSystemEventArgs> OnChange, Action<RenamedEventArgs> OnRename = null, Action<ErrorEventArgs> OnError = null)
        {
            exists = Directory.Exists(path);
            if (exists)
            {
                p = path;
                watcher = new FileSystemWatcher
                {
                    Path = p,
                    NotifyFilter = DefaultFilter
                };
                OnChangedAction = OnChange;
                OnRenamedAction = OnRename;
                OnErrorsAction = OnError;
            }
        }

        public void Start()
        {
            if (exists)
            {
                INIT();
            }
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
        }

        public void SetBuffer(int buffer)
        {
            watcher.InternalBufferSize = buffer;
        }

        public void SetFilter(NotifyFilters NewFilter)
        {
            watcher.NotifyFilter = NewFilter;
        }

        public void IncludeSubfolders(bool include)
        {
            IncludeSubs = include;
        }

        private void INIT()
        {
            watcher.Changed += OnChange;
            watcher.Created += OnChange;
            watcher.Deleted += OnChange;

            if (OnRenamedAction != null)
            {
                watcher.Renamed += OnRename;
            }

            if (OnErrorsAction != null)
            {
                watcher.Error += OnError;
            }

            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = IncludeSubs;
        }

        private void OnChange(object sender, FileSystemEventArgs e)
        {
            try
            {
                waitHandle.WaitOne();
                OnChangedAction?.Invoke(e);
                watcher.EnableRaisingEvents = false;
            }
            finally
            {
                waitHandle.Set();
                watcher.EnableRaisingEvents = true;
            }
        }

        private void OnRename(object sender, RenamedEventArgs e)
        {
            try
            {
                waitHandle.WaitOne();
                OnRenamedAction?.Invoke(e);
                watcher.EnableRaisingEvents = false;
            }
            finally
            {
                waitHandle.Set();
                watcher.EnableRaisingEvents = true;
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            try
            {
                waitHandle.WaitOne();
                OnErrorsAction?.Invoke(e);
                watcher.EnableRaisingEvents = false;
            }
            finally
            {
                waitHandle.Set();
                watcher.EnableRaisingEvents = true;
            }
        }
    }
}
