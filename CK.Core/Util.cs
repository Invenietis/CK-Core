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
* Copyright © 2007-2012, 
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

namespace CK.Core
{
    /// <summary>
    /// Utility class.
    /// Offers useful functions, constants, singletons and delegates.
    /// </summary>
    static public partial class Util
	{
        /// <summary>
        /// Gets a static empty <see cref="String"/> array.
        /// </summary>
        static public readonly string[] EmptyStringArray = new string[0];

        /// <summary>
        /// The empty version is defined as the Major.Minor.Build.Revision set to "0.0.0.0".
        /// </summary>
        static public readonly Version EmptyVersion = new Version( 0, 0, 0, 0 );

        /// <summary>
        /// Gets 1900, january the 1st. This is the 'zero' of Sql Server datetime and smalldatetime
        /// types.
        /// </summary>
        static public readonly DateTime SqlServerEpoch = new DateTime( 1900, 1, 1, 0, 0, 0, DateTimeKind.Utc );

        /// <summary>
        /// Gets 1970, january the 1st. This is the 'zero' of numerous date/time system
        /// like Unix file system or javascript.
        /// </summary>
        static public readonly DateTime UnixEpoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );

        /// <summary>
        /// Private array currently used by Converter functions.
        /// </summary>
        static char[] _hexChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// Centralized <see cref="IDisposable.Dispose"/> action call: it adapts an <see cref="IDisposable"/> interface to an <see cref="Action"/>.
        /// Can be safely called if <paramref name="obj"/> is null. 
        /// See <see cref="DisposeAction"/> to wrap an action in a <see cref="IDisposable"/> interface.
        /// </summary>
        /// <param name="obj">The disposable object to dispose (can be null).</param>
        public static void ActionDispose( IDisposable obj )
        {
            if( obj != null ) obj.Dispose();
        }

        [Obsolete( "Use CreateDisposableAction instead.", true )]
        public static IDisposable DisposeAction( Action a )
        {
            return CreateDisposableAction( a );
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

        class FakeDisposable : IDisposable { public void  Dispose() { } }

        /// <summary>
        /// A void, immutable, <see cref="IDisposable"/> that does absolutely nothing.
        /// </summary>
        public static readonly IDisposable EmptyDisposable = new FakeDisposable(); 

        /// <summary>
        /// Centralized void action call for any type. 
        /// This method is one of the safest method never written in the world. 
        /// It does absolutely nothing.
        /// </summary>
        /// <param name="obj">Any object.</param>
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
        /// Binary search immplementation that relies on a <see cref="Comparison{T}"/>.
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
        /// Binary search immplementation that relies on an extended comparer: a function that knows how to 
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

    }
}
