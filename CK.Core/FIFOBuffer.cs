using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace CK.Core
{
    /// <summary>
    /// Simple implementation of a fixed size FIFO stack based on a circular buffer. 
    /// The .Net <see cref="Queue{T}"/>'s size increase as needed whereas this FIFO automatically loses the oldest items.
    /// Note that when <typeparamref name="T"/> is a reference type, null can be pushed and pop.
    /// This can easily be used as a LIFO stack thanks to <see cref="PopLast"/> and <see cref="PeekLast"/> methods.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    [Serializable]
    [DebuggerTypeProxy( typeof( CK.Core.Debugging.ReadOnlyCollectionDebuggerView<> ) )]
    public class FIFOBuffer<T> : IReadOnlyList<T>, ICKWritableCollector<T>, ISerializable
    {
        int _count;
        int _first;
        int _next;
        T[] _buffer;

        /// <summary>
        /// Initializes a new <see cref="FIFOBuffer{T}"/> with an initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity (can be 0).</param>
        public FIFOBuffer( int capacity )
        {
            if( capacity < 0 )
                throw new ArgumentException( "Capacity must be greater than or equal to zero.", "capacity" );
            _buffer = new T[capacity];
            _count = 0;
            _first = 0;
            _next = 0;
        }

        /// <summary>
        /// Gets or sets the capacity (internal buffer will be resized).
        /// </summary>
        public int Capacity
        {
            get { return _buffer.Length; }
            set
            {
                if( value == _buffer.Length ) return;

                if( value < 0 )
                    throw new ArgumentException( Resources.CapacityMustBeGreaterThanOrEqualToZero, "value" );

                var dst = new T[value];
                if( _count > 0 ) CopyTo( dst );
                _first = 0;
                if( _count > value )
                {
                    _count = value;
                    _next = 0;
                }
                else _next = _count;
                _buffer = dst;
            }
        }

        /// <summary>
        /// Gets the actual count of element: it is necessary less than or equal to <see cref="Capacity"/>.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Truncates the queue: only the <paramref name="newCount"/> newest items are kept.
        /// Pops as many old items (the ones that have been pushed first) in order for <see cref="Count"/> to be equal to newCount.
        /// </summary>
        /// <param name="newCount">The final number of items. If it is greater or equal to the current <see cref="Count"/>, nothing is done.</param>
        public void Truncate( int newCount )
        {
            if( newCount < 0 ) throw new ArgumentOutOfRangeException( "newCount" );
            if( newCount == 0 ) Clear();
            else while( _count > newCount )
                {
                    _buffer[_first] = default( T );
                    if( ++_first == _buffer.Length ) _first = 0;
                    _count--;
                }
        }

        /// <summary>
        /// Tests whether the buffer actually contains the given object.
        /// </summary>
        /// <param name="item">Object to test.</param>
        /// <returns>True if the object exists.</returns>
        public bool Contains( object item )
        {
            return IndexOf( item ) >= 0;
        }

        /// <summary>
        /// Gets the index of the given object.
        /// </summary>
        /// <param name="item">Object to find.</param>
        /// <returns>The index of the object or -1 if not found.</returns>
        public int IndexOf( object item )
        {
            return (item == null && default( T ) == null) || item is T ? IndexOf( (T)item ) : Int32.MinValue;
        }

        /// <summary>
        /// Gets the index of the given object.
        /// </summary>
        /// <param name="item">Object to find.</param>
        /// <returns>
        /// The index of the object or the bitwise complement of <see cref="Count"/> if not 
        /// found (that is a negative value, see <see cref="ICKReadOnlyList{T}.IndexOf"/>).
        /// </returns>
        public int IndexOf( T item )
        {
            var comparer = EqualityComparer<T>.Default;
            int bufferIndex = _first;
            for( int i = 0; i < _count; i++, bufferIndex++ )
            {
                if( bufferIndex == _buffer.Length ) bufferIndex = 0;
                if( comparer.Equals( _buffer[bufferIndex], item ) ) return i;
            }
            return ~_count;
        }

        /// <summary>
        /// Gets the element by index. Index 0 is the oldest item, the one returned by <see cref="Peek"/> and <see cref="Pop"/>.
        /// </summary>
        /// <param name="index">Index must be positive and less than <see cref="Count"/>.</param>
        /// <returns>The indexed element.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification="This is the right location to raise this exception!" )]
        public T this[int index]
        {
            get
            {
                if( index < 0 || index >= _count ) throw new IndexOutOfRangeException();
                index += _first;
                int rollIdx = index - _buffer.Length;
                return _buffer[rollIdx >= 0 ? rollIdx : index];
            }
        }

        /// <summary>
        /// Removes the element by index. Index 0 is the oldest item, the one returned by <see cref="Peek"/> and <see cref="Pop"/>.
        /// </summary>
        /// <param name="index">Index must be positive and less than <see cref="Count"/>.</param>
        public void RemoveAt( int index )
        {
            if( index < 0 || index >= _count ) throw new IndexOutOfRangeException();
            --_count;
            if( index == 0 )
            {
                _buffer[_first] = default( T );
                if( ++_first == _buffer.Length ) _first = 0;
                return;
            }
            index += _first;
            int len;
            int rollIdx = index - _buffer.Length;
            if( rollIdx < 0 ) 
            {
                len = _next - index;
                Debug.Assert( len != 0, "We do not RemoveAt( _count )." );
                if( len > 0 )
                {
                    Debug.Assert( _next > 0 );
                    Array.Copy( _buffer, index + 1, _buffer, index, len );
                    _buffer[_next] = default( T );
                    --_next;
                }
                else
                {
                    Debug.Assert( _next < index && index > _first && _next <= _first );
                    len = _buffer.Length - index - 1;
                    if( len > 0 ) Array.Copy( _buffer, index + 1, _buffer, index, len );
                    if( _next > 0 )
                    {
                        _buffer[_buffer.Length - 1] = _buffer[0];
                        Array.Copy( _buffer, 1, _buffer, 0, _next );
                        if( _next != _first ) _buffer[_next] = default( T );
                        --_next;
                    }
                    else
                    {
                        if( _first != 0 ) _buffer[0] = default( T );
                        _next = _buffer.Length - 1;
                    }
                }
            }
            else
            {
                len = _next - rollIdx;
                Debug.Assert( len != 0, "We do not RemoveAt( _count )." );
                if( len > 0 )
                {
                    Array.Copy( _buffer, rollIdx + 1, _buffer, rollIdx, len );
                    --_next;
                    Debug.Assert( _next >= 0 );
                }
                else
                {
                    len = _buffer.Length - rollIdx;
                    if( len > 0 ) Array.Copy( _buffer, rollIdx + 1, _buffer, rollIdx, len );
                    _buffer[_buffer.Length - 1] = _buffer[0];
                    if( _next > 0 ) Array.Copy( _buffer, 1, _buffer, 0, _next-- );
                    else _next = _buffer.Length - 1;
                }
            }
        }

        /// <summary>
        /// Clears the internal buffer.
        /// </summary>
        public void Clear()
        {
            _count = 0;
            _first = 0;
            _next = 0;
            Array.Clear( _buffer, 0, _buffer.Length );
        }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="item">Item to push.</param>
        public void Push( T item )
        {
            Debug.Assert( _count <= _buffer.Length );
            if( _buffer.Length > 0 )
            {
                _buffer[_next++] = item;
                if( _next == _buffer.Length ) _next = 0;
                if( _count == _buffer.Length )
                {
                    if( ++_first == _buffer.Length ) _first = 0;
                }
                else _count++;
            }
        }

        /// <summary>
        /// Gets and removes the first item (the one that has been <see cref="Push"/>ed first).
        /// <see cref="Count"/> must be greater than 0 otherwise an exception is thrown.
        /// </summary>
        /// <returns>The first (oldest) item.</returns>
        public T Pop()
        {
            if( _count == 0 ) throw new InvalidOperationException( Resources.FIFOBufferEmpty );
            var item = _buffer[_first];
            _buffer[_first] = default( T );
            if( ++_first == _buffer.Length ) _first = 0;
            _count--;
            return item;
        }

        /// <summary>
        /// Gets and removes the last item (the last one that has been <see cref="Push"/>ed).
        /// <see cref="Count"/> must be greater than 0 otherwise an exception is thrown.
        /// </summary>
        /// <returns>The last (newest) item.</returns>
        public T PopLast()
        {
            if( _count == 0 ) throw new InvalidOperationException( Resources.FIFOBufferEmpty );
            if( --_next < 0 ) _next = _first + _count - 1;
            var item = _buffer[_next];
            _buffer[_next] = default( T );
            _count--;
            return item;
        }

        /// <summary>
        /// Gets the first item (the one that has been <see cref="Push"/>ed first).
        /// <see cref="Count"/> must be greater than 0 otherwise an exception is thrown.
        /// </summary>
        /// <returns>The first (oldest) item.</returns>
        public T Peek()
        {
            if( _count == 0 ) throw new InvalidOperationException( Resources.FIFOBufferEmpty );
            return _buffer[_first];
        }

        /// <summary>
        /// Gets the last item (the last one that has been <see cref="Push"/>).
        /// <see cref="Count"/> must be greater than 0 otherwise an exception is thrown.
        /// </summary>
        /// <returns>The last (newest) item.</returns>
        public T PeekLast()
        {
            if( _count == 0 ) throw new InvalidOperationException( Resources.FIFOBufferEmpty );
            int t = _next-1;
            if( t < 0 ) t = _first + _count - 1;
            return _buffer[t];
        }

        bool ICKWritableCollector<T>.Add( T e )
        {
            Push( e );
            return true;
        }

        /// <summary>
        /// Copies as much possible items into the given array. Order is from oldest to newest.
        /// If the target array is too small to contain <see cref="Count"/> items, the newest ones
        /// are copied (the oldest, the ones that will <see cref="Pop"/> first, are skipped).
        /// </summary>
        /// <param name="array">Array that will contain the items.</param>
        /// <returns>Number of items copied.</returns>
        public int CopyTo( T[] array )
        {
            return CopyTo( array, 0  );
        }

        /// <summary>
        /// Copies as much possible items into the given array. Order is from oldest to newest. 
        /// If the target array is too small to contain <see cref="Count"/> items, the newest ones
        /// are copied (the oldest, the ones that will <see cref="Pop"/> first, are skipped).
        /// </summary>
        /// <param name="array">Array that will contain the items.</param>
        /// <param name="arrayIndex">Index in <paramref name="array"/> where copy must start.</param>
        /// <returns>Number of items copied.</returns>
        public int CopyTo( T[] array, int arrayIndex )
        {
            if( array == null ) throw new ArgumentNullException( "array" );
            return CopyTo( array, arrayIndex, array.Length - arrayIndex );
        }

        /// <summary>
        /// Copies as much possible items into the given array. Order is from oldest to newest.
        /// If <paramref name="count"/> is less than <see cref="Count"/>, the newest ones
        /// are copied (the oldest, the ones that will <see cref="Pop"/> first, are skipped).
        /// </summary>
        /// <param name="array">Array that will contain the items.</param>
        /// <param name="arrayIndex">Index in <paramref name="array"/> where copy must start.</param>
        /// <param name="count">Number of items to copy.</param>
        /// <returns>Number of items copied.</returns>
        public int CopyTo( T[] array, int arrayIndex, int count )
        {
            if( array == null ) throw new ArgumentNullException( "array" );
            if( count < 0 || arrayIndex < 0 || arrayIndex + count > array.Length ) throw new IndexOutOfRangeException();
            if( count == 0 ) return 0;

            // Number of item to copy: 
            // if there is enough available space, we copy the whole buffer (_count items) from head to tail.
            // if we need to copy less, we want to copy the count last items (and not the first ones).
            int head = _first;
            int toBeSkipped = _count - count;
            if( toBeSkipped > 0 )
            {
                Debug.Assert( _count > count, "We must copy less items than what we have." );
                head += toBeSkipped;
                if( head > _buffer.Length ) head -= _buffer.Length;
                Debug.Assert( head != _first, "The head for the copy is not the current head." );
            }
            else
            {
                Debug.Assert( count >= _count, "We are asked to copy all items (or more than we have)." );
                // Copy all existing items.
                count = _count;
                Debug.Assert( head == _first, "The head for the copy is the current head." );
            }
            // Detects whether we need 2 or only one copy.
            int afterHead = _buffer.Length - head;
            if( afterHead >= count )
            {
                // Count items are available right after the head.
                Array.Copy( _buffer, head, array, arrayIndex, count );
            }
            else
            {
                // We need two copies.
                // 1 - From head to the end.
                Array.Copy( _buffer, head, array, arrayIndex, afterHead );
                // 2 - From start to tail.
                Array.Copy( _buffer, 0, array, arrayIndex + afterHead, _next );
            }
            return count;
        }

        /// <summary>
        /// Gets the enumerator (from oldest to newest item).
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            int bufferIndex = _first;
            for( int i = 0; i < _count; i++, bufferIndex++ )
            {
                if( bufferIndex == _buffer.Length ) bufferIndex = 0;
                yield return _buffer[bufferIndex];
            }
        }

        /// <summary>
        /// Creates an array that contains <see cref="Count"/> items from oldest to newest.
        /// </summary>
        /// <returns>An array with the contained items. Never null.</returns>
        public T[] ToArray()
        {
            var t = new T[_count];
            CopyTo( t, 0, _count );
            return t;
        }

        /// <summary>
        /// Non-generic version of <see cref="GetEnumerator"/>.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        /// <summary>
        /// Overridden to display the current count of items and capacity for this buffer.
        /// </summary>
        /// <returns>Current count and capacity.</returns>
        public override string ToString()
        {
            return String.Format( "Count = {0} (Capacity = {1})", _count, _buffer.Length );
        }

        #if NET451 || NET46
        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        protected FIFOBuffer( SerializationInfo info, StreamingContext context )
        {
            _buffer = new T[info.GetInt32( "c" )];
            _count = info.GetInt32( "n" );
            T[] a = (T[])info.GetValue( "d", typeof(object) );
            a.CopyTo( _buffer, 0 );
        }

        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            GetObjectData( info, context );
        }

        /// <summary>
        /// Serialization.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        protected virtual void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "c", _buffer.Length );
            info.AddValue( "n", _count );
            info.AddValue( "d", ToArray() );
        }

        #endif

    }
}
