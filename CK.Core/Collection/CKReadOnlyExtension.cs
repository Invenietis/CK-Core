#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Collection\CKReadOnlyExtension.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="ICKReadOnlyCollection{T}"/>, <see cref="ICKReadOnlyList{T}"/> and <see cref="ICKReadOnlyUniqueKeyedCollection{T,TKey}"/>.
    /// </summary>
    public static class CKReadOnlyExtension
    {
        /// <summary>
        /// Gets the item with the associated key, forgetting the exists out parameter in <see cref="ICKReadOnlyUniqueKeyedCollection{T,TKey}.GetByKey(TKey,out bool)"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the collection.</typeparam>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <param name="coll">Keyed collection of elements.</param>
        /// <param name="key">The item key.</param>
        /// <returns>The item that matches the key, default(T) if the key can not be found.</returns>
        static public T GetByKey<T, TKey>( this ICKReadOnlyUniqueKeyedCollection<T, TKey> coll, TKey key )
        {
            bool exists;
            return coll.GetByKey( key, out exists );
        }

        /// <summary>
        /// Creates an array from a read only collection.
        /// This is a much more efficient version than the IEnumerable ToArray extension method
        /// since this implementation allocates one and only one array. 
        /// </summary>
        /// <typeparam name="T">Type of the array and lists elements.</typeparam>
        /// <param name="list">Read only collection of elements.</param>
        /// <returns>A new array that contains the same element as the collection.</returns>
        static public T[] ToArray<T>( this IReadOnlyCollection<T> list )
        {
            T[] r = new T[list.Count];
            int i = 0;
            foreach( T item in list ) r[i++] = item;
            return r;
        }

        /// <summary>
        /// Creates a <see cref="IReadOnlyList{T}"/> from a <see cref="IList{T}"/>.
        /// It is an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{T}"/> to create a read only list from.</param>
        /// <returns>A read only list that contains the elements from the input sequence.</returns>
        static public ICKReadOnlyList<T> ToReadOnlyList<T>( this IList<T> source )
        {
            if( source.Count == 0 ) return CKReadOnlyListEmpty<T>.Empty;
            if( source.Count == 1 ) return new CKReadOnlyListMono<T>( source[0] );
            T[] t = new T[source.Count];
            source.CopyTo( t, 0 );
            return new CKReadOnlyListOnIList<T>( t );
        }

        /// <summary>
        /// Attempts to consider a <see cref="IList{T}"/> as a <see cref="IReadOnlyList{T}"/>.
        /// If the actual object supports IReadOnlyList&lt;T&gt; it is a direct cast (in .Net 4.5, the 
        /// standard List&lt;T&gt; extends IReadOnlyList&lt;T&gt;), otherwise <see cref="ToReadOnlyList{T}(IList{T})"/> is 
        /// called to obtain an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{T}"/> to consider as a read only list.</param>
        /// <returns>A read only list that is the original list or contains the elements from the original list.</returns>
        static public IReadOnlyList<T> AsReadOnlyList<T>( this IList<T> source )
        {
            IReadOnlyList<T> rl = source as IReadOnlyList<T>;
            return rl ?? source.ToReadOnlyList<T>();
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyList{T}"/> from a <see cref="IList{U}"/>.
        /// It is an independant storage that keeps the references to the <paramref name="convertor"/> results.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the resulting read only list.</typeparam>
        /// <typeparam name="U">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{U}"/> to create a read only list from.</param>
        /// <param name="convertor">A function that transforms <typeparamref name="U"/> into <typeparamref name="T"/> elements.</param>
        /// <returns>A read only list that contains the conversion of elements from the input sequence.</returns>
        static public ICKReadOnlyList<T> ToReadOnlyList<T, U>( IList<U> source, Func<U, T> convertor )
        {
            if( convertor == null ) throw new ArgumentNullException( "convertor" );

            if( source == null || source.Count == 0 ) return CKReadOnlyListEmpty<T>.Empty;
            if( source.Count == 1 ) return new CKReadOnlyListMono<T>( convertor( source[0] ) );
            T[] t = new T[source.Count];
            for( int i = 0; i < t.Length; ++i ) t[i] = convertor( source[i] );
            return new CKReadOnlyListOnIList<T>( t );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyList{T}"/> from a sub sequence of a <see cref="IList{T}"/>.
        /// It is an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{T}"/> to create a read only list from.</param>
        /// <param name="startIndex">Starting index in source where copy must start.</param>
        /// <param name="count">Number of elements to take into account.</param>
        /// <returns>A read only list that contains the elements from the input sequence.</returns>
        static public ICKReadOnlyList<T> ToReadOnlyList<T>( this IList<T> source, int startIndex, int count )
        {
            if( count == 0 ) return CKReadOnlyListEmpty<T>.Empty;
            if( count < 0 ) throw new ArgumentOutOfRangeException( "count", count, R.ArgumentCountNegative );
            if( count == 1 ) return new CKReadOnlyListMono<T>( source[startIndex] );
            T[] t = new T[count];
            int i = 0;
            while( count-- > 0 ) t[i++] = source[startIndex++];
            return new CKReadOnlyListOnIList<T>( t );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyList{T}"/> from a sub sequence of a <see cref="IList{U}"/> and a convertor delegate.
        /// It is an independant storage that keeps the references to the <paramref name="convertor"/> results.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the resulting read only list.</typeparam>
        /// <typeparam name="U">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{U}"/> to create a read only list from.</param>
        /// <param name="startIndex">Starting index in source where copy must start.</param>
        /// <param name="count">Number of elements to take into account.</param>
        /// <param name="convertor">A function that transforms <typeparamref name="U"/> into <typeparamref name="T"/> elements.</param>
        /// <returns>A read only list that contains the converted elements from the input sequence.</returns>
        static public ICKReadOnlyList<T> ToReadOnlyList<T, U>( this IList<U> source, int startIndex, int count, Func<U, T> convertor )
        {
            if( convertor == null ) throw new ArgumentNullException( "convertor" );
            if( count == 0 ) return CKReadOnlyListEmpty<T>.Empty;
            if( count < 0 ) throw new ArgumentOutOfRangeException( "count", count, R.ArgumentCountNegative );
            if( count == 1 ) return new CKReadOnlyListMono<T>( convertor( source[startIndex] ) );
            T[] t = new T[count];
            int i = 0;
            while( count-- > 0 ) t[i++] = convertor( source[startIndex++] );
            return new CKReadOnlyListOnIList<T>( t );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyList{T}"/> from a <see cref="IList{T}"/> starting at a given index.
        /// It is an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{T}"/> to create a read only list from.</param>
        /// <param name="startIndex">Starting index in source where copy must start.</param>
        /// <returns>A read only list that contains the elements from the input sequence.</returns>
        static public ICKReadOnlyList<T> ToReadOnlyList<T>( this IList<T> source, int startIndex )
        {
            return ToReadOnlyList( source, startIndex, source.Count - startIndex );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyList{T}"/> from a <see cref="IList{U}"/> and a convertor delegate, starting at a given index.
        /// It is an independant storage that keeps the references to the <paramref name="convertor"/> results.
        /// </summary>
        /// <typeparam name="T">The type of the elements of out.</typeparam>
        /// <typeparam name="U">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{U}"/> to create a read only list from.</param>
        /// <param name="startIndex">Starting index in source where copy must start.</param>
        /// <param name="convertor">A function that transforms <typeparamref name="U"/> into <typeparamref name="T"/> elements.</param>
        /// <returns>A read only list that contains converted elements from the input sequence.</returns>
        static public ICKReadOnlyList<T> ToReadOnlyList<T,U>( this IList<U> source, int startIndex, Func<U, T> convertor )
        {
            return ToReadOnlyList( source, startIndex, source.Count - startIndex, convertor );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyList{T}"/> that is a copy of the <see cref="ICollection{T}"/> content.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="ICollection{T}"/> to create a read only list from.</param>
        /// <returns>A read only list that contains the elements from the input collection following the enumeration order.</returns>
        static public ICKReadOnlyList<T> ToReadOnlyList<T>( this ICollection<T> source )
        {
            if( source.Count == 0 ) return CKReadOnlyListEmpty<T>.Empty;
            if( source.Count == 1 )
            {
                using( IEnumerator<T> e = source.GetEnumerator() )
                {
                    return e.MoveNext() ? new CKReadOnlyListMono<T>( e.Current ) : CKReadOnlyListEmpty<T>.Empty;
                }
            }
            T[] t = new T[source.Count];
            source.CopyTo( t, 0 );
            return new CKReadOnlyListOnIList<T>( t );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyList{T}"/> from a <see cref="ICollection{T}"/>.
        /// It is an independant storage that keeps the references to the <paramref name="convertor"/> results.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the resulting read only list.</typeparam>
        /// <typeparam name="U">The type of the elements of source.</typeparam>
        /// <param name="convertor">A function that transforms <typeparamref name="U"/> into <typeparamref name="T"/> elements.</param>
        /// <param name="source">A <see cref="ICollection{T}"/> to create a read only list from.</param>
        /// <returns>A read only list that contains the elements from the input collection following the enumeration order.</returns>
        static public ICKReadOnlyList<T> ToReadOnlyList<T, U>( this ICollection<U> source, Func<U, T> convertor )
        {
            if( convertor == null ) throw new ArgumentNullException( "convertor" );
            if( source == null || source.Count == 0 ) return CKReadOnlyListEmpty<T>.Empty;
            if( source.Count == 1 )
            {
                using( IEnumerator<U> e = source.GetEnumerator() )
                {
                    return e.MoveNext() ? new CKReadOnlyListMono<T>( convertor( e.Current ) ) : CKReadOnlyListEmpty<T>.Empty;
                }
            }
            int i = 0;
            T[] t = new T[source.Count];
            foreach( U e in source ) t[i++] = convertor( e );
            return new CKReadOnlyListOnIList<T>( t );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyList{T}"/> from a <see cref="IEnumerable{T}"/>.
        /// It is an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IEnumerable{T}"/> to create a read only list from.</param>
        /// <returns>A read only list that contains the elements from the input sequence following the enumeration order.</returns>
        static public ICKReadOnlyList<T> ToReadOnlyList<T>( this IEnumerable<T> source )
        {
            using( IEnumerator<T> e = source.GetEnumerator() )
            {
                if( !e.MoveNext() ) return CKReadOnlyListEmpty<T>.Empty;
                T one = e.Current;
                if( !e.MoveNext() ) return new CKReadOnlyListMono<T>( one );
                T two = e.Current;
                T[] t;
                if( e.MoveNext() )
                {
                    T three = e.Current;
                    if( e.MoveNext() )
                    {
                        t = new T[] { one, two, three, e.Current };
                        int count = 4;
                        while( e.MoveNext() )
                        {
                            if( count == t.Length ) Array.Resize( ref t, count * 2 );
                            t[count++] = e.Current;
                        }
                        if( count != t.Length ) Array.Resize( ref t, count );
                    }
                    else t = new T[] { one, two, three };
                }
                else t = new T[] { one, two };
                return new CKReadOnlyListOnIList<T>( t );
            }
        }

        #region ToReadOnlyCollection (simple relay to ToReadOnlyList versions).

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyCollection{T}"/> from a <see cref="IList{T}"/>.
        /// It is an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{T}"/> to create a read only collection from.</param>
        /// <returns>A read only collection that contains the elements from the input sequence.</returns>
        static public ICKReadOnlyCollection<T> ToReadOnlyCollection<T>( this IList<T> source )
        {
            return ToReadOnlyList( source );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyCollection{T}"/> from a <see cref="IList{U}"/> and a convertor delegate.
        /// It is an independant storage that keeps the references to the <paramref name="convertor"/> results.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the resulting read only list.</typeparam>
        /// <typeparam name="U">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{U}"/> to create a read only collection from.</param>
        /// <param name="convertor">A function that transforms <typeparamref name="U"/> into <typeparamref name="T"/> elements.</param>
        /// <returns>A read only collection that contains the elements from the input sequence.</returns>
        static public ICKReadOnlyCollection<T> ToReadOnlyCollection<T,U>( this IList<U> source, Func<U, T> convertor )
        {
            return ToReadOnlyList( source, convertor );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyCollection{T}"/> from a sub seqence of a <see cref="IList{T}"/>.
        /// It is an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{T}"/> to create a read only list from.</param>
        /// <param name="startIndex">Starting index in source where copy must start.</param>
        /// <param name="count">Number of elements to take into account.</param>
        /// <returns>A read only collection that contains the elements from the input sequence.</returns>
        static public ICKReadOnlyCollection<T> ToReadOnlyCollection<T>( this IList<T> source, int startIndex, int count )
        {
            return ToReadOnlyList( source, startIndex, count );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyCollection{T}"/> from a sub seqence of a <see cref="IList{U}"/> and a convertor delegate.
        /// It is an independant storage that keeps the references to the <paramref name="convertor"/> results.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the resulting read only list.</typeparam>
        /// <typeparam name="U">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{U}"/> to create a read only list from.</param>
        /// <param name="startIndex">Starting index in source where copy must start.</param>
        /// <param name="count">Number of elements to take into account.</param>
        /// <param name="convertor">A function that transforms <typeparamref name="U"/> into <typeparamref name="T"/> elements.</param>
        /// <returns>A read only collection that contains the converted elements from the input sequence.</returns>
        static public ICKReadOnlyCollection<T> ToReadOnlyCollection<T, U>( this IList<U> source, int startIndex, int count, Func<U, T> convertor )
        {
            return ToReadOnlyList( source, startIndex, count, convertor );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyCollection{T}"/> from a <see cref="IList{T}"/>.
        /// It is an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{T}"/> to create a read only collection from.</param>
        /// <param name="startIndex">Starting index in source where copy must start.</param>
        /// <returns>A read only collection that contains the elements from the input sequence.</returns>
        static public ICKReadOnlyCollection<T> ToReadOnlyCollection<T>( this IList<T> source, int startIndex )
        {
            return ToReadOnlyList( source, startIndex );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyCollection{T}"/> from a <see cref="IList{U}"/> and a convertor delegate, starting at a given index.
        /// It is an independant storage that keeps the references to the <paramref name="convertor"/> results.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the resulting read only list.</typeparam>
        /// <typeparam name="U">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IList{U}"/> to create a read only list from.</param>
        /// <param name="startIndex">Starting index in source where copy must start.</param>
        /// <param name="convertor">A function that transforms <typeparamref name="U"/> into <typeparamref name="T"/> elements.</param>
        /// <returns>A read only collection that contains converted elements from the input sequence.</returns>
        static public ICKReadOnlyCollection<T> ToReadOnlyCollection<T, U>( this IList<U> source, int startIndex, Func<U, T> convertor )
        {
            return ToReadOnlyList( source, startIndex, convertor );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyCollection{T}"/> from a <see cref="ICollection{T}"/>.
        /// It is an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="ICollection{T}"/> to create a read only list from.</param>
        /// <returns>A read only collection that contains the elements from the input collection.</returns>
        static public ICKReadOnlyCollection<T> ToReadOnlyCollection<T>( this ICollection<T> source )
        {
            return ToReadOnlyList( source );
        }

        /// <summary>
        /// Creates a <see cref="IReadOnlyCollection{T}"/> from a <see cref="ICollection{U}"/>.
        /// It is an independant storage that keeps the references to the <paramref name="convertor"/> results.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the resulting read only collection.</typeparam>
        /// <typeparam name="U">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="ICollection{U}"/> to create a read only list from.</param>
        /// <param name="convertor">A function that transforms <typeparamref name="U"/> into <typeparamref name="T"/> elements.</param>
        /// <returns>A read only collection that contains the conversion of elements from the input sequence.</returns>
        static public ICKReadOnlyCollection<T> ToReadOnlyCollection<T, U>( this ICollection<U> source, Func<U, T> convertor )
        {
            return ToReadOnlyList( source, convertor );
        }

        /// <summary>
        /// Creates a <see cref="ICKReadOnlyCollection{T}"/> from a <see cref="IEnumerable{T}"/>.
        /// It is an independant storage (a copy).
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A <see cref="IEnumerable{T}"/> to create a read only list from.</param>
        /// <returns>A read only collection that contains the elements from the input sequence following the enumeration order.</returns>
        static public ICKReadOnlyCollection<T> ToReadOnlyCollection<T>( this IEnumerable<T> source )
        {
            return ToReadOnlyList( source );
        }

        #endregion
    }
}
