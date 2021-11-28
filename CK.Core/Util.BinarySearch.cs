using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    public static partial class Util
    {
        /// <summary>
        /// Binary search implementation with a comparable that knows its value.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="TComparable">Type of the comparable. Best performance is achieved with a struct.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="startIndex">The starting index in the list.</param>
        /// <param name="length">The number of elements to consider in the list.</param>
        /// <param name="comparable">The comparable that knows its value.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(Array, object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        public static int BinarySearch<T, TComparable>( IReadOnlyList<T> sortedList, int startIndex, int length, TComparable comparable )
            where TComparable : IComparable<T>
        {
            Guard.IsNotNull( sortedList, nameof( sortedList ) );
            int low = startIndex;
            int high = (startIndex + length) - 1;
            while( low <= high )
            {
                int mid = (int)(((uint)high + (uint)low) >> 1);
                int cmp = comparable.CompareTo( sortedList[mid] );
                if( cmp == 0 ) return mid;
                if( cmp > 0 ) low = mid + 1;
                else high = mid - 1;
            }
            return ~low;
        }

        /// <summary>
        /// Adapts a comparer and a value to a comparable.
        /// This adapter as well as <see cref="ComparisonComparable{T}"/>, <see cref="DefaultComparerComparable{T}"/> and <see cref="KeyedComparisonComparable{T, TKey}"/>
        /// can be used with <see cref="MemoryExtensions"/> binary search span extension methods.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <typeparam name="TComparer">The value's comparer.</typeparam>
        public readonly struct ComparerComparable<T, TComparer> : IComparable<T>
            where TComparer : IComparer<T>
        {
            private readonly T _value;
            private readonly TComparer _comparer;

            /// <summary>
            /// Initializes a new adapter.
            /// </summary>
            /// <param name="value">The value to locate.</param>
            /// <param name="comparer">The comparer to use.</param>
            public ComparerComparable( T value, TComparer comparer )
            {
                _value = value;
                _comparer = comparer;
            }

            /// <summary>
            /// Simple relay to the comparer's function.
            /// </summary>
            /// <param name="other">The other value (from the list).</param>
            /// <returns>The relative comparison.</returns>
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int CompareTo( T? other ) => _comparer.Compare( _value, other );
        }

        /// <summary>
        /// Binary search implementation of a value and a comparer. Uses <see cref="ComparerComparable{T,TComparer}"/> adapter.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="TComparer">Type of the comparer. Best performance is achieved with a struct.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="startIndex">The starting index in the list.</param>
        /// <param name="length">The number of elements to consider in the list.</param>
        /// <param name="value">The value to locate.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(Array, object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int BinarySearch<T, TComparer>( IReadOnlyList<T> sortedList, int startIndex, int length, T value, TComparer comparer )
            where TComparer : IComparer<T>
        {
            return BinarySearch( sortedList, startIndex, length, new ComparerComparable<T, TComparer>( value, comparer ) );
        }

        /// <summary>
        /// Adapts a value and a <see cref="Comparison{T}"/> delegate to a comparable.
        /// This adapter as well as <see cref="ComparerComparable{T, TComparer}"/>, <see cref="DefaultComparerComparable{T}"/> and <see cref="KeyedComparisonComparable{T, TKey}"/>
        /// can be used with <see cref="MemoryExtensions"/> binary search span extension methods.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        public readonly struct ComparisonComparable<T> : IComparable<T>
        {
            private readonly T _value;
            private readonly Comparison<T> _comparison;

            /// <summary>
            /// Initializes a new adapter.
            /// </summary>
            /// <param name="value">The value to locate.</param>
            /// <param name="comparison">The comparison function.</param>
            public ComparisonComparable( T value, Comparison<T> comparison )
            {
                _value = value;
                _comparison = comparison;
            }

            /// <summary>
            /// Simple relay to the comparison function.
            /// </summary>
            /// <param name="other">The other value (from the list).</param>
            /// <returns>The relative comparison.</returns>
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int CompareTo( T? other ) => other == null ? 1 : _comparison( _value, other );
        }


        /// <summary>
        /// Binary search implementation that relies on a <see cref="Comparison{T}"/>.
        /// Caution: no null checks are done by this function. Uses <see cref="ComparisonComparable{T}"/> adapter.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="startIndex">The starting index in the list.</param>
        /// <param name="length">The number of elements to consider in the list.</param>
        /// <param name="value">The value to locate.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <returns>Same as <see cref="System.Array.BinarySearch(System.Array, object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int BinarySearch<T>( IReadOnlyList<T> sortedList, int startIndex, int length, T value, Comparison<T> comparison )
        {
            return BinarySearch( sortedList, startIndex, length, new ComparisonComparable<T>( value, comparison ) );
        }

        /// <summary>
        /// Adapts a value and a <see cref="Func{T,TKey,UInt32}"/> delegate to a comparable.
        /// This adapter as well as <see cref="ComparisonComparable{T}"/>, <see cref="DefaultComparerComparable{T}"/> and <see cref="ComparerComparable{T, TComparer}"/>
        /// can be used with <see cref="MemoryExtensions"/> binary search span extension methods.
        /// </summary>
        /// <typeparam name="T">The type of the value in the list.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        public readonly struct KeyedComparisonComparable<T, TKey> : IComparable<T>
        {
            private readonly TKey _key;
            private readonly Func<T, TKey, int> _comparison;

            /// <summary>
            /// Initializes a new adapter.
            /// </summary>
            /// <param name="key">The key to locate.</param>
            /// <param name="comparison">The keyed comparison function.</param>
            public KeyedComparisonComparable( TKey key, Func<T, TKey, int> comparison )
            {
                Guard.IsNotNull( comparison, nameof( comparison ) );
                _key = key;
                _comparison = comparison;
            }

            /// <summary>
            /// Simple relay to the keyed comparison function.
            /// </summary>
            /// <param name="other">The other value (from the list).</param>
            /// <returns>The relative comparison.</returns>
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int CompareTo( T? other ) => other == null ? -1 : -_comparison( other, _key );
        }

        /// <summary>
        /// Binary search implementation that relies on an extended comparer: a function that knows how to 
        /// compare the elements of the list to a key of another type. Uses <see cref="KeyedComparisonComparable{T,TKey}"/> adapter.
        /// Caution: no null checks are done by this function.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="startIndex">The starting index in the list.</param>
        /// <param name="length">The number of elements to consider in the list.</param>
        /// <param name="key">The value of the key.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(System.Array, object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int BinarySearch<T, TKey>( IReadOnlyList<T> sortedList, int startIndex, int length, TKey key, Func<T, TKey, int> comparison )
        {
            return BinarySearch( sortedList, startIndex, length, new KeyedComparisonComparable<T, TKey>( key, comparison ) );
        }

        /// <summary>
        /// Adapts a value to a comparable based on its <see cref="Comparer{T}.Default"/> comparer.
        /// This adapter as well as <see cref="ComparerComparable{T, TComparer}"/>, <see cref="ComparisonComparable{T}"/>
        /// and <see cref="KeyedComparisonComparable{T, TKey}"/> can be used with <see cref="MemoryExtensions"/> binary search span extension methods.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        public readonly struct DefaultComparerComparable<T> : IComparable<T>
        {
            private readonly T _value;
            private readonly Comparer<T> _comparer;

            /// <summary>
            /// Initializes a new adapter.
            /// </summary>
            /// <param name="value">The value to locate.</param>
            public DefaultComparerComparable( T value )
            {
                _value = value;
                _comparer = Comparer<T>.Default;
            }

            /// <summary>
            /// Simple relay to the comparer's function.
            /// </summary>
            /// <param name="other">The other value (from the list).</param>
            /// <returns>The relative comparison.</returns>
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public int CompareTo( T? other ) => _comparer.Compare( _value, other );
        }

        /// <summary>
        /// Binary search implementation that uses <see cref="DefaultComparerComparable{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements.</typeparam>
        /// <param name="sortedList">Read only list of elements.</param>
        /// <param name="startIndex">The starting index in the list.</param>
        /// <param name="length">The number of elements to consider in the list.</param>
        /// <param name="value">The value to locate.</param>
        /// <returns>Same as <see cref="Array.BinarySearch(System.Array, object)"/>: negative index if not found which is the bitwise complement of (the index of the next element plus 1).</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int BinarySearch<T>( IReadOnlyList<T> sortedList, int startIndex, int length, T value )
        {
            return BinarySearch( sortedList, startIndex, length, new DefaultComparerComparable<T>( value ) );
        }
    }
}
