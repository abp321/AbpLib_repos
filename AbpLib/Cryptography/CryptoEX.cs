using System.Security.Cryptography;
using System.Text;

namespace AbpLib.Cryptography
{
    public static class CryptoEX
    {
        public static string ToHMACSHA384Hash(this string token, string key)
        {
            HMACSHA384 sha384Hash = new HMACSHA384(Encoding.Default.GetBytes(key));
            byte[] bytes = sha384Hash.ComputeHash(Encoding.UTF8.GetBytes(token));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));//x2 = format to hex
            }
            return sb.ToString();
        }
    }
}
