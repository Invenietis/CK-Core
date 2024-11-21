using System;
using System.IO;

namespace CK.Core.Tests;

public partial class Utf8JsonStreamReaderTests
{
    public class SlicedStream : Stream
    {
        readonly Stream _inner;
        readonly ReadMode _mode;

        public enum ReadMode
        {
            OneByte,
            TwoBytes,
            Random,
            Full
        }

        public SlicedStream( Stream inner, ReadMode mode )
        {
            _inner = inner;
            _mode = mode;
        }

        public bool IsDisposed { get; private set; }

        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                IsDisposed = true;
                _inner.Dispose();
            }
            base.Dispose( disposing );
        }

        public override bool CanRead => true;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _inner.Length;

        public override long Position { get => _inner.Position; set => _inner.Position = value; }

        public override long Seek( long offset, SeekOrigin origin ) => _inner.Seek( offset, origin );

        public override void SetLength( long value ) => _inner.SetLength( value );

        public override void Write( byte[] buffer, int offset, int count ) => _inner.Write( buffer, offset, count );

        public override void Flush() => _inner.Flush();

        public override int Read( byte[] buffer, int offset, int count ) => Read( buffer.AsSpan( offset, count ) );

        public override int Read( Span<byte> buffer )
        {
            int len = Math.Min( buffer.Length, _mode switch
            {
                ReadMode.OneByte => 1,
                ReadMode.TwoBytes => 2,
                ReadMode.Full => buffer.Length,
                _ => Random.Shared.Next( 10 ) + 1
            } );
            if( len == 0 ) return 0;
            var s = buffer.Slice( 0, len );
            return _inner.Read( s );
        }
    }
}
