#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\FIFOBuffer.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Simple implementation of a fixed size FIFO stack based on a circular buffer. 
    /// The .Net <see cref="Queue{T}"/>'s size increase as needed whereas this FIFO automatically loses the oldest items.
    /// Note that when <typeparamref name="T"/> is a reference type, null can be pushed and pop.
    /// This can easily be used as a LIFO stack thanks to <see cref="PopLast"/> and <see cref="PeekLast"/> methods.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    [DebuggerTypeProxy( typeof( Impl.CKReadOnlyCollectionDebuggerView<> ) )]
    public class FIFOBuffer<T> : ICKReadOnlyList<T>, ICKWritableCollector<T>
    {
        int _count;
        int _head;
        int _tail;
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
            _head = 0;
            _tail = 0;
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
                    throw new ArgumentException( "Capacity must be greater than or equal to zero.", "value" );

                var dst = new T[value];
                if( _count > 0 ) CopyTo( dst );
                _head = 0;
                if( _count > value )
                {
                    _count = value;
                    _tail = 0;
                }
                else _tail = _count;
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
                    _buffer[_head] = default( T );
                    if( ++_head == _buffer.Length ) _head = 0;
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
            int bufferIndex = _head;
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
        public T this[int index]
        {
            get
            {
                if( index < 0 || index >= _count ) throw new IndexOutOfRangeException();
                index += _head;
                int roll = index - _buffer.Length;
                return _buffer[roll >= 0 ? roll : index];
            }
        }

        /// <summary>
        /// Clears the internal buffer.
        /// </summary>
        public void Clear()
        {
            _count = 0;
            _head = 0;
            _tail = 0;
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
                _buffer[_tail++] = item;
                if( _tail == _buffer.Length ) _tail = 0;
                if( _count == _buffer.Length )
                {
                    if( ++_head == _buffer.Length ) _head = 0;
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
            if( _count == 0 ) throw new InvalidOperationException( R.FIFOBufferEmpty );
            var item = _buffer[_head];
            _buffer[_head] = default( T );
            if( ++_head == _buffer.Length ) _head = 0;
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
            if( _count == 0 ) throw new InvalidOperationException( R.FIFOBufferEmpty );
            if( --_tail < 0 ) _tail = _head + _count - 1;
            var item = _buffer[_tail];
            _buffer[_tail] = default( T );
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
            if( _count == 0 ) throw new InvalidOperationException( R.FIFOBufferEmpty );
            return _buffer[_head];
        }

        /// <summary>
        /// Gets the last item (the last one that has been <see cref="Push"/>).
        /// <see cref="Count"/> must be greater than 0 otherwise an exception is thrown.
        /// </summary>
        /// <returns>The last (newest) item.</returns>
        public T PeekLast()
        {
            if( _count == 0 ) throw new InvalidOperationException( R.FIFOBufferEmpty );
            int t = _tail-1;
            if( t < 0 ) t = _head + _count - 1;
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
                Array.Copy( _buffer, head, array, arrayIndex, count );
            }
            else
            {
                // We need two copies.
                // 1 - From head to the end.
                Array.Copy( _buffer, head, array, arrayIndex, afterHead );
                // 2 - From start to tail.
                Array.Copy( _buffer, 0, array, arrayIndex + afterHead, _tail );
            }
            return count;
        }

        /// <summary>
        /// Gets the enumerator (from oldest to newest item).
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            int bufferIndex = _head;
            for( int i = 0; i < _count; i++, bufferIndex++ )
            {
                if( bufferIndex == _buffer.Length ) bufferIndex = 0;
                yield return _buffer[bufferIndex];
            }
        }

        /// <summary>
        /// Creates an array that contains <see cref="Count"/> items.
        /// </summary>
        /// <returns>An array with the contained items.</returns>
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
        /// Overriden to display the current count of items and capacity for this buffer.
        /// </summary>
        /// <returns>Current count and capacity.</returns>
        public override string ToString()
        {
            return String.Format( "Count = {0} (Capacity = {1})", _count, _buffer.Length );
        }
    }
}
