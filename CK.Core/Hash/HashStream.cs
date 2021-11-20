using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        HashAlgorithm _hash;
        readonly Stream? _reader;
        readonly Stream? _writer;
        readonly bool _leaveOpen;

        /// <summary>
        /// Initializes a new instance of <see cref="HashStream{T}"/>.
        /// This stream is a terminal one.
        /// </summary>
        /// <param name="hashName">The <see cref="HashAlgorithmName"/>.</param>
        public HashStream( string hashName )
        {
            _hash = CreateHash( hashName );
        }

        static HashAlgorithm CreateHash( string hashName )
        {
            return HashAlgorithm.Create( hashName ) ?? throw new ArgumentException( $"Unknown hash algorithm name '{hashName}'.", nameof( hashName ) );
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HashStream"/> as a decorator on a read or write inner stream.
        /// </summary>
        /// <param name="hashName">The <see cref="HashAlgorithmName"/>.</param>
        /// <param name="inner">The inner (decorated) stream.</param>
        /// <param name="read">True to read from the inner stream through this one.</param>
        /// <param name="leaveOpen">True to leave the inner stream opened when disposing this one.</param>
        public HashStream( string hashName, Stream inner, bool read, bool leaveOpen )
        {
            _hash = CreateHash( hashName );
            _leaveOpen = leaveOpen;
            if( read ) _reader = inner;
            else _writer = inner;
        }

        /// <summary>
        /// Gets the final hash value.
        /// Once this value has been read this stream should be disposed.
        /// </summary>
        /// <returns>The final hash value.</returns>
        public byte[] GetFinalResult()
        {
            _hash.TransformFinalBlock( Array.Empty<byte>(), 0, 0 );
            return _hash.Hash!;
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
        /// Flushes the inner (decorated) stream if any. 
        /// </summary>
        public override void Flush()
        {
            _reader?.Flush();
            _writer?.Flush();
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        public override long Length => throw new NotSupportedException();

        /// <summary>
        /// This property is not supported.
        /// </summary>
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
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
            if( _reader == null ) throw new InvalidOperationException();
            int r = _reader.Read( buffer, offset, count );
            _hash.TransformBlock( buffer, offset, r, null, 0 );
            return r;
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        public override long Seek( long offset, SeekOrigin origin ) => throw new NotSupportedException();

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        public override void SetLength( long value ) => throw new NotSupportedException();

        /// <summary>
        /// Writes the given bytes into the inner (decorated) stream and handles them to update the SHA1
        /// computation state.
        /// This can be called only if this stream is a decorator initialized in write mode.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes.This method copies count bytes from buffer to the current
        /// stream.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin copying bytes to the current
        /// stream.
        /// </param>
        /// <param name="count">
        /// The number of bytes to be written to the current stream.
        /// </param>
        public override void Write( byte[] buffer, int offset, int count )
        {
            if( _reader != null ) throw new InvalidOperationException();
            _hash.TransformBlock( buffer, offset, count, null, 0 );
            _writer?.Write( buffer, offset, count );
        }

        /// <summary>
        /// Asynchronously writes the given bytes into the inner (decorated) stream and handles them to
        /// update the SHA1 computation state.
        /// This can be called only if this stream is a decorator initialized in write mode.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer from which to begin copying bytes to the
        /// stream.
        /// </param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The awaitable.</returns>
        public override Task WriteAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            if( _reader != null ) return Task.FromException( new InvalidOperationException() );
            _hash.TransformBlock( buffer, offset, count, null, 0 );
            return _writer != null ? _writer.WriteAsync( buffer, offset, count, cancellationToken ) : Task.CompletedTask;
        }

        /// <summary>
        /// Asynchronously reads the bytes from the inner (decorated) stream and handles them to update the SHA1
        /// computation state.
        /// This can be called only if this stream is a decorator initialized in read mode.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation.The value of the TResult
        /// parameter contains the total number of bytes read into the buffer. The result
        /// value can be less than the number of bytes requested if the number of bytes currently
        /// available is less than the requested number, or it can be 0 (zero) if the end
        /// of the stream has been reached.
        /// </returns>
        public override Task<int> ReadAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
        {
            if( _reader == null ) return Task.FromException<int>( new InvalidOperationException() );
            return _reader.ReadAsync( buffer, offset, count, cancellationToken )
                          .ContinueWith( x =>
                          {
                              _hash.TransformBlock( buffer, offset, x.Result, null, 0 );
                              return x.Result;
                          }, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default );
        }
    }
}
