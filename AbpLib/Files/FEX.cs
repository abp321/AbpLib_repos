using System.IO;
using System.Linq;

namespace AbpLib.Files
{
    public static class BackUpFiles
    {
        public static void CopyFiles(this string SourcePath, string DestinationPath, string LookFor = "*.cs")
        {
            CreateDirs(SourcePath, DestinationPath);
            CopyDirFiles(SourcePath, DestinationPath, LookFor);
            TrimFolders(DestinationPath, LookFor);
        }

        private static void CreateDirs(string SourcePath, string DestinationPath)
        {
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
            }
        }

        private static void CopyDirFiles(string SourcePath, string DestinationPath, string LookFor)
        {
            foreach (string newPath in Directory.GetFiles(SourcePath, LookFor, SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
            }
        }

        private static bool TrimFolders(string startLocation, string pattern)
        {
            bool result = true;
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                bool directoryResult = TrimFolders(directory, pattern);
                result &= directoryResult;

                if (Directory.GetFiles(directory, pattern).Any())
                {
                    result = false;
                    continue;
                }

                foreach (var file in Directory.GetFiles(directory))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        // error handling
                        result = directoryResult = false;
                    }
                }

                if (!directoryResult) continue;
                try
                {
                    Directory.Delete(directory, false);
                }
                catch (IOException)
                {
                    // error handling
                    result = false;
                }
            }

            return result;
        }
    }
}
