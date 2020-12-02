using AbpLib.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AbpLib.Files
{
    public static class Binary
    {
        private const int bufferSize = 4096;

        public static string WriteFile(this byte[] bytes, string path)
        {
            try
            {
                using BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Truncate & FileMode.Append));
                writer.Write(bytes);
                return path.FileSizeText(SizeTypes.KiloBytes);
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        public static bool TryWriteFile(this IEnumerable<byte[]> ByteArrays, string path, out string result)
        {
            try
            {
                using BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Truncate & FileMode.Append));
                foreach (byte[] ba in ByteArrays)
                {
                    writer.Write(ba);
                }
            }
            catch (Exception err)
            {
                result = err.Message;
                return false;
            }
            result = path.FileSizeText(SizeTypes.KiloBytes);
            return true;
        }

        public static IEnumerable<byte[]> FileBytes(this string path)
        {
            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using BinaryReader reader = new BinaryReader(stream, Encoding.Default);
            byte[] buffer = new byte[bufferSize];
            int count;
            if (reader.BaseStream.CanRead)
            {
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                {
                    yield return buffer;
                }
            }
        }

        public static bool TryReadBytes(this string path, out byte[] bytes)
        {
            bool IsReadable;
            try
            {
                List<byte> ByteList = new List<byte>();
                foreach (byte[] ba in path.FileBytes())
                {
                    ByteList.AddRange(ba); 
                }
                if(ByteList.Count == 0)
                {
                    string msg = "File is empty";
                    bytes = msg.ToBytes();
                    IsReadable = false;
                }
                else
                {
                    bytes = ByteList.ToArray();
                    IsReadable = true;
                }
            }
            catch (IOException err)
            {
                bytes = err.Message.ToBytes();
                IsReadable = false;
            }
            return IsReadable;
        }
    }
}
