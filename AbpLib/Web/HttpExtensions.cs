using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace AbpLib.Web
{
    public static class HttpExtensions
    {
        public static string GetIP(this HttpRequest r) => r.HttpContext.Connection.RemoteIpAddress.ToString();

        public static string SERIALIZE<T>(this T m)
        {
            JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Serialize(m, options);
        }

        public static async Task<T> DESERIALIZE<T>(this HttpClient client, string url)
        {
            JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            using HttpResponseMessage msg = await client.GetAsync(url);
            string s = await msg.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(s, options);
        }

        public static T DESERIALIZE<T>(this string s)
        {
            JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(s, options);
        }

        public static async Task<T> DeserializeBody<T>(this HttpRequest r)
        {
            string body = await r.GetBodyContent();
            return body.DESERIALIZE<T>();
        }

        public static StringContent SERIALIZE_CONTENT<T>(this T m, MediaTypes mediaType = MediaTypes.JSON)
        {
            return new StringContent(m.SERIALIZE(), Encoding.UTF8, mediaType.GetMediaType());
        }

        public static async Task<T[]> DeserializeStream<T>(this HttpClient client, string url)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                IgnoreReadOnlyProperties = true,
                IgnoreNullValues = true
            };
            using HttpResponseMessage msg = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            using Stream stream = await msg.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T[]>(stream, options);
        }

        public static async Task<T[]> DeserializeStream<T>(this HttpResponseMessage msg)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                IgnoreReadOnlyProperties = true,
                IgnoreNullValues = true
            };
            using Stream stream = await msg.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T[]>(stream, options);
        }

        public static async Task<string> GetBodyContent(this HttpRequest r, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;

            using StreamReader reader = new StreamReader(r.Body, encoding);
            string result = await reader.ReadToEndAsync();

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");//This version can handle danish letters
            byte[] utfBytes = encoding.GetBytes(result);
            byte[] isoBytes = Encoding.Convert(encoding, iso, utfBytes);
            return iso.GetString(isoBytes);
        }

        public static string GetMediaType(this MediaTypes type)
        {
            return type switch
            {
                MediaTypes.PlainText => "text/plain",
                MediaTypes.OctetStream => "application/octet-stream", 
                MediaTypes.UrlEncoded => "application/x-www-form-urlencoded",
                MediaTypes.JSON => "application/json",
                _ => "",
            };
        }
    }
}
