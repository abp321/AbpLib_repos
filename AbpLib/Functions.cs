using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AbpLib
{
    public static class Functions
    {
        public static int RN(this int max, int min = 0) => new Random().Next(min, max);
        public static double PercentageOf(this double x, double y) => (x / y * 100).RoundUp();

        public static double RNDOUBLE(this int max, int min = 0)
        {
            Random r = new Random();
            double rf = r.NextDouble();
            return rf + max.RN(min);
        }

        public static float RNFLOAT(this int max, int min = 0)
        {
            return (float)max.RNDOUBLE(min);
        }

        public static string PropertyView<T>(this T obj, params string[] exceptionFields)
        {
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] props = obj.GetType().GetProperties().Where(x => !x.Name.ContainsAny(exceptionFields)).ToArray();
            for (int i = 0; i < props.Length; i++)
            {
                object value = props[i].GetValue(obj, null) ?? "NULL";
                string name = props[i].Name;
                sb.AppendLine($"{name}: {value}");
            }
            return sb.ToString();
        }

        public static string DictionaryView(this Dictionary<string, object> dict)
        {
            StringBuilder sb = new StringBuilder();
            foreach(KeyValuePair<string, object> n in dict)
            {
                sb.AppendLine($"Name: {n.Key} Value: {n.Value} Type: {n.Value.GetType().Name}");
            }

            return sb.ToString();
        }

        public static string DictionaryView(this IReadOnlyDictionary<string, object> dict)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> n in dict)
            {
                sb.AppendLine($"Name: {n.Key} Value: {n.Value} Type: {n.Value?.GetType().Name}");
            }

            return sb.ToString();
        }

        public static decimal RoundUp(this decimal i, double decimalPlaces = 2)
        {
            decimal power = (decimal)Math.Pow(10, decimalPlaces);
            return Math.Ceiling(i * power) / power;
        }

        public static double RoundUp(this double i, double decimalPlaces = 2)
        {
            double power = Math.Pow(10, decimalPlaces);
            return Math.Ceiling(i * power) / power;
        }

        public static float RoundUp(this float i, double decimalPlaces = 2)
        {
            double power = Math.Pow(10, decimalPlaces);
            return (float)(Math.Ceiling(i * power) / power);
        }

        public static string GetValue(this List<string> list, string find)
        {
            if (string.IsNullOrEmpty(find)) return "";

            bool canFind = list.ToArray().ContainsAny(find);
            return canFind ? list[list.FindIndex(x => x.Equals(find, StringComparison.OrdinalIgnoreCase))] : "";
        }

        public static string GetValue(this string[] list, string find)
        {
            if (string.IsNullOrEmpty(find)) return "";

            bool canFind = list.ContainsAny(find);
            List<string> newList = new List<string>(list);

            return canFind ? newList[newList.FindIndex(x => x.Equals(find, StringComparison.OrdinalIgnoreCase))] : "";
        }

        //Generics
        public static void AddUnique<T>(this List<T> list, params T[] values)
        {
            for (int i = 0; i < values.Length; i++) if (!list.Contains(values[i])) list.Add(values[i]);
        }

        public static T GetChangedProperties<T>(this T A, T B)
        {
            T C = Activator.CreateInstance<T>();
            if (A != null && B != null)
            {
                PropertyInfo[] prop1 = A.GetType().GetProperties();
                PropertyInfo[] prop2 = B.GetType().GetProperties();

                foreach (PropertyInfo p in prop1)
                {
                    foreach (var n in prop2.Where(s => s.GetValue(B) != p.GetValue(A) && s.Name == p.Name))
                    {
                        C.GetType().GetProperty(p.Name).SetValue(C, p.GetValue(A));
                    }
                }
            }
            return C;
        }

        //Streams
        public static async Task<byte[]> GetBytes(this Stream s)
        {
            using MemoryStream mem = new MemoryStream();
            await s.CopyToAsync(mem);
            return mem.ToArray();
        }
    }

    //Enums
    public enum MediaTypes
    {
        PlainText = 0, OctetStream = 1, UrlEncoded = 2, JSON = 3
    }

    public enum SizeTypes
    {
        Bytes = 0, KiloBytes = 1, MegaBytes = 2, GigaBytes = 3
    }
}
