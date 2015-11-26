#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Util.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace CK.Core
{
    /// <summary>
    /// Utility class.
    /// Offers useful functions, constants, singletons and delegates.
    /// </summary>
    static public partial class Util
	{
        /// <summary>
        /// Gets 1900, January the 1st. This is the 'zero' of Sql Server datetime and smalldatetime
        /// types.
        /// </summary>
        static public readonly DateTime SqlServerEpoch = new DateTime( 1900, 1, 1, 0, 0, 0, DateTimeKind.Utc );

        /// <summary>
        /// Gets 1970, January the 1st. This is the 'zero' of numerous date/time system
        /// like Unix file system or javascript.
        /// </summary>
        static public readonly DateTime UnixEpoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );

        /// <summary>
        /// Represents the smallest possible value for a <see cref="DateTime"/> in <see cref="DateTimeKind.Utc"/>.         
        /// </summary>
        static public readonly DateTime UtcMinValue = new DateTime( 0L, DateTimeKind.Utc );

        /// <summary>
        /// Represents the largest possible value for a <see cref="DateTime"/> in <see cref="DateTimeKind.Utc"/>.         
        /// </summary>
        static public readonly DateTime UtcMaxValue = new DateTime( 0x2bca2875f4373fffL, DateTimeKind.Utc );


        /// <summary>
        /// Centralized <see cref="IDisposable.Dispose"/> action call: it adapts an <see cref="IDisposable"/> interface to an <see cref="Action"/>.
        /// Can be safely called if <paramref name="obj"/> is null. 
        /// See <see cref="CreateDisposableAction"/> to wrap an action in a <see cref="IDisposable"/> interface.
        /// </summary>
        /// <param name="obj">The disposable object to dispose (can be null).</param>
        public static void ActionDispose( IDisposable obj )
        {
            if( obj != null ) obj.Dispose();
        }

        /// <summary>
        /// Wraps an action in a <see cref="IDisposable"/> interface
        /// Can be safely called if <paramref name="a"/> is null (the dispose call will do nothing).
        /// See <see cref="ActionDispose"/> to adapt an <see cref="IDisposable"/> interface to an <see cref="Action"/>.
        /// </summary>
        /// <param name="a">The action to call when <see cref="IDisposable.Dispose"/> is called.</param>
        public static IDisposable CreateDisposableAction( Action a )
        {
            return new DisposableAction() { A = a };
        }

        class DisposableAction : IDisposable
        {
            public Action A;
            public void Dispose()
            {
                Action a = A;
                if( a != null )
                {
                    a();
                    A = null;
                }
            }
        }

        /// <summary>
        /// Centralized void action call for any type. 
        /// This method is one of the safest method never written in the world. 
        /// It does absolutely nothing.
        /// </summary>
        /// <param name="obj">Any object.</param>
        [ExcludeFromCodeCoverage]
        public static void ActionVoid<T>( T obj ) 
        { 
        }

        /// <summary>
        /// Centralized void action call for any pair of types. 
        /// This method is one of the safest method never written in the world. 
        /// It does absolutely nothing.
        /// </summary>
        /// <param name="o1">Any object.</param>
        /// <param name="o2">Any object.</param>
        [ExcludeFromCodeCoverage]
        public static void ActionVoid<T1, T2>( T1 o1, T2 o2 )
        {
        }

        /// <summary>
        /// Centralized void action call for any 3 types. 
        /// This method is one of the safest method never written in the world. 
        /// It does absolutely nothing.
        /// </summary>
        /// <param name="o1">Any object.</param>
        /// <param name="o2">Any object.</param>
        /// <param name="o3">Any object.</param>
        [ExcludeFromCodeCoverage]
        public static void ActionVoid<T1, T2, T3>( T1 o1, T2 o2, T3 o3 )
        {
        }

        /// <summary>
        /// Centralized identity function for any type.
        /// </summary>
        /// <typeparam name="T">Type of the function parameter and return value.</typeparam>
        /// <param name="value">Any value returned unchanged.</param>
        /// <returns>The <paramref name="value"/> provided is returned as-is.</returns>
        public static T FuncIdentity<T>( T value )
        {
            return value;
        }


        /// <summary>
        /// Wraps an action in a predicate that returns always the provided result.
        /// </summary>
        /// <typeparam name="T">The type of the action's parameter.</typeparam>
        /// <param name="this">The action (a method that accepts <typeparamref name="T"/> as its only argument).</param>
        /// <param name="result">result that will be returned.</param>
        /// <returns>A predicate that performs the action and returns true.</returns>
        static public Func<T,bool> ToPredicate<T>( this Action<T> @this, bool result )
        {
            return delegate( T o ) { @this( o ); return result; };
        }

        /// <summary>
        /// Wraps an action in a predicate that returns always true.
        /// </summary>
        /// <typeparam name="T">The type of the action's parameter.</typeparam>
        /// <param name="a">The action (a method that accepts <typeparamref name="T"/> as its only argument).</param>
        /// <returns>A predicate that performs the action and returns true.</returns>
        static public Func<T, bool> AlwaysTrue<T>( Action<T> a )
        {
            return delegate( T o ) { a( o ); return true; };
        }

        /// <summary>
        /// Wraps an action in a predicate that returns always false.
        /// </summary>
        /// <typeparam name="T">The type of the action's parameter.</typeparam>
        /// <param name="a">The action (a method that accepts <typeparamref name="T"/> as its only argument).</param>
        /// <returns>A predicate that performs the action and returns false.</returns>
        static public Func<T, bool> AlwaysFalse<T>( Action<T> a )
        {
            return delegate( T o ) { a( o ); return false; };
        }

#if net40
        /// <summary>
        /// Binary search implementation that relies on a <see cref="Comparison{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <param name="array">An array of elements.</param>
        /// <param name="startIndex">The starting index in the array.</param>
        /// <param name="length">The number of elements to consider in the array.</param>
        /// <param name="value">The value to locate.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(Array,object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        public static int BinarySearch<T>( T[] array, int startIndex, int length, T value, Comparison<T> comparison )
        {
            int low = startIndex;
            int high = (startIndex + length) - 1;
            while( low <= high )
            {
                int mid = low + ((high - low) >> 1);
                int cmp = comparison( array[mid], value );
                if( cmp == 0 ) return mid;
                if( cmp < 0 ) low = mid + 1;
                else high = mid - 1;
            }
            return ~low;
        }

        /// <summary>
        /// Binary search implementation that relies on an extended comparer: a function that knows how to 
        /// compare the elements of the array to a key of another type.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <param name="array">An array of elements.</param>
        /// <param name="startIndex">The starting index in the array.</param>
        /// <param name="length">The number of elements to consider in the array.</param>
        /// <param name="key">The value of the key.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(Array,object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        public static int BinarySearch<T, TKey>( T[] array, int startIndex, int length, TKey key, Func<T, TKey, int> comparison )
        {
            int low = startIndex;
            int high = (startIndex + length) - 1;
            while( low <= high )
            {
                int mid = low + ((high - low) >> 1);
                int cmp = comparison( array[mid], key );
                if( cmp == 0 ) return mid;
                if( cmp < 0 ) low = mid + 1;
                else high = mid - 1;
            }
            return ~low;
        }
#endif

        /// <summary>
        /// Binary search implementation that relies on a <see cref="Comparison{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="startIndex">The starting index in the list.</param>
        /// <param name="length">The number of elements to consider in the list.</param>
        /// <param name="value">The value to locate.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(Array,object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        public static int BinarySearch<T>( IReadOnlyList<T> sortedList, int startIndex, int length, T value, Comparison<T> comparison )
        {
            int low = startIndex;
            int high = (startIndex + length) - 1;
            while( low <= high )
            {
                int mid = low + ((high - low) >> 1);
                int cmp = comparison( sortedList[mid], value );
                if( cmp == 0 ) return mid;
                if( cmp < 0 ) low = mid + 1;
                else high = mid - 1;
            }
            return ~low;
        }

        /// <summary>
        /// Binary search implementation that relies on an extended comparer: a function that knows how to 
        /// compare the elements of the list to a key of another type.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="startIndex">The starting index in the list.</param>
        /// <param name="length">The number of elements to consider in the list.</param>
        /// <param name="key">The value of the key.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(Array,object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        public static int BinarySearch<T, TKey>( IReadOnlyList<T> sortedList, int startIndex, int length, TKey key, Func<T, TKey, int> comparison )
        {
            int low = startIndex;
            int high = (startIndex + length) - 1;
            while( low <= high )
            {
                int mid = low + ((high - low) >> 1);
                int cmp = comparison( sortedList[mid], key );
                if( cmp == 0 ) return mid;
                if( cmp < 0 ) low = mid + 1;
                else high = mid - 1;
            }
            return ~low;
        }

        /// <summary>
        /// Binary search implementation that relies on <see cref="IComparable{TValue}"/> implemented by the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements. It must implement <see cref="IComparable{TValue}"/>.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="startIndex">The starting index in the list.</param>
        /// <param name="length">The number of elements to consider in the list.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(Array,object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        public static int BinarySearch<T, TValue>( IReadOnlyList<T> sortedList, int startIndex, int length, TValue value ) where T : IComparable<TValue>
        {
            int low = startIndex;
            int high = (startIndex + length) - 1;
            while( low <= high )
            {
                int mid = low + ((high - low) >> 1);
                int cmp = sortedList[mid].CompareTo( value );
                if( cmp == 0 ) return mid;
                if( cmp < 0 ) low = mid + 1;
                else high = mid - 1;
            }
            return ~low;
        }

        /// <summary>
        /// Binary search implementation that relies on <see cref="IComparable{TValue}"/> implemented by the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements. It must implement <see cref="IComparable{TValue}"/>.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(Array,object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        public static int BinarySearch<T, TValue>( IReadOnlyList<T> sortedList, TValue value ) where T : IComparable<TValue>
        {
            return BinarySearch( sortedList, 0, sortedList.Count, value );
        }

        #region Interlocked helpers.

        /// <summary>
        /// Thread-safe way to set any reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="transformer">Function that knows how to obtain the desired object from the current one. This function may be called more than once.</param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        public static T InterlockedSet<T>( ref T target, Func<T, T> transformer ) where T : class
        {
            T current = target;
            T newOne = transformer( current );
            if( Interlocked.CompareExchange( ref target, newOne, current ) != current )
            {
                // After a lot of readings of msdn and internet, I use the SpinWait struct...
                // This is the recommended way, so...
                SpinWait sw = new SpinWait();
                do
                {
                    sw.SpinOnce();
                    current = target;
                }
                while( Interlocked.CompareExchange( ref target, (newOne = transformer( current )), current ) != current );
            }
            return newOne;
        }

        /// <summary>
        /// Thread-safe way to set any reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <typeparam name="TArg">Type of the first parameter.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="a">Argument of the transformer.</param>
        /// <param name="transformer">Function that knows how to obtain the desired object from the current one. This function may be called more than once.</param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        public static T InterlockedSet<T, TArg>( ref T target, TArg a, Func<T, TArg, T> transformer ) where T : class
        {
            T current = target;
            T newOne = transformer( current, a );
            if( Interlocked.CompareExchange( ref target, newOne, current ) != current )
            {
                SpinWait sw = new SpinWait();
                do
                {
                    sw.SpinOnce();
                    current = target;
                }
                while( Interlocked.CompareExchange( ref target, (newOne = transformer( current, a )), current ) != current );
            }
            return newOne;
        }

        /// <summary>
        /// Thread-safe way to set any reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <typeparam name="TArg1">Type of the first parameter.</typeparam>
        /// <typeparam name="TArg2">Type of the second parameter.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="a1">First argument of the transformer.</param>
        /// <param name="a2">Second argument of the transformer.</param>
        /// <param name="transformer">Function that knows how to obtain the desired object from the current one. This function may be called more than once.</param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        public static T InterlockedSet<T, TArg1, TArg2>( ref T target, TArg1 a1, TArg2 a2, Func<T, TArg1, TArg2, T> transformer ) where T : class
        {
            // Use closure here to reuse InterlockedSet implementation.
            return InterlockedSet( ref target, t => transformer( t, a1, a2 ) );
        }

        /// <summary>
        /// Thread-safe way to set any reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <typeparam name="TArg1">Type of the first parameter.</typeparam>
        /// <typeparam name="TArg2">Type of the second parameter.</typeparam>
        /// <typeparam name="TArg3">Type of the third parameter.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="a1">First argument of the transformer.</param>
        /// <param name="a2">Second argument of the transformer.</param>
        /// <param name="a3">Third argument of the transformer.</param>
        /// <param name="transformer">Function that knows how to obtain the desired object from the current one. This function may be called more than once.</param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        public static T InterlockedSet<T, TArg1, TArg2, TArg3>( ref T target, TArg1 a1, TArg2 a2, TArg3 a3, Func<T, TArg1, TArg2, TArg3, T> transformer ) where T : class
        {
            // Use closure here to reuse InterlockedSet implementation.
            return InterlockedSet( ref target, t => transformer( t, a1, a2, a3 ) );
        }

        /// <summary>
        /// Thread-safe way to set any reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <typeparam name="TArg1">Type of the first parameter.</typeparam>
        /// <typeparam name="TArg2">Type of the second parameter.</typeparam>
        /// <typeparam name="TArg3">Type of the third parameter.</typeparam>
        /// <typeparam name="TArg4">Type of the fourth parameter.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="a1">First argument of the transformer.</param>
        /// <param name="a2">Second argument of the transformer.</param>
        /// <param name="a3">Third argument of the transformer.</param>
        /// <param name="a4">Fourth argument of the transformer.</param>
        /// <param name="transformer">Function that knows how to obtain the desired object from the current one. This function may be called more than once.</param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        public static T InterlockedSet<T, TArg1, TArg2, TArg3, TArg4>( ref T target, TArg1 a1, TArg2 a2, TArg3 a3, TArg4 a4, Func<T, TArg1, TArg2, TArg3, TArg4, T> transformer ) where T : class
        {
            // Use closure here to reuse InterlockedSet implementation.
            return InterlockedSet( ref target, t => transformer( t, a1, a2, a3, a4 ) );
        }

        /// <summary>
        /// Thread-safe way to set any reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <typeparam name="TArg1">Type of the first parameter.</typeparam>
        /// <typeparam name="TArg2">Type of the second parameter.</typeparam>
        /// <typeparam name="TArg3">Type of the third parameter.</typeparam>
        /// <typeparam name="TArg4">Type of the fourth parameter.</typeparam>
        /// <typeparam name="TArg5">Type of the fifth parameter.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="a1">First argument of the transformer.</param>
        /// <param name="a2">Second argument of the transformer.</param>
        /// <param name="a3">Third argument of the transformer.</param>
        /// <param name="a4">Fourth argument of the transformer.</param>
        /// <param name="a5">Fifth argument of the transformer.</param>
        /// <param name="transformer">Function that knows how to obtain the desired object from the current one. This function may be called more than once.</param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        public static T InterlockedSet<T, TArg1, TArg2, TArg3, TArg4, TArg5>( ref T target, TArg1 a1, TArg2 a2, TArg3 a3, TArg4 a4, TArg5 a5, Func<T, TArg1, TArg2, TArg3, TArg4, TArg5, T> transformer ) where T : class
        {
            // Use closure here to reuse InterlockedSet implementation.
            return InterlockedSet( ref target, t => transformer( t, a1, a2, a3, a4, a5 ) );
        }

        /// <summary>
        /// Atomically removes an item in an array.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array. Can be null.</param>
        /// <param name="o">Item to remove.</param>
        /// <returns>The array containing the new item. Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedRemove<T>( ref T[] items, T o )
        {
            return InterlockedSet( ref items, o, ( current, item ) =>
            {
                if( current == null || current.Length == 0 ) return current;
                int idx = Array.IndexOf( current, item );
                if( idx < 0 ) return current;
                if( current.Length == 1 ) return Util.EmptyArray<T>.Empty;
                var newArray = new T[current.Length - 1];
                Array.Copy( current, 0, newArray, 0, idx );
                Array.Copy( current, idx + 1, newArray, idx, newArray.Length - idx );
                return newArray;
            } );
        }

        /// <summary>
        /// Atomically removes the first item from an array that matches a predicate.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array. Can be null.</param>
        /// <param name="predicate">Predicate that identifies the item to remove.</param>
        /// <returns>The array containing the new item. Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedRemove<T>( ref T[] items, Func<T,bool> predicate )
        {
            if( predicate == null ) throw new ArgumentNullException( "predicate" );
            return InterlockedSet( ref items, predicate, ( current, p ) =>
            {
                if( current == null || current.Length == 0 ) return current;
                int idx = current.IndexOf( p );
                if( idx < 0 ) return current;
                if( current.Length == 1 ) return Util.EmptyArray<T>.Empty;
                var newArray = new T[current.Length - 1];
                Array.Copy( current, 0, newArray, 0, idx );
                Array.Copy( current, idx + 1, newArray, idx, newArray.Length - idx );
                return newArray;
            } );
        }

        /// <summary>
        /// Atomically removes one or more items from an array that match a predicate.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array. Can be null.</param>
        /// <param name="predicate">Predicate that identifies items to remove.</param>
        /// <returns>The array containing the new item. Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedRemoveAll<T>( ref T[] items, Func<T,bool> predicate )
        {
            if( predicate == null ) throw new ArgumentNullException( "predicate" );
            return InterlockedSet( ref items, predicate, ( current, p ) =>
            {
                int len;
                if( current == null || (len = current.Length) == 0 ) return current;
                for( int i = 0; i < len; ++i )
                {
                    if( !p( current[i] ) )
                    {
                        List<T> collector = new List<T>();
                        collector.Add( current[i] );
                        while( ++i < len )
                        {
                            if( !p( current[i] ) ) collector.Add( current[i] );
                        }
                        return collector.ToArray();
                    }
                }
                return Util.EmptyArray<T>.Empty;                
            } );
        }

        /// <summary>
        /// Atomically adds an item to an array (that can be null) if it does not already exist in the array.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array. Can be null.</param>
        /// <param name="o">The item to insert at position 0 (if <paramref name="prepend"/> is true) or at the end only if it does not already appear in the array.</param>
        /// <param name="prepend">True to insert the item at the head of the array (index 0) instead of at its end.</param>
        /// <returns>The array containing the new item. Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedAddUnique<T>( ref T[] items, T o, bool prepend = false )
        {
            return InterlockedSet( ref items, o, ( oldItems, item ) =>
            {
                if( oldItems == null || oldItems.Length == 0 ) return new T[]{ item };
                if( Array.IndexOf( oldItems, item ) >= 0 ) return oldItems;
                T[] newArray = new T[oldItems.Length + 1];
                Array.Copy( oldItems, 0, newArray, prepend ? 1 : 0, oldItems.Length );
                newArray[prepend ? 0 : oldItems.Length] = item;
                return newArray;
            } );
        }

        /// <summary>
        /// Atomically adds an item to an array (that can be null).
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array. Can be null.</param>
        /// <param name="o">The item to insert at position 0 (if <paramref name="prepend"/> is true) or at the end.</param>
        /// <param name="prepend">True to insert the item at the head of the array (index 0) instead of at its end.</param>
        /// <returns>The array containing the new item. Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedAdd<T>( ref T[] items, T o, bool prepend = false )
        {
            return InterlockedSet( ref items, o, ( oldItems, item ) =>
            {
                if( oldItems == null || oldItems.Length == 0 ) return new T[]{ item };
                T[] newArray = new T[oldItems.Length + 1];
                Array.Copy( oldItems, 0, newArray, prepend ? 1 : 0, oldItems.Length );
                newArray[prepend ? 0 : oldItems.Length] = item;
                return newArray;
            } );
        }

        /// <summary>
        /// Atomically adds an item to an existing array (that can be null) if no existing item satisfies a condition.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <typeparam name="TItem">Type of the item to add: can be any specialization of T.</typeparam>
        /// <param name="items">Reference (address) of the array. Can be null.</param>
        /// <param name="tester">Predicate that must be satisfied for at least one existing item.</param>
        /// <param name="factory">Factory that will be called if no existing item satisfies <paramref name="tester"/>. It will be called only once if needed.</param>
        /// <param name="prepend">True to insert the item at the head of the array (index 0) instead of at its end.</param>
        /// <returns>
        /// The array containing the an item that satisfies the tester function. 
        /// Note that it may differ from the "current" items content since another thread may have already changed it.
        /// </returns>
        /// <remarks>
        /// The factory function MUST return an item that satisfies the tester function otherwise a <see cref="InvalidOperationException"/> is thrown.
        /// </remarks>
        public static T[] InterlockedAdd<T, TItem>( ref T[] items, Func<TItem, bool> tester, Func<TItem> factory, bool prepend = false ) where TItem : T
        {
            if( tester == null ) throw new ArgumentNullException( "tester" );
            if( factory == null ) throw new ArgumentNullException( "factory" );
            TItem newE = default(TItem);
            bool needFactory = true;
            return InterlockedSet( ref items, oldItems =>
            {
                T[] newArray;
                if( oldItems != null )
                    foreach( var e in oldItems )
                        if( e is TItem && tester( (TItem)e ) ) return oldItems;
                if( needFactory )
                {
                    needFactory = false;
                    newE = factory();
                    if( !tester( newE ) ) throw new InvalidOperationException( R.FactoryTesterMismatch );
                }
                if( oldItems == null || oldItems.Length == 0 ) newArray = new T[] { newE };
                else
                {
                    newArray = new T[oldItems.Length + 1];
                    Array.Copy( oldItems, 0, newArray, prepend ? 1 : 0, oldItems.Length );
                    newArray[prepend ? 0 : oldItems.Length] = newE;
                }
                return newArray;
            } );
        }

        #endregion

    }
}
