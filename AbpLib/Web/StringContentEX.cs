using System.Net.Http;
using System.Text;

namespace AbpLib.Web
{
    public static class StringContentEX
    {
        //Encode to stringcontent - json
        public static string EncodeToStringContent(this string s,bool removeSpace = false)
        {
            bool isMultipleFields = s.Contains(',');
            string FormatedText = s.FormatFields(isMultipleFields);
            string result = removeSpace ? FormatedText.RemoveChar() : FormatedText;
            return result;
        }

        //Convert to stringcontent
        public static StringContent ToStringContent(this string s, bool removeSpace = true)
        {
            bool isMultipleFields = s.Contains(',');
            string FormatedText = s.FormatFields(isMultipleFields);
            string result = removeSpace ? FormatedText.RemoveChar() : FormatedText;
            return new StringContent(result, Encoding.UTF8, "application/json");
        }

        public static StringContent NewContent(this StringContent con, string value)
        {
            return new StringContent(con.AddContentString(value),Encoding.UTF8, "application/json");
        }

        public static string AddContentString(this StringContent con, string value)
        {
            string OldValue = con.ReadAsStringAsync().Result;
            StringBuilder sb = new StringBuilder(OldValue);
            sb.Remove(sb.Length - 1, 1);//cut end
            sb.Append(',');

            sb.Append(value[1..]);//cut start
            return sb.ToString();
        }

        public static string AddContentString(this string OldValue, string value)
        {
            StringBuilder sb = new StringBuilder(OldValue);
            sb.Remove(sb.Length - 1, 1);//cut end
            sb.Append(',');

            sb.Append(value[1..]);//cut start
            return sb.ToString();
        }

        private static string FormatFields(this string s, bool isMultiple)
        {
            StringBuilder sb = new StringBuilder("{");

            if (!isMultiple)
            {
                string[] OneField = s.Split(':');
                foreach (string str in OneField)
                {
                    sb.Append($"\"{str}\":");
                }
                sb.RemoveLast();
                sb.Append('}');
                return sb.ToString();
            }

            char ch = ':';
            string[][] MultipleFields = s.DoubleSplit(':', ',');

            for (int i = 0; i < MultipleFields.Length; i++)
            {
                for (int j = 0; j < MultipleFields[i].Length; j++)
                {
                    string content = MultipleFields[i][j];
                    sb.Append($"\"{content}\"").Append(ch);
                    ch = ':';
                }
                ch = ',';
            }
            sb.RemoveLast();
            sb.Append('}');
            return sb.ToString();
        }
    }
}
