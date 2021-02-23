using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Utility class.
    /// Offers useful functions, constants, singletons and delegates.
    /// </summary>
    public static partial class Util
    {
        /// <summary>
        /// Represents the smallest possible value for a <see cref="DateTime"/> in <see cref="DateTimeKind.Utc"/>.         
        /// </summary>
        static public readonly DateTime UtcMinValue = new DateTime( 0L, DateTimeKind.Utc );

        /// <summary>
        /// Represents the largest possible value for a <see cref="DateTime"/> in <see cref="DateTimeKind.Utc"/>.         
        /// </summary>
        static public readonly DateTime UtcMaxValue = new DateTime( 0x2bca2875f4373fffL, DateTimeKind.Utc );

        [Obsolete("Use the standard Array.Empty<T>()",true)]
        public static class Array
        {
            /// <summary>
            /// Gets an empty array for a type.
            /// </summary>
            /// <typeparam name="T">Type of the array items.</typeparam>
            /// <returns>An empty array.</returns>
            public static T[] Empty<T>()
            {
                return System.Array.Empty<T>();
            }
        }

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

        class DisposableAction : IDisposable
        {
            public Action A;
            public void Dispose()
            {
                Action a = A;
                if( a != null && Interlocked.CompareExchange( ref A, null, a ) == a ) a();
            }
        }

        /// <summary>
        /// Wraps an action in a <see cref="IDisposable"/> interface
        /// Can be safely called if <paramref name="a"/> is null (the dispose call will do nothing) and in multi threaded context:
        /// the call to action will be done once and only once by the first call to dispose.
        /// See <see cref="ActionDispose"/> to adapt an IDisposable interface to an <see cref="Action"/>.
        /// </summary>
        /// <param name="a">The action to call when <see cref="IDisposable.Dispose"/> is called.</param>
        public static IDisposable CreateDisposableAction( Action a )
        {
            return new DisposableAction() { A = a };
        }

        class VoidDisposable : IDisposable { public void Dispose() { } }

        /// <summary>
        /// A void, immutable, <see cref="IDisposable"/> that does absolutely nothing.
        /// </summary>
        public static readonly IDisposable EmptyDisposable = new VoidDisposable();

        /// <summary>
        /// Unix Epoch (1st of January 1970).
        /// </summary>
        public static DateTime UnixEpoch  = new DateTime(1970,1,1);

        /// <summary>
        /// Sql Server Epoch (1st of January 1900): this is the 0 legacy datetime.
        /// </summary>
        public static DateTime SqlServerEpoch  = new DateTime(1900,1,1);

        /// <summary>
        /// The 0.0.0.0 Version.
        /// </summary>
        public static Version EmptyVersion = new Version( 0, 0, 0, 0 );

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
        /// Binary search implementation that relies on a <see cref="Comparison{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="startIndex">The starting index in the list.</param>
        /// <param name="length">The number of elements to consider in the list.</param>
        /// <param name="value">The value to locate.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <returns>Same as <see cref="System.Array.BinarySearch(System.Array, object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
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
        /// <returns>Same as <see cref="System.Array.BinarySearch(System.Array, object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
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
        /// <returns>Same as <see cref="System.Array.BinarySearch(System.Array, object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
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
        /// <returns>Same as <see cref="System.Array.BinarySearch(System.Array, object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
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
        /// Atomically removes an item in an array.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array. Can be null.</param>
        /// <param name="o">Item to remove.</param>
        /// <returns>The array without the item. Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedRemove<T>( ref T[] items, T o )
        {
            return InterlockedSet( ref items, o, ( current, item ) =>
            {
                if( current == null || current.Length == 0 ) return current;
                int idx = System.Array.IndexOf( current, item );
                if( idx < 0 ) return current;
                if( current.Length == 1 ) return System.Array.Empty<T>();
                var newArray = new T[current.Length - 1];
                System.Array.Copy( current, 0, newArray, 0, idx );
                System.Array.Copy( current, idx + 1, newArray, idx, newArray.Length - idx );
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
        public static T[] InterlockedRemove<T>( ref T[] items, Func<T, bool> predicate )
        {
            if( predicate == null ) throw new ArgumentNullException( "predicate" );
            return InterlockedSet( ref items, predicate, ( current, p ) =>
            {
                if( current == null || current.Length == 0 ) return current;
                int idx = current.IndexOf( p );
                if( idx < 0 ) return current;
                if( current.Length == 1 ) return System.Array.Empty<T>();
                var newArray = new T[current.Length - 1];
                System.Array.Copy( current, 0, newArray, 0, idx );
                System.Array.Copy( current, idx + 1, newArray, idx, newArray.Length - idx );
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
        public static T[] InterlockedRemoveAll<T>( ref T[] items, Func<T, bool> predicate )
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
                        List<T> collector = new List<T>
                        {
                            current[i]
                        };
                        while( ++i < len )
                        {
                            if( !p( current[i] ) ) collector.Add( current[i] );
                        }
                        return collector.ToArray();
                    }
                }
                return System.Array.Empty<T>();
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
                if( oldItems == null || oldItems.Length == 0 ) return new T[] { item };
                if( System.Array.IndexOf( oldItems, item ) >= 0 ) return oldItems;
                T[] newArray = new T[oldItems.Length + 1];
                System.Array.Copy( oldItems, 0, newArray, prepend ? 1 : 0, oldItems.Length );
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
                if( oldItems == null || oldItems.Length == 0 ) return new T[] { item };
                T[] newArray = new T[oldItems.Length + 1];
                System.Array.Copy( oldItems, 0, newArray, prepend ? 1 : 0, oldItems.Length );
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
            TItem newE = default;
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
                    if( !tester( newE ) ) throw new InvalidOperationException( Impl.CoreResources.FactoryTesterMismatch );
                }
                if( oldItems == null || oldItems.Length == 0 ) newArray = new T[] { newE };
                else
                {
                    newArray = new T[oldItems.Length + 1];
                    System.Array.Copy( oldItems, 0, newArray, prepend ? 1 : 0, oldItems.Length );
                    newArray[prepend ? 0 : oldItems.Length] = newE;
                }
                return newArray;
            } );
        }

        #endregion

    }
}
