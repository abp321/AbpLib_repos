namespace AbpLib.SQL
{
    public readonly struct Queries
    {
        public const string DISABLE_INDEXES = "DECLARE @sql AS VARCHAR(MAX)='' SELECT @sql = @sql + 'ALTER INDEX ' + sys.indexes.name + ' ON  ' + sys.objects.name + ' DISABLE;' +CHAR(13)+CHAR(10) FROM sys.indexes JOIN sys.objects ON sys.indexes.object_id = sys.objects.object_id WHERE sys.indexes.type_desc = 'NONCLUSTERED' AND sys.objects.type_desc = 'USER_TABLE' EXEC(@sql);";
        public const string ENABLE_INDEXES = "DECLARE @sql AS VARCHAR(MAX)='' SELECT @sql = @sql + 'ALTER INDEX ' + sys.indexes.name + ' ON  ' + sys.objects.name + ' REBUILD;' +CHAR(13)+CHAR(10) FROM sys.indexes JOIN sys.objects ON sys.indexes.object_id = sys.objects.object_id WHERE sys.indexes.type_desc = 'NONCLUSTERED' AND sys.objects.type_desc = 'USER_TABLE' EXEC(@sql);";
        public static string TABLENAMES(string DB) => $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG='{DB}'";
        public static string AllColumnNames(string TABLE) => $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{TABLE}' ORDER BY ORDINAL_POSITION";
        public static string AllColumnNames_NOPK(string TABLE) => $"DECLARE @tableName nvarchar(max) = '{TABLE}' SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName EXCEPT SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS [tc] JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE [ku] ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME AND ku.table_name = @tableName";
        public static string PKFromTable(string TABLE)=> $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1 AND TABLE_NAME = '{TABLE}'";
        public static string COLUMN_COUNT(string DB, string TABLE)=> $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE table_catalog = '{DB}'  AND table_name = '{TABLE}'";
        public static string SEARCH_TABLE(string SEARCH, string TABLE)=> $"declare @SearchValue as varchar(20) set @SearchValue = '{SEARCH}' select * from {TABLE} where convert(nvarchar(255),(select {TABLE}.* for XML PATH (''),TYPE).query('for $item in * where $item=sql:variable(\"@SearchValue\") return $item'))<>''";
        public static string ROWCOUNT(string TABLE) => $"select sum([rows]) from sys.partitions where object_id=object_id('{TABLE}') and index_id in (0,1)";
        public static string SELECT_FROM_ROWINDEX(string TABLE, int ROWINDEX, string PKFIELD,string FIELD) => $"Select {FIELD} from (Select ROW_NUMBER() OVER (order by {PKFIELD}) as 'Row_Number', {FIELD}  from {TABLE}) as tbl Where tbl.Row_Number = {ROWINDEX}";
        public static string LAST_USERUPDATE(string DB, string TABLE) => $"SELECT top 1 last_user_update FROM sys.dm_db_index_usage_stats WHERE database_id = DB_ID('{DB}') AND OBJECT_ID=OBJECT_ID('{TABLE}') order by last_user_update desc";
        public static string LAST_UPDATE(string DB, string TABLE)=> $"SELECT user_updates, last_user_update, system_updates,last_system_update FROM sys.dm_db_index_usage_stats WHERE database_id = DB_ID('{DB}') AND OBJECT_ID=OBJECT_ID('{TABLE}')";
        public static string FIRST_PKFIELD(string TABLE) => $"SELECT top 1 COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1 AND TABLE_NAME = '{TABLE}'";
        public static string DATATYPE(string TABLE, string FIELD) => $"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{TABLE}' AND COLUMN_NAME = '{FIELD}'";
    }
}
