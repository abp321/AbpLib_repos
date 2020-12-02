using System;
using System.Collections.Generic;
using System.Reflection;

namespace AbpLib
{
    public static class GENERICS
    {
        public static void SetDefaultValues<T>(this T m)
        {
            PropertyInfo[] props = m.GetType().GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                props[i].SetValue(m, props[i].PropertyType.GetDefaultObject());
            }
        }

        private static object GetDefaultObject(this Type t)
        {
            return t switch
            {
                Type _ when t == typeof(string) => string.Empty,
                Type _ when t == typeof(object) => new object(),
                Type _ when t == typeof(string[]) => new string[0],
                Type _ when t == typeof(List<string>) => new List<string>(),
                Type _ when t == typeof(Dictionary<string, string>) => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                Type _ when t == typeof(Dictionary<string, object>) => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
                _ => default,
            };
        }
    }
}
