#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Impl\Extension.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace CK.Core
{
    /// <summary>
    /// Utility class.
    /// </summary>
	static public class Util
	{

		static int[] _multiplyDeBruijnBitPosition = 
				{ 
					0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 
					31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9 
				};

        
        /// <summary>
        /// Compute the Log2 (logarithm base 2) of a given number.
        /// </summary>
        /// <param name="v">Integer to compute</param>
        /// <returns>Log2 of the given integer</returns>
        [CLSCompliant(false)]
		static public int Log2( UInt32 v )
		{
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v = (v >> 1) + 1;
			return _multiplyDeBruijnBitPosition[((v * 0x077CB531U)) >> 27];
		}

        /// <summary>
        /// Compute the Log2ForPower2 (logarithm base 2 power 2) of a given number.
        /// </summary>
        /// <param name="v">Integer to compute. It MUST be a power of 2.</param>
        /// <returns>Result</returns>
        [CLSCompliant(false)]
		static public int Log2ForPower2( UInt32 v )
		{
			return _multiplyDeBruijnBitPosition[(((uint)v * 0x077CB531U)) >> 27];
		}

        /// <summary>
        /// Counts the number of bits in the given byte.
        /// </summary>
        /// <param name="v">The value for which number of bits must be computed.</param>
        /// <returns>The number of bits.</returns>
		static public int BitCount( Byte v )
        {        
            return (int)( (v * 0x200040008001 & 0x111111111111111) % 0xF );
        }

        /// <summary>
        /// Gets a static empty <see cref="String"/> array.
        /// </summary>
        static public readonly string[] EmptyStringArray = new string[0];

        /// <summary>
        /// The empty version is defined as the Major.Minor.Build.Revision set to "0.0.0.0".
        /// </summary>
        static public readonly Version EmptyVersion = new Version( 0, 0, 0, 0 );

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

        /// <summary>
        /// Wraps an action in a <see cref="IDisposable"/> interface
        /// Can be safely called if <paramref name="a"/> is null (the dispose call will do nothing).
        /// See <see cref="ActionDispose"/> to adapt an <see cref="IDisposable"/> interface to an <see cref="Action"/>.
        /// </summary>
        /// <param name="a">The action to call when <see cref="IDisposable.Dispose"/> is called.</param>
        public static IDisposable DisposeAction( Action a )
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
        /// This method is the safest method never written. 
        /// It does absolutely nothing.
        /// </summary>
        /// <param name="obj">Any object.</param>
        public static void ActionVoid<T>( T obj ) 
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
        /// <returns>Same as <see cref="Array.BinarySearch"/> (negative index if not found).</returns>
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
        /// <returns>Same as <see cref="Array.BinarySearch"/> (negative index if not found).</returns>
        public static int BinarySearch<T,TKey>( T[] array, int startIndex, int length, TKey key, Func<T,TKey,int> comparison )
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
        /// Provides methods to combine hash values.
        /// Based on Daniel J. Bernstein algorithm (http://cr.yp.to/cdb/cdb.txt).
        /// </summary>
        public static class Hash
        {

            /// <summary>
            /// Gets a very classical start value.
            /// It seems that this value has nothing special (mathematically speaking) except that it 
            /// has been used and reused by many people since DJB choose it.
            /// </summary>
            public static Int64 StartValue { get { return 5381; } }

            /// <summary>
            /// Combines an existing hash value with a new one.
            /// </summary>
            /// <param name="hash">Current hash.</param>
            /// <param name="value">Value to combine.</param>
            /// <returns>A combined hash.</returns>
            public static Int64 Combine( Int64 hash, int value )
            {
                return ((hash << 5) + hash) ^ value;
            }

            /// <summary>
            /// Combines an existing hash value with an object's hash (object can be null).
            /// </summary>
            /// <param name="hash">Current hash.</param>
            /// <param name="o">Object whose hash must be combined (can be null).</param>
            /// <returns>A combined hash.</returns>
            public static Int64 Combine( Int64 hash, object o )
            {
                return Combine( hash, o != null ? o.GetHashCode() : 0 );
            }

            /// <summary>
            /// Combines an existing hash value with multiples object's hash.
            /// </summary>
            /// <param name="hash">Current hash.</param>
            /// <param name="c">Multiple objects. Can be null.</param>
            /// <returns>A combined hash.</returns>
            public static Int64 Combine( Int64 hash, IEnumerable c )
            {
                int nb = 0;
                if( c != null )
                {
                    foreach( object o in c )
                    {
                        hash = Combine( hash, o );
                        nb++;
                    }
                }
                return Combine( hash, nb );
            }

            /// <summary>
            /// Combines an existing hash value with multiples object's written directly as parameters.
            /// </summary>
            /// <param name="hash">Current hash.</param>
            /// <param name="objects">Multiple objects.</param>
            /// <returns>A combined hash.</returns>
            public static Int64 Combine( Int64 hash, params object[] objects )
            {
                return Combine( hash, (IEnumerable)objects );
            }

        }


    }
}
