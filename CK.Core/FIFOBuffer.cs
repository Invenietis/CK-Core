#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\FIFOBuffer.cs) is part of CiviKey. 
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
    /// Note that when <typeparamref name="T"/> is a reference type, null can be pushed and pop.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public class FIFOBuffer<T> : IReadOnlyList<T>, IWritableCollector<T> 
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
            return (item == null && default(T) == null ) || item is T ? IndexOf( (T)item ) : Int32.MinValue;
        }

        /// <summary>
        /// Gets the index of the given object.
        /// </summary>
        /// <param name="item">Object to find.</param>
        /// <returns>
        /// The index of the object or the bitwise complement of <see cref="Count"/> if not 
        /// found (that is a negative value, see <see cref="IReadOnlyList{T}.IndexOf"/>).
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
        /// Gets the element by index. Index 0 is the one returned by <see cref="Peek"/> (and the next one that will be returned by <see cref="Pop"/>).
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
                return _buffer[ roll >= 0 ? roll : index];
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
        /// </summary>
        /// <returns>The first (oldest) item.</returns>
        public T Pop()
        {
            if( _count == 0 ) throw new InvalidOperationException( "FIFOBuffer is empty." );
            var item = _buffer[_head];
            _buffer[_head] = default( T );
            if( ++_head == _buffer.Length ) _head = 0;
            _count--;
            return item;
        }

        /// <summary>
        /// Gets the first item (the one that has been <see cref="Push"/>ed first).
        /// </summary>
        /// <returns>The first (oldest) item.</returns>
        public T Peek()
        {
            if( _count == 0 ) throw new InvalidOperationException( "FIFOBuffer is empty." );
            return _buffer[_head];
        }

        bool IWritableCollector<T>.Add( T e )
        {
            Push( e );
            return true;
        }

        /// <summary>
        /// Copies as much possible items into the given array. 
        /// If the target array is too small to contain <see cref="Count"/> items, the newest ones
        /// are copied (the oldest, the ones that will <see cref="Pop"/> first, are skipped).
        /// </summary>
        /// <param name="array">Array that will contain the items.</param>
        /// <returns>Number of items copied.</returns>
        public int CopyTo( T[] array )
        {
            return CopyTo( array, 0 );
        }

        /// <summary>
        /// Copies as much possible items into the given array. 
        /// If the target array is too small to contain <see cref="Count"/> items, the newest ones
        /// are copied (the oldest, the ones that will <see cref="Pop"/> first, are skipped).
        /// </summary>
        /// <param name="array">Array that will contain the items.</param>
        /// <param name="arrayIndex">Index in <paramref name="array"/> where copy must start.</param>
        /// <returns>Number of items copied.</returns>
        public int CopyTo( T[] array, int arrayIndex )
        {
            return CopyTo( array, arrayIndex, array.Length - arrayIndex );
        }

        /// <summary>
        /// Copies as much possible items into the given array. 
        /// If <paramref name="count"/> is less than <see cref="Count"/>, the newest ones
        /// are copied (the oldest, the ones that will <see cref="Pop"/> first, are skipped).
        /// </summary>
        /// <param name="array">Array that will contain the items.</param>
        /// <param name="arrayIndex">Index in <paramref name="array"/> where copy must start.</param>
        /// <param name="count">Number of items to copy.</param>
        /// <returns>Number of items copied.</returns>
        public int CopyTo( T[] array, int arrayIndex, int count )
        {
            if( count < 0 ) throw new ArgumentException();
            
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
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator (from oldest to newest item).</returns>
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

    }
}
