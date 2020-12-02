using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AbpLib.SQL
{
    public class AbpSql
    {
        public string CN { get; set; }
        public string TABLE { get; set; }
        public string DATABASE { get; set; }
        public string USER { get; set; }
        public string PASSWORD { get; set; }

        public void SetConnection(string NewConnection) => InitConnection(NewConnection, TABLE);
        public void SetTable(string NewTable) => TABLE = NewTable;

        public Task<int> ColumnCount() => CN.SqlValue<int>(Queries.COLUMN_COUNT(DATABASE, TABLE));
        public Task<int> ROWCOUNT(string table) => CN.SqlValue<int>(Queries.ROWCOUNT(table));
        public Task<DateTime> LastUserChange(string table) => CN.SqlValue<DateTime>(Queries.LAST_USERUPDATE(DATABASE, table));
        public Task<string> GetDataType(string table, string field) => CN.SqlValue<string>(Queries.DATATYPE(table, field));

        public Task<string[]> ColumnNames(string table) => CN.SqlArray<string>(Queries.AllColumnNames(table));
        public Task<string[]> ColumnNamesNOPK(string table) => CN.SqlArray<string>(Queries.AllColumnNames_NOPK(table));
        public Task<string[]> TableNames() => CN.SqlArray<string>(Queries.TABLENAMES(DATABASE));

        public Task<T> GetValue<T>(string query) => CN.SqlValue<T>(query);
        public Task<T> GetModel<T>(string query) => CN.LoadModel<T>(query);

        public Task<T[]> GetArray<T>(string query) => CN.SqlArray<T>(query);
        public Task<T[]> GetModels<T>(string query) => CN.LoadModels<T>(query);

        public IAsyncEnumerable<Dictionary<string, object>> GetRecordStream(string query) => CN.SqlRecordStream(query);
        public Task<Dictionary<string, object>> OneRecord(string query) => CN.GetOneRecord(query);

        public Task<string[]> GetStringArray(string query) => CN.SqlStringArray(query);
        public Task<bool> CheckQuery(string query) => CN.Check(query);

        public Task DisableIndexes() => CN.ExeQuery(Queries.DISABLE_INDEXES);
        public Task EnableIndexes() => CN.ExeQuery(Queries.ENABLE_INDEXES);
        public Task Execute(string query) => CN.ExeQuery(query);

        public Task InsertRecord<T>(T m, string table, params string[] byteFields) => CN.InsertValues(m, table, byteFields);
        public Task InsertRecord<T>(T m, string table, string[] exceptionFields, params string[] byteFields) => CN.InsertValues(m, table, exceptionFields, byteFields);

        public Task<int> InsertRecordReturnUK<T>(T m, string table, params string[] byteFields) => CN.InsertValuesReturnUK(m, table, byteFields);

        public Task UpdateRecord<T>(T m, string query, params string[] byteFields) => CN.UpdateValues(m, query, byteFields);
        public Task UpdateRecord<T>(T m, string query, string[] exceptionFields,params string[] byteFields) => CN.UpdateValues(m, query,exceptionFields, byteFields);
        public AbpSql(string ConnectionString, string Table = "")
        {
            InitConnection(ConnectionString, Table);
        }

        private void InitConnection(string ConnectionString, string Table)
        {
            CN = ConnectionString;
            TABLE = Table;
            DATABASE = ConnectionString.GetBetween("Initial Catalog=", ";");
            if (CN.Contains("Password") && CN.Contains("User ID"))
            {
                USER = CN.GetBetween("User ID=", ";");
                PASSWORD = CN.Split(';')[^1].Split('=')[^1];
            }
        }

        public async Task<bool> ResetTable(string user, string password)
        {
            bool CanReset = USER == user && PASSWORD == password;
            if (CanReset) await CN.ExeQuery($"TRUNCATE TABLE {TABLE}; dbcc checkident('dbo.{TABLE}', RESEED, 1)");

            return CanReset;
        }

        public async Task ClearNulls(string table = "")
        {
            table = table != "" ? table : TABLE;
            StringBuilder sb = new StringBuilder();
            string[] columns = await CN.SqlArray<string>(Queries.AllColumnNames_NOPK(table));
            for (int i = 0; i < columns.Length; i++)
            {
                bool last = i == columns.Length - 1;
                char k = last ? ' ' : ';';
                string UpdateString = $"update {table} set {columns[i]} = '' where {columns[i]} is null{k}";
                sb.Append(UpdateString);
            }
            await CN.ExeQuery(sb.ToString());
        }

        public async Task InsertBytes(string table, string column, string stmt, byte[] bytes, bool update = false)
        {
            await CN.InsertByteArray(table, column, stmt, bytes, update);
        }
    }

    internal static class AbpSqlEX
    {
        internal static async Task ExeQuery(this string connection, string query)
        {
            using SqlConnection cn = new SqlConnection(connection);
            using SqlCommand cmd = new SqlCommand(query, cn);
            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await cn.CloseAsync();
        }

        internal static async Task<T> SqlValue<T>(this string connection, string query)
        {
            using SqlConnection cn = new SqlConnection(connection);
            using SqlCommand cmd = new SqlCommand(query, cn);
            await cn.OpenAsync();
            object val = await cmd.ExecuteScalarAsync();
            await cn.CloseAsync();
            return val.ParseDBValue<T>();
        }

        internal static async Task<T[]> SqlArray<T>(this string connection, string query)
        {
            using SqlConnection cn = new SqlConnection(connection);
            using SqlCommand cmd = new SqlCommand(query, cn);
            await cn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            List<T> list = new List<T>();
            while (await reader.ReadAsync())
            {
                list.Add(reader.GetValue(0).ParseDBValue<T>());
            }
            await cn.CloseAsync();
            return list.ToArray();
        }

        internal static async Task<string[]> SqlStringArray(this string connection, string query)
        {
            using SqlConnection cn = new SqlConnection(connection);
            using SqlCommand cmd = new SqlCommand(query, cn);
            await cn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            List<string> list = new List<string>();
            while (await reader.ReadAsync())
            {
                list.Add(reader.GetValue(0).ToString());
            }
            await cn.CloseAsync();
            return list.ToArray();
        }

        internal static async IAsyncEnumerable<Dictionary<string, object>> SqlRecordStream(this string connection, string query)
        {
            using SqlConnection cn = new SqlConnection(connection);
            using SqlCommand cmd = new SqlCommand(query, cn);
            await cn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    d.TryAdd(reader.GetName(i), reader.GetValue(i));
                }

                if (d.Count > 0) yield return d;
            }
            await cn.CloseAsync();
        }

        internal static async Task<Dictionary<string, object>> GetOneRecord(this string connection, string query)
        {
            using SqlConnection cn = new SqlConnection(connection);
            using SqlCommand cmd = new SqlCommand(query, cn);
            await cn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    d.TryAdd(reader.GetName(i), reader.GetValue(i));
                }

                if (d.Count > 0)
                {
                    await cn.CloseAsync();
                    return d;
                }
            }
            await cn.CloseAsync();
            return new Dictionary<string, object>();
        }

        internal static async Task<T> LoadModel<T>(this string connection, string query)
        {
            T m = Activator.CreateInstance<T>();
            PropertyInfo[] props = m.GetType().GetProperties();

            using SqlConnection cn = new SqlConnection(connection);
            using SqlCommand cmd = new SqlCommand(query, cn);
            await cn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    object obj = await reader.GetFieldValueAsync<object>(i);
                    string name = reader.GetName(i);

                    foreach (PropertyInfo p in props.Where(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        object safeValue = obj.ParseDBValue<object>();
                        if (!safeValue.IsNullOrDefault())
                        {
                            if (safeValue.GetType() == typeof(byte[]))
                            {
                                p.SetValue(m, Convert.ChangeType(Convert.ToBase64String((byte[])safeValue), typeof(string)));
                            }
                            else
                            {
                                p.SetValue(m, Convert.ChangeType(safeValue, p.PropertyType));
                            }
                        }
                    }
                }
                await cn.CloseAsync();
                break;
            }
            return m;
        }

        internal static async Task<T[]> LoadModels<T>(this string connection, string query)
        {
            List<T> list = new List<T>();
            T m = Activator.CreateInstance<T>();
            PropertyInfo[] props = m.GetType().GetProperties();

            using SqlConnection cn = new SqlConnection(connection);
            using SqlCommand cmd = new SqlCommand(query, cn);
            await cn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    object obj = await reader.GetFieldValueAsync<object>(i);
                    string name = reader.GetName(i);

                    foreach (PropertyInfo p in props.Where(s => s.Name.Equals(name,StringComparison.OrdinalIgnoreCase)))
                    {
                        object safeValue = obj.ParseDBValue<object>();
                        if (!safeValue.IsNullOrDefault())
                        {
                            if (safeValue.GetType() == typeof(byte[]))
                            {
                                p.SetValue(m, Convert.ChangeType(Convert.ToBase64String((byte[])safeValue), typeof(string)));
                            }
                            else
                            {
                                p.SetValue(m, Convert.ChangeType(safeValue, p.PropertyType));
                            }
                        }
                    }
                }
                list.Add(m);
            }
            await cn.CloseAsync();
            return list.ToArray();
        }

        internal static async Task UpdateValues<T>(this string connection, T m, string query, params string[] byteFields)
        {
            if (query.ContainsAny("from", "where"))
            {
                string table = query.WordAfter("from");
                string condition = query.GetBetween(table, query.LastWord()) + query.LastWord();
                StringBuilder UpdateQuery = new StringBuilder($"UPDATE {table} SET ");
                PropertyInfo[] props = m.GetType().GetProperties();

                List<SqlParameter> parameters = new List<SqlParameter>();

                for (int i = 0; i < props.Length; i++)
                {
                    string name = $"[{props[i].Name}]";
                    object value = props[i].GetValue(m);
                    if (value != null)
                    {
                        string paramName = $"@V{i + 1}";
                        UpdateQuery.Append($"{name}={paramName},");

                        if (byteFields.ContainsAny(name))
                        {
                            byte[] bytes = Convert.FromBase64String(value.ToString());
                            parameters.AddSqlParameter(paramName, bytes);
                        }
                        else
                        {
                            parameters.AddSqlParameter(paramName, value);
                        }
                    }
                }
                UpdateQuery.RemoveLast(condition);

                if (parameters.Count > 0)
                {
                    using SqlConnection cn = new SqlConnection(connection);

                    using SqlCommand cmd = new SqlCommand(UpdateQuery.ToString(), cn);
                    cmd.Parameters.AddRange(parameters.ToArray());
                    await cn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    await cn.CloseAsync();
                }
            }
        }

        internal static async Task UpdateValues<T>(this string connection, T m, string query, string[] exceptionFields, params string[] byteFields)
        {
            if (query.ContainsAny("from", "where"))
            {
                string table = query.WordAfter("from");
                string condition = query.GetBetween(table, query.LastWord()) + query.LastWord();
                StringBuilder UpdateQuery = new StringBuilder($"UPDATE {table} SET ");
                PropertyInfo[] props = m.GetType().GetProperties().Where(x=> !x.Name.ContainsAny(exceptionFields)).ToArray();
                List<SqlParameter> parameters = new List<SqlParameter>();

                for (int i = 0; i < props.Length; i++)
                {
                    string name = $"[{props[i].Name}]";
                    object value = props[i].GetValue(m);
                    if (value != null)
                    {
                        string paramName = $"@V{i + 1}";
                        UpdateQuery.Append($"{name}={paramName},");

                        if (byteFields.ContainsAny(name))
                        {
                            byte[] bytes = Convert.FromBase64String(value.ToString());
                            parameters.AddSqlParameter(paramName, bytes);
                        }
                        else
                        {
                            parameters.AddSqlParameter(paramName, value);
                        }
                    }
                }
                UpdateQuery.RemoveLast(condition);

                if (parameters.Count > 0)
                {
                    using SqlConnection cn = new SqlConnection(connection);

                    using SqlCommand cmd = new SqlCommand(UpdateQuery.ToString(), cn);
                    cmd.Parameters.AddRange(parameters.ToArray());
                    await cn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    await cn.CloseAsync();
                }
            }
        }

        internal static async Task InsertValues<T>(this string connection, T m, string table, params string[] byteFields)
        {
            StringBuilder Fields = new StringBuilder($"INSERT INTO {table}(");
            StringBuilder Values = new StringBuilder(" VALUES (");
            PropertyInfo[] props = m.GetType().GetProperties();
            List<SqlParameter> parameters = new List<SqlParameter>();

            for (int i = 0; i < props.Length; i++)
            {
                string name = $"[{props[i].Name}]";
                object value = props[i].GetValue(m);
                if (value != null)
                {
                    bool last = i == props.Length - 1;
                    string paramName = $"@V{i + 1}";

                    char k = last ? ')' : ',';
                    Fields.Append($"{name}{k}");
                    Values.Append($"{paramName}{k}");
                    if (byteFields.ContainsAny(name))
                    {
                        byte[] bytes = Convert.FromBase64String(value.ToString());
                        parameters.AddSqlParameter(paramName, bytes);
                    }
                    else
                    {
                        parameters.AddSqlParameter(paramName, value);
                    }
                }
            }
            Fields.Append(Values);

            if (parameters.Count > 0)
            {
                using SqlConnection cn = new SqlConnection(connection);
                using SqlCommand cmd = new SqlCommand(Fields.ToString(), cn);
                cmd.Parameters.AddRange(parameters.ToArray());
                await cn.OpenAsync();

                await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }
        }

        internal static async Task InsertValues<T>(this string connection, T m, string table, string[] exceptionFields, params string[] byteFields)
        {
            StringBuilder Fields = new StringBuilder($"INSERT INTO {table}(");
            StringBuilder Values = new StringBuilder(" VALUES (");
            PropertyInfo[] props = m.GetType().GetProperties().Where(x => !x.Name.ContainsAny(exceptionFields)).ToArray();
            List<SqlParameter> parameters = new List<SqlParameter>();

            for (int i = 0; i < props.Length; i++)
            {
                string name = $"[{props[i].Name}]";
                object value = props[i].GetValue(m);
                if (value != null)
                {
                    bool last = i == props.Length - 1;
                    string paramName = $"@V{i + 1}";

                    char k = last ? ')' : ',';
                    Fields.Append($"{name}{k}");
                    Values.Append($"{paramName}{k}");
                    if (byteFields.ContainsAny(name))
                    {
                        byte[] bytes = Convert.FromBase64String(value.ToString());
                        parameters.AddSqlParameter(paramName, bytes);
                    }
                    else
                    {
                        parameters.AddSqlParameter(paramName, value);
                    }
                }
            }
            Fields.Append(Values);

            if (parameters.Count > 0)
            {
                using SqlConnection cn = new SqlConnection(connection);
                using SqlCommand cmd = new SqlCommand(Fields.ToString(), cn);
                cmd.Parameters.AddRange(parameters.ToArray());
                await cn.OpenAsync();

                await cmd.ExecuteNonQueryAsync();
                await cn.CloseAsync();
            }
        }

        internal static async Task<int> InsertValuesReturnUK<T>(this string connection, T m, string table, string[] byteFields)
        {
            int result = 0;

            StringBuilder Fields = new StringBuilder($"INSERT INTO {table}(");
            StringBuilder Values = new StringBuilder(" VALUES (");
            PropertyInfo[] props = m.GetType().GetProperties();
            List<SqlParameter> parameters = new List<SqlParameter>();

            for (int i = 0; i < props.Length; i++)
            {
                string name = $"[{props[i].Name}]";
                object value = props[i].GetValue(m);
                if (value != null)
                {
                    bool last = i == props.Length - 1;
                    string paramName = $"@V{i + 1}";

                    char k = last ? ')' : ',';
                    Fields.Append($"{name}{k}");
                    Values.Append($"{paramName}{k}");
                    if (byteFields.ContainsAny(name))
                    {
                        byte[] bytes = Convert.FromBase64String(value.ToString());
                        parameters.AddSqlParameter(paramName, bytes);
                    }
                    else
                    {
                        parameters.AddSqlParameter(paramName, value);
                    }
                }
            }
            Fields.Append(Values).Append(";SELECT SCOPE_IDENTITY()");

            if (parameters.Count > 0)
            {
                using SqlConnection cn = new SqlConnection(connection);
                using SqlCommand cmd = new SqlCommand(Fields.ToString(), cn);
                cmd.Parameters.AddRange(parameters.ToArray());
                await cn.OpenAsync();

                result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                await cn.CloseAsync();
            }

            return result;
        }

        public static async Task<bool> Check(this string connection, string query)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(connection);
                await cn.OpenAsync();
                string QueryCheck = $"IF NOT EXISTS({query}) select 0 else select 1 ";
                SqlCommand check_value = new SqlCommand(QueryCheck, cn);
                object o = await check_value.ExecuteScalarAsync();
                int val = Convert.ToInt32(o);
                await cn.CloseAsync();
                return val == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task InsertByteArray(this string connection, string table, string column, string stmt, byte[] bytes, bool update)
        {
            using SqlConnection cn = new SqlConnection(connection);
            await cn.OpenAsync();
            string query = update ? $"update {table} set {column} = @data {stmt}" : $"INSERT INTO {table} ({column}) VALUES (@data) {stmt}";
            using SqlCommand cmd = new SqlCommand(query, cn);
            cmd.Parameters.Add("@data", SqlDbType.VarBinary, bytes.Length);
            cmd.Parameters["@data"].Value = bytes;
            await cmd.ExecuteNonQueryAsync();
            await cn.CloseAsync();
        }
    }
}
