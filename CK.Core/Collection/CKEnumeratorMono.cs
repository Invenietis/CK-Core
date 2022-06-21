using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core
{
    /// <summary>
    /// Defines an optimized <see cref="IEnumerator{T}"/> that contains
    /// only one element.
    /// </summary>
	public sealed class CKEnumeratorMono<T> : IEnumerator<T>
	{
        T _val;
        int _pos;

        /// <summary>
        /// Initializes a new enumerator bound to one and only one value.
        /// </summary>
        /// <param name="val">Unique value that will be enumerated by this <see cref="CKEnumeratorMono{T}"/></param>
        public CKEnumeratorMono( T val )
        {
            _val = val;
            _pos = -1;
        }

        /// <summary>
        /// Gets the strongly typed element in the collection at the current position of the enumerator.
        /// </summary>
		public T Current
		{
            get
            {
                if( _pos == 0 ) return _val;
                throw new InvalidOperationException();
            }
		}

        /// <summary>
        /// Dispose the <see cref="IEnumerator{T}"/>.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void Dispose()
		{
		}

        [ExcludeFromCodeCoverage]
		object? IEnumerator.Current => Current; 

        /// <summary>
        /// Move to the next element.
        /// </summary>
        /// <returns>True the first time, false otherwise</returns>
		public bool MoveNext()
		{
			return ++_pos == 0;
		}

        /// <summary>
        /// Resets the enumerator.
        /// </summary>
		public void Reset()
		{
            _pos = -1;
		}
	}

}
