using System;

namespace CK.Plugin.Config
{
    public class CollectionElementChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets wich element is concerned by the event.
        /// </summary>
        public T Element { get; private set; }

        public CollectionElementChangedEventArgs( T element )
        {
            Element = element;
        }
    }
}
