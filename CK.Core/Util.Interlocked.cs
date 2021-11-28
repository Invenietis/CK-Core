using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core
{
    public static partial class Util
    {
        /// <summary>
        /// Thread-safe way to set any nullable reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="transformer">Function that knows how to obtain the desired object from the current one. This function may be called more than once.</param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        public static T? InterlockedNullableSet<T>( ref T? target, Func<T?, T?> transformer ) where T : class
        {
            Guard.IsNotNull( transformer, nameof( transformer ) );
            T? current = target;
            T? newOne = transformer( current );
            if( Interlocked.CompareExchange( ref target, newOne, current ) != current )
            {
                var sw = new SpinWait();
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
        /// Thread-safe way to set a non nullable reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="transformer">Function that knows how to obtain the desired object from the current one. This function may be called more than once.</param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T InterlockedSet<T>( ref T target, Func<T, T> transformer ) where T : class
        {
            Guard.IsNotNull( target, nameof( target ) );
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            return InterlockedNullableSet( ref target, transformer );
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Thread-safe way to set any reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <typeparam name="TArg">Type of the transformer argument parameter.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="a">Argument of the transformer.</param>
        /// <param name="transformer">
        /// Function that knows how to obtain the desired object from the current one. This function may be called more than once.
        /// </param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        public static T? InterlockedNullableSet<T, TArg>( ref T? target, TArg a, Func<T?, TArg, T?> transformer ) where T : class
        {
            Guard.IsNotNull( transformer, nameof( transformer ) );
            T? current = target;
            T? newOne = transformer( current, a );
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
        /// Thread-safe way to set a non nullable reference type. Uses <see cref="Interlocked.CompareExchange{T}"/> and <see cref="SpinWait"/>.
        /// </summary>
        /// <typeparam name="T">Any reference type.</typeparam>
        /// <typeparam name="TArg">Type of the transformer argument parameter.</typeparam>
        /// <param name="target">Reference (address) to set.</param>
        /// <param name="a">Argument of the transformer.</param>
        /// <param name="transformer">
        /// Function that knows how to obtain the desired object from the current one. This function may be called more than once.
        /// </param>
        /// <returns>The object that has actually been set. Note that it may differ from the "current" target value if another thread already changed it.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T InterlockedSet<T, TArg>( ref T target, TArg a, Func<T, TArg, T> transformer ) where T : class
        {
            Guard.IsNotNull( target, nameof( target ) );
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            return InterlockedNullableSet( ref target, a, transformer );
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Atomically removes an item from a non nullable array reference: it must be and will be <see cref="Array.Empty{T}()"/>
        /// when there's no items.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array.</param>
        /// <param name="o">Item to remove.</param>
        /// <returns>The array without the item. Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedRemove<T>( ref T[] items, T o )
        {
            return InterlockedSet( ref items, o, ( current, item ) =>
            {
                if( current.Length == 0 ) return Array.Empty<T>();
                int idx = Array.IndexOf( current, item );
                if( idx < 0 ) return current;
                if( current.Length == 1 ) return Array.Empty<T>();
                var newArray = new T[current.Length - 1];
                System.Array.Copy( current, 0, newArray, 0, idx );
                System.Array.Copy( current, idx + 1, newArray, idx, newArray.Length - idx );
                return newArray;
            } );
        }

        /// <summary>
        /// Atomically removes the first item from a non nullable array reference that matches a predicate.
        /// The referenced array must be and will be <see cref="Array.Empty{T}()"/> when there's no items.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array.</param>
        /// <param name="predicate">Predicate that identifies the item to remove.</param>
        /// <returns>The array containing the new item. Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedRemove<T>( ref T[] items, Func<T, bool> predicate )
        {
            if( predicate == null ) throw new ArgumentNullException( nameof( predicate ) );
            return InterlockedSet( ref items, predicate, ( current, p ) =>
            {
                if( current.Length == 0 ) return Array.Empty<T>();
                int idx = current.IndexOf( p );
                if( idx < 0 ) return current;
                if( current.Length == 1 ) return Array.Empty<T>();
                var newArray = new T[current.Length - 1];
                System.Array.Copy( current, 0, newArray, 0, idx );
                System.Array.Copy( current, idx + 1, newArray, idx, newArray.Length - idx );
                return newArray;
            } );
        }

        /// <summary>
        /// Atomically removes one or more items from a non nullable array reference that match a predicate.
        /// The referenced array must be and will be <see cref="Array.Empty{T}()"/> when there's no items.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array.</param>
        /// <param name="predicate">Predicate that identifies items to remove.</param>
        /// <returns>The cleaned array (may be the empty one). Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedRemoveAll<T>( ref T[] items, Func<T, bool> predicate )
        {
            Guard.IsNotNull( predicate, nameof( predicate ) );
            return InterlockedSet( ref items, predicate, ( current, p ) =>
            {
                if( current.Length == 0 ) return current;
                for( int i = 0; i < current.Length; ++i )
                {
                    if( !p( current[i] ) )
                    {
                        List<T> collector = new List<T>
                        {
                            current[i]
                        };
                        while( ++i < current.Length )
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
        /// Atomically adds an item to a non nullable array reference if it does not already exist in the array (uses <see cref="Array.IndexOf{T}(T[], T)"/>).
        /// The referenced array must be and will be <see cref="Array.Empty{T}()"/> when there's no items.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array.</param>
        /// <param name="o">The item to insert at position 0 (if <paramref name="prepend"/> is true) or at the end only if it does not already appear in the array.</param>
        /// <param name="prepend">True to insert the item at the head of the array (index 0) instead of at its end.</param>
        /// <returns>The array containing the new item. Note that it may differ from the "current" items content since another thread may have already changed it.</returns>
        public static T[] InterlockedAddUnique<T>( ref T[] items, T o, bool prepend = false )
        {
            return InterlockedSet( ref items, o, ( current, item ) =>
            {
                if( current.Length == 0 ) return new T[] { item };
                if( Array.IndexOf( current, item ) >= 0 ) return current;
                T[] newArray = new T[current.Length + 1];
                Array.Copy( current, 0, newArray, prepend ? 1 : 0, current.Length );
                newArray[prepend ? 0 : current.Length] = item;
                return newArray;
            } );
        }

        /// <summary>
        /// Atomically adds an item to a non nullable array reference.
        /// The referenced array must be and will be <see cref="Array.Empty{T}()"/> when there's no items.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <param name="items">Reference (address) of the array.</param>
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
        /// Atomically adds an item to an existing array if no existing item satisfies a condition.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <typeparam name="TItem">Type of the item to add: can be any specialization of T.</typeparam>
        /// <typeparam name="TArg">Type of the argument provided to factory and tester functions.</typeparam>
        /// <param name="items">Reference (address) of the array.</param>
        /// <param name="arg">Argument provided to tester and factory functions.</param>
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
        public static T[] InterlockedAdd<T, TArg, TItem>( ref T[] items, TArg arg, Func<TItem, TArg, bool> tester, Func<TArg, TItem> factory, bool prepend = false ) where TItem : T
        {
            Guard.IsNotNull( tester, nameof( tester ) );
            Guard.IsNotNull( factory, nameof( factory ) );
            TItem newE = default!;
            bool needFactory = true;
            return InterlockedSet( ref items, arg, ( current, arg ) =>
            {
                T[] newArray;
                foreach( var e in current )
                    if( e is TItem item && tester( item, arg ) ) return current;
                if( needFactory )
                {
                    needFactory = false;
                    newE = factory( arg );
                    if( !tester( newE, arg ) ) throw new InvalidOperationException( Impl.CoreResources.FactoryTesterMismatch );
                }
                if( current.Length == 0 ) newArray = new T[] { newE };
                else
                {
                    newArray = new T[current.Length + 1];
                    Array.Copy( current, 0, newArray, prepend ? 1 : 0, current.Length );
                    newArray[prepend ? 0 : current.Length] = newE;
                }
                return newArray;
            } );
        }

        /// <summary>
        /// Atomically adds an item to an existing array if no existing item satisfies a condition.
        /// </summary>
        /// <typeparam name="T">Type of the item array.</typeparam>
        /// <typeparam name="TItem">Type of the item to add: can be any specialization of T.</typeparam>
        /// <param name="items">Reference (address) of the array.</param>
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
            Guard.IsNotNull( tester, nameof( tester ) );
            Guard.IsNotNull( factory, nameof( factory ) );
            TItem newE = default!;
            bool needFactory = true;
            return InterlockedSet( ref items, current =>
            {
                T[] newArray;
                foreach( var e in current )
                    if( e is TItem item && tester( item ) ) return current;
                if( needFactory )
                {
                    needFactory = false;
                    newE = factory();
                    if( !tester( newE ) ) throw new InvalidOperationException( Impl.CoreResources.FactoryTesterMismatch );
                }
                if( current.Length == 0 ) newArray = new T[] { newE };
                else
                {
                    newArray = new T[current.Length + 1];
                    Array.Copy( current, 0, newArray, prepend ? 1 : 0, current.Length );
                    newArray[prepend ? 0 : current.Length] = newE;
                }
                return newArray;
            } );
        }

    }
}
