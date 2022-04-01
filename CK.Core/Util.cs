using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        /// Unix Epoch (1st of January 1970).
        /// </summary>
        public static readonly DateTime UnixEpoch  = new DateTime(1970,1,1);

        /// <summary>
        /// Sql Server Epoch (1st of January 1900): this is the 0 legacy date time.
        /// </summary>
        public static readonly DateTime SqlServerEpoch  = new DateTime(1900,1,1);

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

    }
}
