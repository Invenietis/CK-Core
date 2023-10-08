using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    /// <summary>
    /// Simple implementation of a fixed or dynamic size FIFO queue based on a circular buffer. 
    /// The .Net <see cref="Queue{T}"/>'s size increase as needed whereas this FIFO automatically loses the oldest items.
    /// This can easily be used as a LIFO stack thanks to <see cref="PopLast"/> and <see cref="PeekLast"/> methods.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public sealed class FIFOBuffer<T> : ICKReadOnlyList<T>, ICKWritableCollector<T>
    {
        T[] _buffer;
        int _count;
        int _head;
        int _tail;
        int _version;
        int _maxDynamicCapacity;

        /// <summary>
        /// Initializes a new <see cref="FIFOBuffer{T}"/> with an initial capacity and
        /// no <see cref="MaxDynamicCapacity"/> (fixed size buffer). This can be changed
        /// dynamically by setting <see cref="Capacity"/> or MaxDynamicCapacity.
        /// </summary>
        /// <param name="capacity">Initial capacity (can be 0).</param>
        public FIFOBuffer( int capacity )
        {
            Throw.CheckArgument( capacity >= 0 );
            _buffer = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        /// <summary>
        /// Initializes a new <see cref="FIFOBuffer{T}"/> with initial capacity and <see cref="MaxDynamicCapacity"/>.
        /// </summary>
        /// <param name="capacity">Initial capacity.</param>
        /// <param name="maxDynamicCapacity">Initial maximal capacity: the <see cref="Capacity"/> will automatically grow until up to this size.</param>
        public FIFOBuffer( int capacity = 0, int maxDynamicCapacity = 200 )
            : this( capacity )
        {
            _maxDynamicCapacity = maxDynamicCapacity;
        }

        /// <summary>
        /// Gets or sets the capacity (internal buffer will be resized).
        /// If the new explicit capacity is greater than the <see cref="MaxDynamicCapacity"/>, then
        /// the MaxDynamicCapacity is forgotten (set to 0): the buffer is no more dynamic.
        /// </summary>
        public int Capacity
        {
            get => _buffer.Length;
            set
            {
                if( value == _buffer.Length ) return;
                Throw.CheckOutOfRangeArgument( value >= 0 );
                if( value > _maxDynamicCapacity ) _maxDynamicCapacity = 0;
                var dst = new T[value];
                if( _count > 0 ) CopyTo( dst );
                _head = 0;
                if( _count > value )
                {
                    ++_version;
                    _count = value;
                    _tail = 0;
                }
                else _tail = _count;
                _buffer = dst;
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="Capacity"/> is dynamic thanks to a
        /// non zero maximal capacity.
        /// <see cref="Array.MaxLength"/> is the maximal size.
        /// Defaults to 0 (fixed capacity).
        /// </summary>
        public int MaxDynamicCapacity
        {
            get => _maxDynamicCapacity;
            set
            {
                if( _maxDynamicCapacity == value ) return;
                Throw.CheckOutOfRangeArgument( value >= 0 && value <= Array.MaxLength );
                // If the MaxDynamicCapacity becomes smaller than the current Capacity,
                // we shrink the current buffer.
                _maxDynamicCapacity = value;
                if( value != 0 && value < _buffer.Length )
                {
                    Capacity = value;
                }
            }
        }

        /// <summary>
        /// Gets the actual count of element: it is necessary less than or equal to <see cref="Capacity"/>.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets whether this buffer has reached its <see cref="Capacity"/> or <see cref="MaxDynamicCapacity"/>.
        /// </summary>
        public bool IsFull => _count == _buffer.Length && (_maxDynamicCapacity > 0 && _count == _maxDynamicCapacity);

        /// <summary>
        /// Truncates the queue: only the <paramref name="newCount"/> newest items are kept.
        /// Pops as many old items (the ones that have been pushed first) in order for <see cref="Count"/> to be equal to newCount.
        /// </summary>
        /// <param name="newCount">The final number of items. If it is greater or equal to the current <see cref="Count"/>, nothing is done.</param>
        public void Truncate( int newCount )
        {
            Throw.CheckOutOfRangeArgument( newCount >= 0 );
            if( newCount == 0 ) Clear();
            else
            {
                bool setDef = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
                if( _count > newCount )
                {
                    ++_version;
                    do
                    {
                        if( setDef ) _buffer[_head] = default!;
                        if( ++_head == _buffer.Length ) _head = 0;
                        _count--;
                    }
                    while( _count > newCount );
                }
            }
        }

        /// <summary>
        /// Tests whether the buffer actually contains the given object.
        /// </summary>
        /// <param name="item">Object to test.</param>
        /// <returns>True if the object exists.</returns>
        public bool Contains( object item ) => IndexOf( item ) >= 0;

        /// <summary>
        /// Gets the index of the given object.
        /// </summary>
        /// <param name="item">Object to find.</param>
        /// <returns>The index of the object or a negative value if not found.</returns>
        public int IndexOf( object item )
        {
            return item is T i ? IndexOf( i ) : Int32.MinValue;
        }

        /// <summary>
        /// Gets the index of the given object.
        /// </summary>
        /// <param name="item">Object to find.</param>
        /// <returns>
        /// The index of the object or -1.
        /// </returns>
        public int IndexOf( T item )
        {
            if( _head < _tail )
            {
                return Array.IndexOf( _buffer, item, _head, _count );
            }
            int i = Array.IndexOf( _buffer, item, _head, _buffer.Length - _head );
            if( i < 0 ) i = Array.IndexOf( _buffer, item, 0, _tail );
            return i;
        }

        /// <summary>
        /// Gets the element by index. Index 0 is the oldest item, the one returned by <see cref="Peek"/> and <see cref="Pop"/>.
        /// </summary>
        /// <param name="index">Index must be positive and less than <see cref="Count"/>.</param>
        /// <returns>The indexed element.</returns>
        public T this[int index]
        {
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            get
            {
                Throw.CheckOutOfRangeArgument( index >= 0 && index < _count );
                index += _head;
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
            Throw.CheckOutOfRangeArgument( index >= 0 && index < _count );
            --_count;
            ++_version;
            if( index == 0 )
            {
                if( RuntimeHelpers.IsReferenceOrContainsReferences<T>() ) _buffer[_head] = default!;
                if( ++_head == _buffer.Length ) _head = 0;
                return;
            }
            index += _head;
            int len;
            int rollIdx = index - _buffer.Length;
            if( rollIdx < 0 ) 
            {
                len = _tail - index;
                Debug.Assert( len != 0, "We do not RemoveAt( _count )." );
                if( len > 0 )
                {
                    Debug.Assert( _tail > 0 );
                    Array.Copy( _buffer, index + 1, _buffer, index, len );
                    if( RuntimeHelpers.IsReferenceOrContainsReferences<T>() ) _buffer[_tail] = default!;
                    --_tail;
                }
                else
                {
                    Debug.Assert( _tail < index && index > _head && _tail <= _head );
                    len = _buffer.Length - index - 1;
                    if( len > 0 ) Array.Copy( _buffer, index + 1, _buffer, index, len );
                    if( _tail > 0 )
                    {
                        _buffer[_buffer.Length-1] = _buffer[0];
                        Array.Copy( _buffer, 1, _buffer, 0, _tail );
                        if( _tail != _head && RuntimeHelpers.IsReferenceOrContainsReferences<T>() ) _buffer[_tail] = default!;
                        --_tail;
                    }
                    else
                    {
                        if( _head != 0 && RuntimeHelpers.IsReferenceOrContainsReferences<T>() ) _buffer[0] = default!;
                        _tail = _buffer.Length - 1;
                    }
                }
            }
            else
            {
                len = _tail - rollIdx;
                Debug.Assert( len != 0, "We do not RemoveAt( _count )." );
                if( len > 0 )
                {
                    Array.Copy( _buffer, rollIdx + 1, _buffer, rollIdx, len );
                    --_tail;
                    Debug.Assert( _tail >= 0 );
                }
                else
                {
                    len = _buffer.Length - rollIdx;
                    if( len > 0 ) Array.Copy( _buffer, rollIdx + 1, _buffer, rollIdx, len );
                    _buffer[_buffer.Length - 1] = _buffer[0];
                    if( _tail > 0 ) Array.Copy( _buffer, 1, _buffer, 0, _tail-- );
                    else _tail = _buffer.Length - 1;
                }
            }
        }

        /// <summary>
        /// Clears the internal buffer.
        /// </summary>
        public void Clear()
        {
            if( _count != 0 )
            {
                if( RuntimeHelpers.IsReferenceOrContainsReferences<T>() )
                {
                    if( _head < _tail )
                    {
                        Array.Clear( _buffer, _head, _count );
                    }
                    else
                    {
                        Array.Clear( _buffer, _head, _buffer.Length - _head );
                        Array.Clear( _buffer, 0, _tail );
                    }
                }
                ++_version;
                _count = 0;
            }
            _head = 0;
            _tail = 0;
        }

        /// <summary>
        /// Adds an item.
        /// If <see cref="Capacity"/> or <see cref="MaxDynamicCapacity"/> is reached, the
        /// oldest item is replaced.
        /// </summary>
        /// <param name="item">Item to push.</param>
        public void Push( T item )
        {
            int capacity = _buffer.Length;
            Debug.Assert( _count <= capacity );
            if( _maxDynamicCapacity > 0 && _count == capacity )
            {
                TryGrow();
                capacity = _buffer.Length;
            }
            if( capacity > 0 )
            {
                ++_version;
                _buffer[_tail++] = item;
                if( _tail == capacity ) _tail = 0;
                if( _count == capacity )
                {
                    if( ++_head == capacity ) _head = 0;
                }
                else _count++;
            }
        }

        void TryGrow()
        {
            Debug.Assert( _maxDynamicCapacity > 0 );
            const int GrowFactor = 2;

            // When the initial capacity is 0, bumps to 4.
            int newcapacity = GrowFactor * _buffer.Length;
            if( newcapacity == 0 ) newcapacity = 4;
            if( (uint)newcapacity > _maxDynamicCapacity ) newcapacity = _maxDynamicCapacity;
            Capacity = newcapacity;
        }

        /// <summary>
        /// Gets and removes the first item (the one that has been <see cref="Push"/>ed first).
        /// <see cref="Count"/> must be greater than 0 otherwise an exception is thrown.
        /// </summary>
        /// <returns>The first (oldest) item.</returns>
        public T Pop()
        {
            Throw.CheckState( Count != 0 );
            var item = _buffer[_head];
            Debug.Assert( item != null );
            if( RuntimeHelpers.IsReferenceOrContainsReferences<T>() ) _buffer[_head] = default!;
            if( ++_head == _buffer.Length ) _head = 0;
            _count--;
            ++_version;
            return item;
        }

        /// <summary>
        /// Gets and removes the last item (the last one that has been <see cref="Push"/>ed).
        /// <see cref="Count"/> must be greater than 0 otherwise an exception is thrown.
        /// </summary>
        /// <returns>The last (newest) item.</returns>
        public T PopLast()
        {
            Throw.CheckState( Count != 0 );
            if( --_tail < 0 ) _tail = _head + _count - 1;
            var item = _buffer[_tail];
            Debug.Assert( item != null );
            if( RuntimeHelpers.IsReferenceOrContainsReferences<T>() ) _buffer[_tail] = default!;
            _count--;
            ++_version;
            return item;
        }

        /// <summary>
        /// Gets the first item (the one that has been <see cref="Push"/>ed first).
        /// <see cref="Count"/> must be greater than 0 otherwise an exception is thrown.
        /// </summary>
        /// <returns>The first (oldest) item.</returns>
        public T Peek()
        {
            Throw.CheckState( Count != 0 );
            return _buffer[_head]!;
        }

        /// <summary>
        /// Tries to get the first item (the one that has been <see cref="Push"/>ed first).
        /// </summary>
        /// <param name="result">The first (oldest) item.</param>
        /// <returns>True on success, false if this buffer is empty.</returns>
        public bool TryPeek( [MaybeNullWhen( false )] out T result )
        {
            if( _count == 0 )
            {
                result = default!;
                return false;
            }
            result = _buffer[_head];
            return true;
        }

        /// <summary>
        /// Gets the last item (the last one that has been <see cref="Push"/>).
        /// <see cref="Count"/> must be greater than 0 otherwise an exception is thrown.
        /// </summary>
        /// <returns>The last (newest) item.</returns>
        public T PeekLast()
        {
            Throw.CheckState( Count != 0 );
            int t = _tail - 1;
            if( t < 0 ) t = _head + _count - 1;
            return _buffer[t];
        }

        /// <summary>
        /// Tries to get the last item (the last one that has been <see cref="Push"/>).
        /// </summary>
        /// <param name="result">The last (newest) item.</param>
        /// <returns>True on success, false if this buffer is empty.</returns>
        public bool TryPeekLast( [MaybeNullWhen( false )] out T result )
        {
            if( _count == 0 )
            {
                result = default!;
                return false;
            }
            int t = _tail - 1;
            if( t < 0 ) t = _head + _count - 1;
            result = _buffer[t];
            return true;
        }


        bool ICKWritableCollector<T>.Add( T e )
        {
            Push( e );
            return true;
        }

        /// <summary>
        /// Copies as much possible items into the given span. Order is from oldest to newest.
        /// If the span's length is less than <see cref="Count"/>, the newest ones
        /// are copied (the oldest, the ones that will <see cref="Pop"/> first, are skipped).
        /// </summary>
        /// <param name="destination">Span that will contain the items.</param>
        /// <returns>Number of items copied.</returns>
        public int CopyTo( Span<T> destination )
        {
            if( destination.IsEmpty ) return 0;
            int count = GetCopyInfo( destination.Length, out Span<T> first, out Span<T> second );
            if( count > 0 )
            {
                first.CopyTo( destination.Slice( 0, first.Length ) );
                if( second.Length > 0 )
                {
                    second.CopyTo(destination.Slice( first.Length, second.Length ) );
                }
            }
            return count;
        }

        int GetCopyInfo( int count, out Span<T> first, out Span<T> second )
        {
            Debug.Assert( count > 0 );
            // Number of item to copy: 
            // if there is enough available space, we copy the whole buffer (_count items) from head to tail.
            // if we need to copy less, we want to copy the count last items (and not the first ones).
            int head = _head;
            int toBeSkipped = _count - count;
            if( toBeSkipped > 0 )
            {
                Debug.Assert( _count > count, "We must copy less items than what we have." );
                head += toBeSkipped;
                if( head > _buffer.Length ) head -= _buffer.Length;
                Debug.Assert( head != _head, "The head for the copy is not the current head." );
            }
            else
            {
                Debug.Assert( count >= _count, "We are asked to copy all items (or more than we have)." );
                // Copy all existing items.
                count = _count;
                Debug.Assert( head == _head, "The head for the copy is the current head." );
            }
            // Detects whether we need 2 or only one copy.
            int afterHead = _buffer.Length - head;
            if( afterHead >= count )
            {
                // Count items are available right after the head.
                first = _buffer.AsSpan( head, count );
                second = default;
            }
            else
            {
                // We need two copies.
                // 1 - From head to the end.
                first = _buffer.AsSpan( head, afterHead );
                // Array.Copy( _buffer, head, array, arrayIndex, afterHead );
                // 2 - From start to tail.
                second = _buffer.AsSpan( 0, _tail );
                //Array.Copy( _buffer, 0, array, arrayIndex + afterHead, _next );
            }
            return count;
        }

        /// <summary>
        /// Creates an array that contains <see cref="Count"/> items from oldest to newest.
        /// </summary>
        /// <returns>An array with the contained items. Never null.</returns>
        public T[] ToArray()
        {
            if( _count == 0 ) return Array.Empty<T>();
            var t = new T[_count];
            CopyTo( t.AsSpan() );
            return t;
        }

        /// <summary>
        /// Specialized enumerator.
        /// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            readonly FIFOBuffer<T> _b;
            readonly int _version;
            // -1 = not started, -2 = ended/disposed
            int _index;   
            T? _currentElement;

            internal Enumerator( FIFOBuffer<T> b )
            {
                _b = b;
                _version = b._version;
                _index = -1;
                _currentElement = default;
            }

            public void Dispose()
            {
                _index = -2;
                _currentElement = default;
            }

            public bool MoveNext()
            {
                Throw.CheckState( "The FIFOBuffer has changed.", _version == _b._version );

                if( _index == -2 ) return false;

                _index++;
                if( _index == _b._count )
                {
                    _index = -2;
                    _currentElement = default;
                    return false;
                }
                T[] array = _b._buffer;
                int actualIndex = _b._head + _index;
                int rollIdx = actualIndex - array.Length;
                _currentElement = array[rollIdx >= 0 ? rollIdx : actualIndex];
                return true;
            }

            public T Current
            {
                get
                {
                    Throw.CheckState( "Enumeration has not started or ended.", _index >= 0 );
                    return _currentElement!;
                }
            }

            object? IEnumerator.Current => Current; 

            void IEnumerator.Reset()
            {
                Throw.CheckState( "The FIFOBuffer has changed.", _version == _b._version );
                _index = -1;
                _currentElement = default;
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Gets the enumerator (from oldest to newest item).
        /// </summary>
        /// <returns>An enumerator.</returns>
        public Enumerator GetEnumerator() => new Enumerator( this );

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Overridden to display the current count of items and capacity for this buffer.
        /// </summary>
        /// <returns>Current count and capacity.</returns>
        public override string ToString() => $"Count = {_count} (Capacity = {_buffer.Length})";

    }
}
