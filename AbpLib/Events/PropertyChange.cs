using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AbpLib.Events
{
    public class PropertyChange<T>
    {
        private readonly string _name;
        public PropertyChange(string name = "")
        {
            _name = name;
        }

        private T previous_value;
        private T current_value;

        public T Value
        {
            get
            {
                return current_value;
            }
            set
            {
                current_value = (T)Convert.ChangeType(value, typeof(T));
                if (current_value != null && previous_value != null && !previous_value.Equals(value))
                {
                    OnPropertyChanged(_name);
                }
                previous_value = current_value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
