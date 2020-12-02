using System;
using System.Collections.Generic;
using System.Linq;

namespace AbpLib
{
    public static class BOOL
    {
        public static bool ContainsChar(this string s, char ch = ' ') => s.All(c => c.Equals(ch));
        public static bool IsOnlyLetters(this string s) => s.All(char.IsLetter);
        public static bool IsWS(this string s) => s.All(char.IsWhiteSpace);
        public static bool IsEveryOther(this int n, bool first = true) => n % 2 == (first ? 0 : 1);
        public static bool ContainsAll(this string[] array, Dictionary<string,string> d) => array.All(x => d.Values.Contains(x));
        public static bool NameEquals(this string name, string obj) => name.ToUpper() == obj.ToUpper();
        public static bool IsNull(this string s) => string.IsNullOrEmpty(s);
        public static bool JsonIsValid(this string s) => (s.StartsWith("{") && s.EndsWith("}")) || (s.StartsWith("[") && s.EndsWith("]"));

        public static bool ContainsAny(this string s, params string[] values)
        {
            if (values == null || s == null) return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (s.Contains(values[i], StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool ContainsAny(this string[] values, string s)
        {
            if (values == null || s == null) return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (s.Contains(values[i], StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool ContainsAny(this string s, params char[] values)
        {
            if (values == null || s == null) return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (s.Contains(values[i], StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool EqualsAny(this char s, params char[] values)
        {
            if (values == null || s == default) return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == s) return true;
            }
            return false;
        }

        public static bool EqualsAny(this string s, params string[] values)
        {
            if (values == null || s == null) return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (string.Equals(s, values[i], StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool StartsWithAny(this string s, params string[] values)
        {
            if (values == null || s == null) return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (s.StartsWith(values[i], StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool IsNullOrDefault<T>(this T obj)
        {
            if (obj == null) return true;
            if (Equals(obj, default(T))) return true;

            Type methodType = typeof(T);
            if (Nullable.GetUnderlyingType(methodType) != null) return false;

            Type argumentType = obj.GetType();
            if (argumentType.IsValueType && argumentType != methodType)
            {
                object value = Activator.CreateInstance(obj.GetType());
                return value.Equals(obj);
            }

            return false;
        }
    }
}
