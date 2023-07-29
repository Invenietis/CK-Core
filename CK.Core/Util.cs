using Microsoft.IO;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Utility class.
    /// Offers useful functions, constants, singletons and delegates.
    /// </summary>
    public static partial class Util
    {
        /// <summary>
        /// Represents the smallest possible value for a <see cref="DateTime"/> in <see cref="DateTimeKind.Utc"/>.         
        /// </summary>
        static public readonly DateTime UtcMinValue = new DateTime( 0L, DateTimeKind.Utc );

        /// <summary>
        /// Represents the largest possible value for a <see cref="DateTime"/> in <see cref="DateTimeKind.Utc"/>.         
        /// </summary>
        static public readonly DateTime UtcMaxValue = new DateTime( 0x2bca2875f4373fffL, DateTimeKind.Utc );

        /// <summary>
        /// Centralized <see cref="IDisposable.Dispose"/> action call: it adapts an <see cref="IDisposable"/> interface to an <see cref="Action"/>.
        /// Can be safely called if <paramref name="obj"/> is null. 
        /// See <see cref="CreateDisposableAction"/> to wrap an action in a <see cref="IDisposable"/> interface.
        /// </summary>
        /// <param name="obj">The disposable object to dispose (can be null).</param>
        public static void ActionDispose( IDisposable obj ) => obj?.Dispose();

        sealed class DisposableAction : IDisposable
        {
            public Action? A;
            public void Dispose()
            {
                Action? a = A;
                if( a != null && Interlocked.CompareExchange( ref A, null, a ) == a ) a();
            }
        }

        /// <summary>
        /// Wraps an action in a <see cref="IDisposable"/> interface
        /// Can be safely called if <paramref name="a"/> is null (the dispose call will do nothing) and in multi threaded context:
        /// the call to action will be done once and only once by the first call to dispose.
        /// See <see cref="ActionDispose"/> to adapt an IDisposable interface to an <see cref="Action"/>.
        /// </summary>
        /// <param name="a">The action to call when <see cref="IDisposable.Dispose"/> is called.</param>
        public static IDisposable CreateDisposableAction( Action? a ) => new DisposableAction() { A = a };

        sealed class VoidDisposable : IDisposable { public void Dispose() { } }

        /// <summary>
        /// A void, immutable, <see cref="IDisposable"/> that does absolutely nothing.
        /// </summary>
        public static readonly IDisposable EmptyDisposable = new VoidDisposable();

        /// <summary>
        /// Sql Server Epoch (1st of January 1900): this is the 0 legacy date time, the default value, even if
        /// datetime2 is like the .Net DateTime (0001-01-01 through 9999-12-31, 100ns step).
        /// Its <see cref="DateTimeKind.Unspecified"/> since this is what the Sql client returns.
        /// </summary>
        public static readonly DateTime SqlServerEpoch  = new DateTime( 599266080000000000, DateTimeKind.Unspecified );

        /// <summary>
        /// The <see cref="RecyclableStreamManager"/> is using 128 KiB blocks (small pool).
        /// </summary>
        public const int RecyclableStreamBlockSize = 128 * 1024;

        /// <summary>
        /// The <see cref="RecyclableStreamManager"/> large pool starts with 256 KiB buffers doubling up to 8 MiB (<see cref="RecyclableStreamMaximumBufferSize"/>):
        /// there will be 6 large buffers of 256 KiB, 512 KiB, 1 MiB, 2 MiB, 4 MiB, and 8 MiB.
        /// </summary>
        public const int RecyclableStreamLargeBufferMultiple = 256 * 1024;

        /// <summary>
        /// The <see cref="RecyclableStreamManager"/> will not keep buffers bigger than 8 MiB (large pool).
        /// </summary>
        public const int RecyclableStreamMaximumBufferSize = 8 * 1024 * 1024;

        /// <summary>
        /// The <see cref="RecyclableStreamManager"/> doubles the size of its buffers (large pool).
        /// </summary>
        public const bool RecyclableStreamUseExponentialLargeBuffer = true;

        /// <summary>
        /// Gets a default instance of <see cref="RecyclableMemoryStreamManager"/>. This manager is configured
        /// with at most 256 blocks of 128 KiB for the small pool and at most 32 MiB for its large pool.
        /// <para>
        /// This configuration should be fine as long as not too many big streams are required. However,
        /// there's no "one size fits all" here: the allocation and pool usage should be monitored when possible.
        /// </para>
        /// <para>
        /// The <see cref="RecyclableStreamMaximumSmallPoolFreeBytes"/> and <see cref="RecyclableStreamMaximumLargePoolFreeBytes"/>
        /// can be changed at any time to adjust the pool size. All other settings are immutable.
        /// </para>
        /// <para>
        /// Calling <see cref="RecyclableMemoryStream.ToArray()"/> is allowed (<see cref="RecyclableMemoryStreamManager.ThrowExceptionOnToArray"/> is let to false
        /// and should not be set tot true): small serializations into small buffers must often result in final byte array.
        /// ToArray should NOT be called on large payload...
        /// </para>
        /// </summary>
        public static RecyclableMemoryStreamManager RecyclableStreamManager = new RecyclableMemoryStreamManager( blockSize: RecyclableStreamBlockSize,
                                                                                                                 largeBufferMultiple: RecyclableStreamLargeBufferMultiple,
                                                                                                                 maximumBufferSize: RecyclableStreamMaximumBufferSize,
                                                                                                                 useExponentialLargeBuffer: RecyclableStreamUseExponentialLargeBuffer,
                                                                                                                 maximumSmallPoolFreeBytes: 256 * RecyclableStreamBlockSize,
                                                                                                                 maximumLargePoolFreeBytes: 32 * 1024 * 1024 );
        /// <summary>
        /// Gets or sets <see cref="RecyclableMemoryStreamManager.MaximumFreeSmallPoolBytes"/> of the default <see cref="RecyclableStreamManager"/>.
        /// Defaults to 256 * <see cref="RecyclableStreamBlockSize"/> (256 * 128 KiB).
        /// </summary>
        public static long RecyclableStreamMaximumSmallPoolFreeBytes
        {
            get => RecyclableStreamManager.MaximumFreeSmallPoolBytes;
            set => RecyclableStreamManager.MaximumFreeSmallPoolBytes = value;
        }

        /// <summary>
        /// Gets or sets <see cref="RecyclableMemoryStreamManager.MaximumFreeLargePoolBytes"/> of the default <see cref="RecyclableStreamManager"/>.
        /// Defaults to 32 MiB.
        /// </summary>
        public static long RecyclableStreamMaximumLargePoolFreeBytes
        {
            get => RecyclableStreamManager.MaximumFreeLargePoolBytes;
            set => RecyclableStreamManager.MaximumFreeLargePoolBytes = value;
        }

        /// <summary>
        /// The 0.0.0.0 Version.
        /// </summary>
        public static readonly Version EmptyVersion = new Version( 0, 0, 0, 0 );

        /// <summary>
        /// Creates a base64 url string using <see cref="System.Security.Cryptography.RandomNumberGenerator.Fill(Span{byte})"/>.
        /// </summary>
        /// <param name="len">Length of the random string.</param>
        /// <returns>A random string.</returns>
        public static string GetRandomBase64UrlString( int len )
        {
            const int MaxStackSize = 128;

            Throw.CheckArgument( len >= 0 );
            if( len == 0 ) return string.Empty;

            var requiredEntropy = 3 * len / 4 + 1;
            var safeSize = Base64.GetMaxEncodedToUtf8Length( requiredEntropy );

            byte[]? fromPool = null;
            Span<byte> buffer = safeSize > MaxStackSize
                                ? (fromPool = ArrayPool<byte>.Shared.Rent( safeSize )).AsSpan( 0, safeSize )
                                : stackalloc byte[safeSize];
            try
            {
                System.Security.Cryptography.RandomNumberGenerator.Fill( buffer.Slice( 0, requiredEntropy ) );
                Base64.EncodeToUtf8InPlace( buffer, requiredEntropy, out int bytesWritten );
                Base64UrlHelper.UncheckedBase64ToUrlBase64NoPadding( buffer, ref bytesWritten );
                Debug.Assert( bytesWritten > len );
                return Encoding.ASCII.GetString( buffer.Slice( 0, len ) );
            }
            finally
            {
                if( fromPool != null ) ArrayPool<byte>.Shared.Return( fromPool );
            }
        }

        /// <summary>
        /// Centralized void action call for any type. 
        /// This method is one of the safest method never written in the world. 
        /// It does absolutely nothing.
        /// </summary>
        /// <param name="obj">Any object.</param>
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
        public static void ActionVoid<T1, T2, T3>( T1 o1, T2 o2, T3 o3 )
        {
        }

        /// <summary>
        /// Centralized identity function for any type.
        /// </summary>
        /// <typeparam name="T">Type of the function parameter and return value.</typeparam>
        /// <param name="value">Any value returned unchanged.</param>
        /// <returns>The <paramref name="value"/> provided is returned as-is.</returns>
        public static T FuncIdentity<T>( T value ) => value;

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
        public static CheckedWriteStream CreateCheckedWriteStream( ReadOnlySequence<byte> refBytes ) => new CheckedWriteStreamOnROSBytes( refBytes );

        /// <summary>
        /// Creates <see cref="CheckedWriteStream"/> with its reference bytes from a <see cref="RecyclableMemoryStream"/>.
        /// </summary>
        /// <param name="s">The reference stream.</param>
        /// <returns>A checked write stream.</returns>
        public static CheckedWriteStream CreateCheckedWriteStream( RecyclableMemoryStream s ) => new CheckedWriteStreamOnROSBytes( s.GetReadOnlySequence() );


        static bool? _isGlobalizationInvariantMode;

        /// <summary>
        /// Whether the CultureInfo will always be the <see cref="CultureInfo.InvariantCulture"/>.
        /// See https://github.com/dotnet/runtime/blob/main/docs/design/features/globalization-invariant-mode.md.
        /// <para>
        /// This ugly code is required: see https://stackoverflow.com/questions/75298957/how-to-detect-globalization-invariant-mode
        /// </para>
        /// </summary>
        /// <returns>True if we are running in Invariant Mode, false otherwise.</returns>
        public static bool IsGlobalizationInvariantMode
        {
            get
            {
                return _isGlobalizationInvariantMode ??= TryIt();

                static bool TryIt()
                {
                    try
                    {
                        return CultureInfo.GetCultureInfo( "en-US" ).NumberFormat.CurrencySymbol == "¤";
                    }
                    catch( CultureNotFoundException )
                    {
                        return true;
                    }
                }
            }
        }

    }


}
