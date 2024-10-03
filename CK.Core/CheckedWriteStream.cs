using Microsoft.IO;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;

namespace CK.Core;

/// <summary>
/// A checker stream is a writable stream that compares the bytes written
/// into it with an existing content. Once the new bytes are written, <see cref="GetResult"/>
/// retrieves the conclusion.
/// <para>
/// Use <see cref="Create(ReadOnlySequence{byte})"/> (or <see cref="Create(RecyclableMemoryStream)"/>
/// with an already filled stream) to create a stream with its reference content, write all the data
/// to check into this new stream and call GetResult.
/// </para>
/// </summary>
public abstract class CheckedWriteStream : Stream
{
    /// <summary>
    /// Returned value of <see cref="CheckedWriteStream.GetResult()"/>.
    /// </summary>
    public enum Result
    {
        /// <summary>
        /// No differences.
        /// </summary>
        None,

        /// <summary>
        /// The byte at <see cref="Stream.Position"/> differs.
        /// </summary>
        HasByteDifference,

        /// <summary>
        /// The written bytes overflow the reference bytes.
        /// </summary>
        LongerThanRefBytes,

        /// <summary>
        /// The written bytes are shorter than the reference bytes.
        /// </summary>
        ShorterThanRefBytes
    }

    /// <summary>
    /// Gets or sets a flag that triggers a throw <see cref="ArgumentException"/> when a
    /// byte differs (<see cref="Result.HasByteDifference"/>) or there are more written bytes
    /// than the reference bytes (<see cref="Result.LongerThanRefBytes"/>).
    /// When there are less written bytes than the reference bytes (<see cref="Result.ShorterThanRefBytes"/>)
    /// the exception is thrown by <see cref="GetResult"/>.
    /// <para>
    /// This default to false.
    /// </para>
    /// </summary>
    public abstract bool ThrowArgumentException { get; set; }

    /// <summary>
    /// Gets the final result once all the bytes have been written into this checker.
    /// This throws a <see cref="ArgumentException"/> if <see cref="ThrowArgumentException"/> is true
    /// and there are less written bytes than the reference bytes (<see cref="Result.ShorterThanRefBytes"/>).
    /// </summary>
    /// <returns>The final result.</returns>
    public abstract Result GetResult();

    sealed class CheckedWriteStreamOnROSBytes : CheckedWriteStream
    {
        ReadOnlySequence<byte> _refBytes;
        readonly long _initialLength;
        long _position;
        bool _hasDiff;
        bool _diffAtPosition;
        bool _longerThanRef;

        public bool HasDiff => _hasDiff;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => Throw.NotSupportedException<long>();

        public override bool ThrowArgumentException { get; set; }

        public override long Position
        {
            get => _position;
            set => Throw.NotSupportedException();
        }

        public CheckedWriteStreamOnROSBytes( ReadOnlySequence<byte> refBytes )
        {
            _refBytes = refBytes;
            _initialLength = refBytes.Length;
        }

        public override void Flush() { }

        public override int Read( byte[] buffer, int offset, int count ) => Throw.NotSupportedException<int>();

        public override long Seek( long offset, SeekOrigin origin ) => Throw.NotSupportedException<long>();

        public override void SetLength( long value ) => Throw.NotSupportedException();

        public override void Write( byte[] buffer, int offset, int count ) => Write( buffer.AsSpan( offset, count ) );

        public override void Write( ReadOnlySpan<byte> buffer )
        {
            if( _hasDiff ) return;
            if( (_position += buffer.Length) > _initialLength )
            {
                _position -= buffer.Length;
                _longerThanRef = _hasDiff = true;
                if( ThrowArgumentException ) Throw.ArgumentException( $"Rewrite is longer than first write: length = {_initialLength}." );
            }
            else
            {
                var r = new SequenceReader<byte>( _refBytes );
                if( r.IsNext( buffer, advancePast: true ) )
                {
                    _refBytes = r.UnreadSequence;
                }
                else
                {
                    _diffAtPosition = _hasDiff = true;
                    _position -= buffer.Length;
                    for( int i = 0; i < buffer.Length; ++i )
                    {
                        r.TryRead( out var b );
                        if( b != buffer[i] )
                        {
                            _position += i;
                            if( ThrowArgumentException ) Throw.ArgumentException( $"Write stream differ @{_position}. Expected byte '{b}', got '{buffer[i]}' (length = {_initialLength})." );
                            break;
                        }
                    }
                }
            }
        }

        public override Result GetResult()
        {
            if( _longerThanRef ) return Result.LongerThanRefBytes;
            if( _diffAtPosition ) return Result.HasByteDifference;
            Debug.Assert( !_hasDiff );
            if( _position < _initialLength )
            {
                if( ThrowArgumentException ) Throw.ArgumentException( $"Rewrite is shorter than first write: expected {_initialLength} bytes, got only {_position}." );
                return Result.ShorterThanRefBytes;
            }
            return Result.None;
        }
    }

    /// <summary>
    /// Creates <see cref="CheckedWriteStream"/> with its reference bytes as a <see cref="ReadOnlySequence{T}"/>.
    /// </summary>
    /// <param name="refBytes">The reference bytes.</param>
    /// <returns>A checked write stream.</returns>
    public static CheckedWriteStream Create( ReadOnlySequence<byte> refBytes ) => new CheckedWriteStreamOnROSBytes( refBytes );

    /// <summary>
    /// Creates <see cref="CheckedWriteStream"/> with its reference bytes from a <see cref="RecyclableMemoryStream"/>.
    /// </summary>
    /// <param name="s">The reference stream.</param>
    /// <returns>A checked write stream.</returns>
    public static CheckedWriteStream Create( RecyclableMemoryStream s ) => new CheckedWriteStreamOnROSBytes( s.GetReadOnlySequence() );

}
