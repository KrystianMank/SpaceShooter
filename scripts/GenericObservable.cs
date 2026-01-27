using System;

namespace GenericObervable
{
    public class Observable<T>
    {
        private T _value;

        public class ChanedEventArgs : EventArgs
        {
            public T NewValue { get; set; }
            public T OldValue { get; set; }
        }

        public EventHandler<ChanedEventArgs> Changed;

        public T Value
        {
            get { return _value; }
            set
            {
                if (!value.Equals(_value))
                {
                    T oldValue = _value;
                    _value = value;

                    EventHandler<ChanedEventArgs> handler = Changed;

                    if (handler != null)
                    {
                        handler(this, new ChanedEventArgs
                        {
                            OldValue = oldValue,
                            NewValue = value
                        });
                    }
                }
            }
        }
    }
}