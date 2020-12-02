using AbpLib.Events;
using AbpLib.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AbpLib.SQL
{
    public class AbpSqlEvents
    {
        internal class EventModel
        {
            public string Table { get; set; }
            public DateTime ChangeDate { get; set; }
        }

        private string[] _tables;
        private readonly Action<object, PropertyChangedEventArgs> _PropertyMethod;
        private PropertyChange<string>[] SqlProperties;
        private readonly AbpSql s;
        private readonly int _interval;

        private string[] filter = Array.Empty<string>();

        public AbpSqlEvents(AbpSql SqlObject, Action<object, PropertyChangedEventArgs> PropertyMethod, int interval = 1000)
        {
            s = SqlObject;
            _PropertyMethod = PropertyMethod;
            _interval = interval;
        }

        public void SetFilter(params string[] values) => filter = values;

        public void StartChangeDetection(params string[] tables)
        {
            _tables = tables;
            if (filter.Length > 0) _tables = _tables.Where(s => !s.StartsWithAny(filter)).ToArray();

            SqlProperties = new PropertyChange<string>[_tables.Length];

            for (int i = 0; i < _tables.Length; i++)
            {
                SqlProperties[i] = new PropertyChange<string>("AbpSqlEvents");
                RegisterEvent(SqlProperties[i]);
            }

            new Thread(async _ =>
            {
                for (int j = 0; j < SqlProperties.Length; j++)
                {
                    await foreach (EventModel m in GetLastChange(j))
                    {
                        SqlProperties[j].Value = m.SERIALIZE();
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        private void RegisterEvent(PropertyChange<string> pe) => pe.PropertyChanged += _PropertyMethod.Invoke;
        private void UnregisterEvent(PropertyChange<string> pe) => pe.PropertyChanged -= _PropertyMethod.Invoke;

        public void RegisterEvents()
        {
            for (int i = 0; i < SqlProperties.Length; i++) RegisterEvent(SqlProperties[i]);
        }

        public void UnregisterEvents()
        {
            for (int i = 0; i < SqlProperties.Length; i++) UnregisterEvent(SqlProperties[i]);
        }

        private async IAsyncEnumerable<EventModel> GetLastChange(int index)
        {
            while (true)
            {
                string GetTable = _tables[index];
                DateTime date = await s.LastUserChange(GetTable);

                EventModel m = new EventModel
                {
                    ChangeDate = date,
                    Table = GetTable
                };
                yield return m;

                await Task.Delay(_interval);

            }
        }
    }
}
