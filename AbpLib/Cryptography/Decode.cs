using System;
using System.Text;

namespace AbpLib.Cryptography
{
    public static class Decode
    {
        public static string BytesToString(this byte[] ba) => Encoding.UTF8.GetString(ba);
        public static byte[] StringToBytes(this string s) => Encoding.UTF8.GetBytes(s);

        public static string From64(this string s)
        {
            byte[] bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string HexToString(this string s)
        {
            s = s.Replace("-", "");
            byte[] bytes = new byte[s.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
            }
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
