using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AbpLib
{
    public static class STRING
    {
        public static string TrimSpaces(this string s) => string.Join(" ", s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        public static string LastWord(this string s) => s.TrimEnd().Split(' ').LastOrDefault();
        public static string WordAfter(this string s, string Word, char c = ' ') => s[(s.IndexOf(Word) + Word.Length)..].Split(c)[1];
        public static string RemoveChar(this string s, char ch = ' ') => new string(s.Where(c => !c.Equals(ch)).ToArray());
        public static string RemoveChars(this string s, params char[] ch) => new string(s.Where(c => !c.EqualsAny(ch)).ToArray());
        public static string OnlyLetters(this string s) => new string(s.Where(c => char.IsLetter(c)).ToArray());
        public static string ToSqlDate(this DateTime date, bool dateOnly = false) => date.ToString(dateOnly ? "yyyy-MM-dd": "yyyy-MM-dd HH:mm:ss.fff");
        public static string[][] DoubleSplit(this string s, char sp1, char sp2) => s.Split(sp1).Select(p => p.Split(sp2)).ToArray();
        
        public static string AsProperName(this string s)
        {
            if (s == null) return null;

            return (s.Length > 1) ? char.ToUpper(s[0]) + s[1..] : s.ToUpper();
        }

        public static string CutFront(this string s, Func<char, bool> f)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (f.Invoke(s[i])) return s[i..];
            }
            return s;
        }

        public static string CutEnd(this string s, Func<char, bool> f)
        {
            for (int i = s.Length; i-- > 0;)
            {
                if (f.Invoke(s[i])) return s.Remove(i + 1);
            }
            return s;
        }

        public static string GetBetween(this string s, string strStart, string strEnd)
        {
            int Start = s.IndexOf(strStart, 0) + strStart.Length;
            int End = s.IndexOf(strEnd, Start);
            return (s.Contains(strStart) && s.Contains(strEnd)) ? s[Start..End] : "";
        }

        public static string ReplaceChars(this string s, char Replacement, params char[] ch)
        {
            for (int i = 0; i < ch.Length; i++)
            {
                s = s.Replace(ch[i], Replacement);
            }
            return s;
        }

        public static string WordAfterTrim(this string s, string Word, char c = ' ')
        {
            s = s.TrimSpaces().ToLower();
            return s.WordAfter(Word.ToLower(), c);
        }

        //Stringbuilder
        public static StringBuilder AppendRandom(this StringBuilder sb, int size = 5)
        {
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                char ch = char.ToLower(Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))));
                sb.Append(ch);
            }
            return sb;
        }

        public static StringBuilder RemoveLast(this StringBuilder sb, string newInput = "")
        {
            if (sb == null || sb.Length == 0) return sb;

            sb.Remove(sb.Length - 1, 1);
            if (newInput != "") sb.Append(newInput);

            return sb;
        }

        //JSON
        public static string CleanASCII(this string s)
        {
            StringBuilder sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (c > 127)
                    continue;
                if (c < 32)  // Control characters 
                    continue;
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static string JsonBeautify(this string json)
        {
            const int indentWidth = 4;
            const string pattern = "(?>([{\\[][}\\]],?)|([{\\[])|([}\\]],?)|([^{}:]+:)([^{}\\[\\],]*(?>([{\\[])|,)?)|([^{}\\[\\],]+,?))";

            Match match = Regex.Match(json, pattern);
            StringBuilder beautified = new StringBuilder();
            int indent = 0;
            while (match.Success)
            {
                if (match.Groups[3].Length > 0) indent--;

                string space = new string(' ', indent * indentWidth);
                beautified.AppendLine($"{space} {(match.Groups[4].Length > 0 ? match.Groups[4].Value + " " + match.Groups[5].Value : (match.Groups[7].Length > 0 ? match.Groups[7].Value : match.Value))}");

                if (match.Groups[2].Length > 0 || match.Groups[6].Length > 0) indent++;

                match = match.NextMatch();
            }
            return beautified.ToString();
        }
    }
}
