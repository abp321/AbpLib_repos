using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbpLib.Files
{
    public static class ZIP
    {
        public static bool ZipExists = false;

        public static IEnumerable<string> ReadLinesZIP(this string path, string filetype = ".txt")
        {
            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using BufferedStream BufferStream = new BufferedStream(stream, 128 * 2048);
            using ZipArchive archive = new ZipArchive(BufferStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                if (entry.Name.EndsWith(filetype))
                {
                    using BufferedStream BufferZip = new BufferedStream(entry.Open(), 128 * 2048);
                    using StreamReader streamReader = new StreamReader(BufferZip);
                    while (!streamReader.EndOfStream) yield return streamReader.ReadLine();
                }
            }
        }

        public static void FileToZip(this string path, string path2, bool replacetxt = false, string zipName = "dataZip.zip")
        {
            string ZF = $"{path}/{zipName}";
            try
            {
                using ZipArchive archive = ZipFile.Open(ZF, ZipArchiveMode.Update);
                if (!ZipExists) ZipExists = archive.ZipHasFile("data.txt", replacetxt);

                ZipArchiveEntry entry = ZipExists ? archive.GetEntry("data.txt") : archive.CreateEntry("data.txt");
                using Stream stream = entry.Open();
                using BufferedStream BufferZip = new BufferedStream(stream, 128 * 2048);
                using BinaryWriter writer = new BinaryWriter(BufferZip);
                writer.Write(File.ReadAllBytes(path2));
                writer.Flush();
            }
            catch (IOException)
            {
            }
        }

        public static async Task<string> WriteZipLines(this string path, string[] content, bool replacetxt = false, string zipName = "dataZip.zip")
        {
            StringBuilder sb = new StringBuilder("Bytes added to File: ");
            string ZF = $"{path}/{zipName}";
            decimal StartLength = ZF.FileSize(SizeTypes.Bytes);
            try
            {
                using ZipArchive archive = ZipFile.Open(ZF, ZipArchiveMode.Update);
                if (!ZipExists) ZipExists = archive.ZipHasFile("data.txt", replacetxt);

                ZipArchiveEntry entry = ZipExists ? archive.GetEntry("data.txt") : archive.CreateEntry("data.txt");
                using Stream stream = entry.Open();
                using BufferedStream BufferZip = new BufferedStream(stream, 128 * 2048);
                StreamWriter writer = new StreamWriter(BufferZip);
                using StreamReader reader = new StreamReader(BufferZip);

                writer.BaseStream.Seek(0, SeekOrigin.End);
                for (int i = 0; i < content.Length; i++)
                {
                    string val = content[i];
                    await writer.WriteLineAsync(val);
                    await writer.FlushAsync();
                }

            }
            catch (IOException)
            {
            }
            decimal TotalSize = ZF.FileSize(SizeTypes.Bytes);
            decimal NewLength = TotalSize - StartLength;
            string result = $"{NewLength}, Total Size: {DetermineSize(TotalSize)}";
            sb.Append(result);
            return sb.ToString();
        }

        public static async Task<string> WriteZipLinesUnique(this string path, string[] content, bool replacetxt = false, string zipName = "dataZip.zip")
        {
            StringBuilder sb = new StringBuilder("Bytes added to File: ");
            string ZF = $"{path}/{zipName}";
            decimal StartLength = ZF.FileSize(SizeTypes.Bytes);
            try
            {
                using ZipArchive archive = ZipFile.Open($"{path}/dataZip.zip", ZipArchiveMode.Update);
                if (!ZipExists) ZipExists = archive.ZipHasFile("data.txt", replacetxt);

                content = content.Select(s => s.ToLower()).ToArray();
                ZipArchiveEntry entry = ZipExists ? archive.GetEntry("data.txt") : archive.CreateEntry("data.txt");
                using Stream stream = entry.Open();
                using BufferedStream BufferZip = new BufferedStream(stream, 128 * 2048);
                StreamWriter writer = new StreamWriter(BufferZip);
                using StreamReader reader = new StreamReader(BufferZip);
                string x = await reader.ReadToEndAsync();
                string[] split = x.Split(Environment.NewLine);
                split = split.Select(s => s.ToLower()).ToArray();
                string[] arr = content.Except(split).ToArray();

                writer.BaseStream.Seek(0, SeekOrigin.End);
                for (int i = 0; i < arr.Length; i++)
                {
                    string val = arr[i];
                    if (!string.IsNullOrEmpty(val) && !string.IsNullOrWhiteSpace(val) && val.IsOnlyLetters())
                    {
                        await writer.WriteLineAsync(val);
                        await writer.FlushAsync();
                    }
                }
            }
            catch (IOException)
            {
            }
            decimal TotalSize = ZF.FileSize(SizeTypes.Bytes);
            decimal NewLength = TotalSize - StartLength;
            string result = $"{NewLength}, Total Size: {DetermineSize(TotalSize)}";
            sb.Append(result);
            return sb.ToString();
        }

        private static string DetermineSize(decimal size)
        {
            string[] sizes = new string[] { $"{size}", $"{(size / 1024).RoundUp(4)} kB", $"{(size / 1000 / 1024).RoundUp(4)} mB" };
            int index;
            if (size < 10000) index = 0;
            else if (size >= 10000 && size <= 100000) index = 1;
            else index = 2;

            return sizes[index];
        }

        private static bool ZipHasFile(this ZipArchive archive, string fileFullName, bool del)
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (del)
                {
                    entry.Delete();
                    return false;
                }
                if (entry.FullName.EndsWith(fileFullName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetZIPData(this string file, bool sort = false, bool seperateLines = false, string filetype = ".txt")
        {
            string[] a = file.ReadLinesZIP(filetype).ToArray();
            if (sort) Array.Sort(a);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < a.Length; i++)
            {
                if (seperateLines) sb.Append(a[i]).AppendLine();
                else sb.Append(a[i]);
            }
            return sb.ToString();
        }
    }
}
