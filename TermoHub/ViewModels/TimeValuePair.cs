using System;

namespace TermoHub.ViewModels
{
    public class TimeValuePair<T>
    {
        public DateTime Time { get; set; }
        public T Value { get; set; }

        public TimeValuePair()
            : this(DateTime.Now, default(T))
        {
        }

        public TimeValuePair(DateTime time, T value)
        {
            Time = time;
            Value = value;
        }
    }
}
