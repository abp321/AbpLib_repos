using System;
using System.Text;

namespace AbpLib.Cryptography
{
    public static class Encode
    {
        public static byte[] ToBytes(this string s) => Encoding.UTF8.GetBytes(s);

        public static string To64(this string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }

        public static string ToUTF8(this string s)
        {
            byte[] bytes = Encoding.Default.GetBytes(s);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
