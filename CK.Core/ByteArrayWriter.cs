using System;
using System.Buffers;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Fast and direct implementation of a <see cref="IBufferWriter{T}"/> of bytes
    /// that exposes its <see cref="Buffer"/> and can change its current <see cref="Position"/>.
    /// </summary>
    /// <remarks>
    /// This is based on the <see cref="ArrayBufferWriter{T}"/>.
    /// </remarks>
    public sealed class ByteArrayWriter : IBufferWriter<byte>
    {
        const int ArrayMaxLength = 0x7FFFFFC7;
        const int DefaultInitialBufferSize = 256;

        byte[] _buffer;
        int _index;

        /// <summary>
        /// Creates an instance of an <see cref="ByteArrayWriter"/>, in which data can be written to,
        /// with the default initial capacity.
        /// </summary>
        public ByteArrayWriter()
        {
            _buffer = Array.Empty<byte>();
            _index = 0;
        }

        /// <summary>
        /// Creates an instance of an <see cref="ByteArrayWriter"/>, in which data can be written to,
        /// with an initial capacity specified.
        /// </summary>
        /// <param name="initialCapacity">The minimum capacity with which to initialize the underlying buffer.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="initialCapacity"/> is not positive (i.e. less than or equal to 0).
        /// </exception>
        public ByteArrayWriter( int initialCapacity )
        {
            if( initialCapacity <= 0 ) ThrowArgumentException( nameof( initialCapacity ) );
            _buffer = new byte[initialCapacity];
            _index = 0;
        }

        /// <summary>
        /// Gets the currently allocated buffer.
        /// </summary>
        public byte[] Buffer => _buffer;

        /// <summary>
        /// Returns the data written to the underlying buffer up to <see cref="Position"/>.
        /// </summary>
        public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory( 0, _index );

        /// <summary>
        /// Returns the data written to the underlying buffer up to <see cref="Position"/>.
        /// </summary>
        public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan( 0, _index );

        /// <summary>
        /// Gets or sets the current position. The buffer may grow.
        /// </summary>
        public int Position
        {
            get => _index;
            set
            {
                if( value < 0 ) ThrowArgumentException( nameof( Position ) );
                int delta = value - _buffer.Length;
                if( delta > 0 ) EnsureFreeCapacity( delta - _index );
                _index = value;
            }
        }

        /// <summary>
        /// Returns the total amount of space within the underlying buffer.
        /// </summary>
        public int Capacity => _buffer.Length;

        /// <summary>
        /// Returns the amount of space available that can still be written into without forcing the underlying buffer to grow.
        /// </summary>
        public int FreeCapacity => _buffer.Length - _index;

        /// <summary>
        /// Clears the data written to the underlying buffer.
        /// </summary>
        public void Clear( bool zeroBuffer = true )
        {
            Debug.Assert( _buffer.Length >= _index );
            if( zeroBuffer ) _buffer.AsSpan( 0, _index ).Clear();
            _index = 0;
        }

        /// <summary>
        /// Notifies <see cref="IBufferWriter{T}"/> that <paramref name="count"/> amount of data was written to the output <see cref="Span{T}"/>/<see cref="Memory{T}"/>
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="count"/> is negative.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to advance past the end of the underlying buffer.
        /// </exception>
        /// <remarks>
        /// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
        /// </remarks>
        public void Advance( int count )
        {
            if( count < 0 ) ThrowArgumentException( nameof( count ) );
            if( _index > _buffer.Length - count ) ThrowAdvancedTooFar( _buffer.Length );
            _index += count;
        }

        /// <summary>
        /// Returns a <see cref="Memory{T}"/> to write to that is at least the requested length (specified by <paramref name="sizeHint"/>).
        /// If no <paramref name="sizeHint"/> is provided (or it's equal to <code>0</code>), some non-empty buffer is returned.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sizeHint"/> is negative.
        /// </exception>
        /// <remarks>
        /// This will never return an empty <see cref="Memory{T}"/>.
        /// </remarks>
        /// <remarks>
        /// There is no guarantee that successive calls will return the same buffer or the same-sized buffer.
        /// </remarks>
        /// <remarks>
        /// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
        /// </remarks>
        public Memory<byte> GetMemory( int sizeHint = 0 )
        {
            EnsureFreeCapacity( sizeHint );
            Debug.Assert( _buffer.Length > _index );
            return _buffer.AsMemory( _index );
        }

        /// <summary>
        /// Returns a Span to write to that is at least the requested length (specified by <paramref name="sizeHint"/>).
        /// If no <paramref name="sizeHint"/> is provided (or it's equal to <code>0</code>), some non-empty buffer is returned.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sizeHint"/> is negative.
        /// </exception>
        /// <remarks>
        /// This will never return an empty Span.
        /// </remarks>
        /// <remarks>
        /// There is no guarantee that successive calls will return the same buffer or the same-sized buffer.
        /// </remarks>
        /// <remarks>
        /// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
        /// </remarks>
        public Span<byte> GetSpan( int sizeHint = 0 )
        {
            EnsureFreeCapacity( sizeHint );
            Debug.Assert( _buffer.Length > _index );
            return _buffer.AsSpan( _index );
        }

        /// <summary>
        /// Ensures that at least <paramref name="sizeHint"/> bytes are available.
        /// </summary>
        /// <param name="sizeHint">Number of bytes to ensure.</param>
        public void EnsureFreeCapacity( int sizeHint )
        {
            if( sizeHint < 0 ) ThrowArgumentException( nameof( sizeHint ) );

            if( sizeHint == 0 )
            {
                sizeHint = 1;
            }

            if( sizeHint > FreeCapacity )
            {
                int currentLength = _buffer.Length;

                // Attempt to grow by the larger of the sizeHint and double the current size.
                int growBy = Math.Max( sizeHint, currentLength );

                if( currentLength == 0 )
                {
                    growBy = Math.Max( growBy, DefaultInitialBufferSize );
                }

                int newSize = currentLength + growBy;

                if( (uint)newSize > int.MaxValue )
                {
                    // Attempt to grow to ArrayMaxLength.
                    uint needed = (uint)(currentLength - FreeCapacity + sizeHint);
                    Debug.Assert( needed > currentLength );

                    if( needed > ArrayMaxLength )
                    {
                        ThrowOutOfMemoryException( needed );
                    }

                    newSize = ArrayMaxLength;
                }

                Array.Resize( ref _buffer, newSize );
            }

            Debug.Assert( FreeCapacity > 0 && FreeCapacity >= sizeHint );
        }

        static void ThrowArgumentException( string argName )
        {
            throw new ArgumentException( null, argName );
        }

        static void ThrowAdvancedTooFar( int capacity )
        {
            throw new InvalidOperationException( $"Cannot advance past the end of the buffer, which has a size of {capacity}." );
        }

        static void ThrowOutOfMemoryException( uint capacity )
        {
            throw new OutOfMemoryException( $"Cannot allocate a buffer of size {capacity}." );
        }
    }
}
