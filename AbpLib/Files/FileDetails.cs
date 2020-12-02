using System.IO;

namespace AbpLib.Files
{
    public static class FileDetails
    {
        public static decimal FileSize(this string path, SizeTypes type = SizeTypes.Bytes, double round = 2)
        {
            if (!File.Exists(path)) return 0;

            FileInfo inf = new FileInfo(path);
            decimal l = inf.Length;
            return type switch
            {
                SizeTypes.Bytes => l,
                SizeTypes.KiloBytes => (l / 1024).RoundUp(round),
                SizeTypes.MegaBytes => (l / 1000 / 1024).RoundUp(round),
                SizeTypes.GigaBytes => (l / 1000 / 1000 / 1024).RoundUp(round),
                _ => 0,
            };
        }

        public static string FileSizeText(this string path, SizeTypes type = SizeTypes.Bytes)
        {
            string SizeTypeID = string.Empty;
            switch (type)
            {
                case SizeTypes.Bytes:
                    SizeTypeID = "Bytes";
                    break;
                case SizeTypes.KiloBytes:
                    SizeTypeID = "KB";
                    break;
                case SizeTypes.MegaBytes:
                    SizeTypeID = "MB";
                    break;
                case SizeTypes.GigaBytes:
                    SizeTypeID = "GB";
                    break;
            }

            return $"Size: {path.FileSize(type)} {SizeTypeID}";
        }
    }
}
