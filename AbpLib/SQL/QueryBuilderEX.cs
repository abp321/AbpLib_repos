using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace AbpLib.SQL
{
    public static class QueryBuilderEX
    {
        public static T ParseDBValue<T>(this object obj)
        {
            return (obj == null || obj == DBNull.Value) ? default : (T)obj;
        }

        public static string HandleSingleQuote(this string s)
        {
            if (s.Contains("'"))
            {
                string[] arr = s.Split("'");

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < arr.Length; i++)
                {
                    bool last = i == arr.Length - 1;
                    string x = last ? "" : "''";
                    sb.Append(arr[i] + x);
                }
                return sb.ToString();
            }
            return s;
        }

        public static string ToSelection(this string[] Columns, bool quoted = false)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Columns.Length; i++)
            {
                bool last = i == Columns.Length - 1;
                char k = last ? ' ' : ',';

                if (!quoted) sb.Append($"{Columns[i]}{k}");
                else sb.Append($"\"{Columns[i]}\"{k}");
            }
            return sb.ToString();
        }

        public static string ToSelectionSquareBrackets(this string[] Columns)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Columns.Length; i++)
            {
                bool last = i == Columns.Length - 1;
                char k = last ? ' ' : ',';

                sb.Append($"[{Columns[i]}]{k}");
            }
            return sb.ToString();
        }

        public static string CSType(this string sqlType)
        {
            return sqlType.ToLower() switch
            {
                "varchar" => "string",
                "nvarchar" => "string",
                "smallint" => "short",
                "int" => "int",
                "bigint" => "long",
                "datetime" => "DateTime",
                "datetime2" => "DateTime",
                "date" => "DateTime",
                "bit" => "bool",
                "numeric" => "decimal",
                "money" => "decimal",
                "text" => "string",
                "nchar" => "string",
                "char" => "string",
                "binary" => "byte[]",
                "varbinary" => "byte[]",
                "smallmoney" => "decimal",
                "xml" => "string",
                "tinyint" => "byte",
                "uniqueidentifier" => "Guid",
                _ => sqlType,
            };
        }

        public static string UpdateBuild<T>(this T m, string table)
        {
            StringBuilder sb = new StringBuilder($"UPDATE {table} SET ");
            PropertyInfo[] prop = m.GetType().GetProperties();

            foreach (PropertyInfo p in prop)
            {
                object value = p.GetValue(m);
                if (!value.IsNullOrDefault())
                {
                    sb.Append($"{p.Name} = {value.ParseValueInQuery()},");
                }
            }
            sb.RemoveLast();
            return sb.ToString();
        }

        private static string ParseValueInQuery(this object value)
        {
            string txt = $"'{value}'";
            Type type = value.GetType();
            return type switch
            {
                Type t when t == typeof(short) | t == typeof(int) | t == typeof(long) => txt.RemoveChar('\''),
                Type t when t == typeof(float) | t == typeof(double) | t == typeof(decimal) => txt.RemoveChar('\'').ReplaceChars('.', ','),
                Type t when t == typeof(DateTime) => $"CONVERT(datetime,{txt},105)",
                _ => txt,
            };
        }

        public static void AddSqlParameter(this List<SqlParameter> parameters,string paramName, object value)
        {
            switch (value.GetType())
            {
                case Type t when t == typeof(byte[]):
                    parameters.Add(new SqlParameter(paramName, SqlDbType.VarBinary,((byte[])value).Length) { Value = (byte[])value });
                    break;
                case Type t when t == typeof(bool):
                    parameters.Add(new SqlParameter(paramName, SqlDbType.Bit) { Value = (bool)value });
                    break;
                case Type t when t == typeof(DateTime):
                    parameters.Add(new SqlParameter(paramName, SqlDbType.DateTime) { Value = (DateTime)value });
                    break;
                case Type t when t == typeof(short):
                    parameters.Add(new SqlParameter(paramName, SqlDbType.SmallInt) { Value = (short)value });
                    break;
                case Type t when t == typeof(int):
                    parameters.Add(new SqlParameter(paramName, SqlDbType.Int) { Value = (int)value });
                    break;
                case Type t when t == typeof(long):
                    parameters.Add(new SqlParameter(paramName, SqlDbType.BigInt) { Value = (long)value });
                    break;
                case Type t when t == typeof(double):
                    parameters.Add(new SqlParameter(paramName, SqlDbType.Float) { Value = (double)value });
                    break;
                case Type t when t == typeof(decimal):
                    parameters.Add(new SqlParameter(paramName, SqlDbType.Decimal) { Value = (decimal)value });
                    break;
                case Type t when t == typeof(Guid):
                    parameters.Add(new SqlParameter(paramName, SqlDbType.UniqueIdentifier) { Value = value });
                    break;
                default:
                    parameters.Add(new SqlParameter(paramName, value));
                    break;
            }
        }
    }
}
