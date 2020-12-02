using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AbpLib.SQL
{
    public static class AbpSqlFiles
    {
        public static void UploadFile(this AbpSql s,byte[] bytes, string file)
        {
            try
            {
                using SqlConnection cn = new SqlConnection(s.CN);
                using SqlCommand cmd = new SqlCommand
                {
                    CommandText = $@"insert into {s.TABLE} (FileBytes,FileName,FileType,UploadDate) values (@data,@name,@ext,@upload)",
                    Connection = cn,
                    CommandType = CommandType.Text
                };
                cmd.Parameters.Add("@data", SqlDbType.VarBinary, bytes.Length).Value = bytes;
                cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = file.Split('.')[0];
                cmd.Parameters.Add("@ext", SqlDbType.VarChar).Value = file.Split('.')[1];
                cmd.Parameters.Add("@upload", SqlDbType.DateTime).Value = DateTime.Now;
                cn.Open();
                cmd.ExecuteNonQuery();
                cn.Close();
            }
            catch(Exception)
            {

            }
        }

        public static byte[] DownloadFile(this AbpSql s, string name)
        {
            using SqlConnection cn = new SqlConnection(s.CN);
            using SqlCommand cmd = cn.CreateCommand();
            cmd.CommandText = $@"SELECT FileBytes FROM {s.TABLE} WHERE  FileName = @name";
            cmd.Parameters.AddWithValue("@name", name);
            cn.Open();
            byte[] result = cmd.ExecuteScalar() as byte[];
            cn.Close();
            return result;
        }
    }
}
