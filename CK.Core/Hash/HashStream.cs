using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Simple wrapper around a <see cref="HashAlgorithm"/> instance to compute
    /// the hash of a stream. Can be used in read or write mode and as a terminal stream
    /// or as a decorator.
    /// </summary>
    public class HashStream : Stream
    {
        IncrementalHash _hash;
        readonly Stream? _reader;
        readonly Stream? _writer;
        readonly bool _leaveOpen;

        /// <summary>
        /// Initializes a new instance of <see cref="HashStream"/>.
        /// This stream is a terminal one.
        /// </summary>
        /// <param name="hashName">The <see cref="HashAlgorithmName"/>.</param>
        public HashStream( HashAlgorithmName hashName )
        {
            _hash = IncrementalHash.CreateHash( hashName );
        }

        /// <summary>
        /// Initializes a new instance with an exisitng <see cref="IncrementalHash"/>.
        /// This stream is a terminal one.
        /// </summary>
        /// <param name="hasher">The <see cref="IncrementalHash"/>.</param>
        public HashStream( IncrementalHash hasher )
        {
            Throw.CheckNotNullArgument( hasher );
            _hash = hasher;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HashStream"/> as a decorator on a read or write inner stream.
        /// </summary>
        /// <param name="hasher">The <see cref="HashAlgorithmName"/>.</param>
        /// <param name="inner">The inner (decorated) stream.</param>
        /// <param name="read">True to read from the inner stream through this one.</param>
        /// <param name="leaveOpen">True to leave the inner stream opened when disposing this one.</param>
        public HashStream( HashAlgorithmName hasher, Stream inner, bool read, bool leaveOpen )
        {
            _hash = IncrementalHash.CreateHash( hasher );
            _leaveOpen = leaveOpen;
            if( read ) _reader = inner;
            else _writer = inner;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HashStream"/> as a decorator on a read or write inner stream
        /// with an existing <see cref="IncrementalHash"/>.
        /// </summary>
        /// <param name="hasher">The <see cref="IncrementalHash"/>.</param>
        /// <param name="inner">The inner (decorated) stream.</param>
        /// <param name="read">True to read from the inner stream through this one.</param>
        /// <param name="leaveOpen">True to leave the inner stream opened when disposing this one.</param>
        public HashStream( IncrementalHash hasher, Stream inner, bool read, bool leaveOpen )
        {
            Throw.CheckNotNullArgument( hasher );
            Throw.CheckNotNullArgument( inner );
            _hash = hasher;
            _leaveOpen = leaveOpen;
            if( read ) _reader = inner;
            else _writer = inner;
        }

        /// <summary>
        /// Gets the inner hasher.
        /// </summary>
        public IncrementalHash IncrementalHash => _hash;

        /// <summary>
        /// Gets the final hash value.
        /// Once this value has been read this stream should be disposed.
        /// </summary>
        /// <returns>The final hash value.</returns>
        public byte[] GetFinalResult()
        {
            return _hash.GetHashAndReset();
        }

        /// <summary>
        /// Overridden to dispose the internal <see cref="SHA1Managed"/> instance.
        /// </summary>
        /// <param name="disposing">True on actual disposing, false when called by the GC.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && _hash != null )
            {
                _hash.Dispose();
                _hash = null!;
                if( !_leaveOpen )
                {
                    _reader?.Dispose();
                    _writer?.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets whether this stream is a decorator in read mode.
        /// </summary>
        public override bool CanRead => _reader != null;

        /// <summary>
        /// Seek (like any operation on position) is not supported: always false.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets whether this stream is either a terminal one or a
        /// decorator opened in write mode.
        /// </summary>
        public override bool CanWrite => _reader == null;

        /// <summary>
        /// Flushes the inner (decorated) writer stream if any. 
        /// </summary>
        public override void Flush()
        {
            _writer?.Flush();
        }

        /// <summary>
        /// Flushes the inner (decorated) writer stream if any. 
        /// </summary>
        public override Task FlushAsync( CancellationToken cancellationToken )
        {
            return _writer != null ? _writer.FlushAsync( cancellationToken ) : Task.CompletedTask;
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        public override long Length => Throw.NotSupportedException<long>();

        /// <summary>
        /// This property is not supported.
        /// </summary>
        public override long Position
        {
            get => Throw.NotSupportedException<long>();
            set => Throw.NotSupportedException();
        }

        /// <summary>
        /// Reads the bytes from the inner (decorated) stream and handles them to update the SHA1
        /// computation state.
        /// This can be called only if this stream is a decorator initialized in read mode.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains the specified
        /// byte array with the values between offset and (offset + count - 1) replaced by
        /// the bytes read from the current source.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing the data read
        /// from the current stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number
        /// of bytes requested if that many bytes are not currently available, or zero (0)
        /// if the end of the stream has been reached.
        /// </returns>
        public override int Read( byte[] buffer, int offset, int count )
        {
            Throw.CheckState( _reader != null );
            int r = _reader.Read( buffer, offset, count );
            _hash.AppendData( buffer, offset, r );
            return r;
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        public override long Seek( long offset, SeekOrigin origin ) => Throw.NotSupportedException<long>();

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        public override void SetLength( long value ) => Throw.NotSupportedException();

        /// <inheritdoc />
        public override void Write( byte[] buffer, int offset, int count )
        {
            Throw.CheckState( _reader == null );
            _hash.AppendData( buffer, offset, count );
            _writer?.Write( buffer, offset, count );
        }

        /// <inheritdoc />
        public override Task WriteAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            if( _reader != null ) return Task.FromException( new InvalidOperationException() );
            _hash.AppendData( buffer, offset, count );
            return _writer != null ? _writer.WriteAsync( buffer, offset, count, cancellationToken ) : Task.CompletedTask;
        }

        /// <inheritdoc />
        public override void Write( ReadOnlySpan<byte> buffer )
        {
            _hash.AppendData( buffer );
            _writer?.Write( buffer );
        }

        /// <inheritdoc />
        public override ValueTask WriteAsync( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default )
        {
            _hash.AppendData( buffer.Span );
            return _writer != null ? _writer.WriteAsync( buffer, cancellationToken ) : default;
        }

        /// <inheritdoc />
        public override Task<int> ReadAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            if( _reader == null ) return Task.FromException<int>( new InvalidOperationException() );
            return _reader.ReadAsync( buffer, offset, count, cancellationToken )
                          .ContinueWith( x =>
                          {
                              _hash.AppendData( buffer, offset, x.Result );
                              return x.Result;
                          }, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default );
        }

        /// <inheritdoc />
        public override async ValueTask<int> ReadAsync( Memory<byte> buffer, CancellationToken cancellationToken = default )
        {
            Throw.CheckState( _reader != null );
            int read = await _reader.ReadAsync( buffer, cancellationToken );
            _hash.AppendData( buffer.Span.Slice( 0, read ) );
            return read;
        }

        /// <inheritdoc />
        public override int Read( Span<byte> buffer )
        {
            Throw.CheckState( _reader != null );
            int read = _reader.Read( buffer );
            _hash.AppendData( buffer.Slice( 0, read ) );
            return read;
        }
    }
}
