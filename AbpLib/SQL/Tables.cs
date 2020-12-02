using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AbpLib.SQL
{
    public static class Tables
    {
        public static DataTable CreateTable(this AbpSql s, string query)
        {
            using SqlConnection cn = new SqlConnection(s.CN);
            cn.Open();
            using IDataReader dtr = new SqlCommand(query, cn).ExecuteReader(CommandBehavior.CloseConnection);
            return AsIDataReader(dtr);
        }

        public static async Task<DataTable> CreateTableAsync(this AbpSql s, string query)
        {
            using SqlConnection cn = new SqlConnection(s.CN);
            await cn.OpenAsync();
            using IDataReader dtr = await new SqlCommand(query, cn).ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return AsIDataReader(dtr);
        }

        private static DataTable AsIDataReader(IDataReader dataReader)
        {
            DataTable schemaTable = dataReader.GetSchemaTable();
            DataTable resultTable = new DataTable();

            for (int i = 0; i < schemaTable.Rows.Count; i++)
            {
                DataColumn dataColumn = new DataColumn
                {
                    ColumnName = (string)schemaTable.Rows[i]["ColumnName"],
                    DataType = Type.GetType(schemaTable.Rows[i]["DataType"].ToString()),
                    ReadOnly = (bool)schemaTable.Rows[i]["IsReadOnly"],
                    AutoIncrement = (bool)schemaTable.Rows[i]["IsAutoIncrement"],
                    Unique = (bool)schemaTable.Rows[i]["IsUnique"]
                };
                resultTable.Columns.Add(dataColumn);
            }

            while (dataReader.Read())
            {
                DataRow dataRow = resultTable.NewRow();
                for (int i = 0; i < resultTable.Columns.Count; i++)
                {
                    dataRow[i] = dataReader[i];
                }
                resultTable.Rows.Add(dataRow);
            }
            return resultTable;
        }
    }
}
