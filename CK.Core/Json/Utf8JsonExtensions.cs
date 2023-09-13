using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace CK.Core
{
    /// <summary>
    /// Provides extensions Json reader and writer objects.
    /// </summary>
    public static class Utf8JsonExtensions
    {
        /// <summary>
        /// Throws a <see cref="JsonException"/> with the current <see cref="Utf8JsonReader.BytesConsumed"/> and <see cref="Utf8JsonReader.CurrentDepth"/>.
        /// </summary>
        /// <param name="r">This reader.</param>
        /// <param name="message">The exception message.</param>
        [DoesNotReturn]
        public static void ThrowJsonException( this ref Utf8JsonReader r, string message )
        {
            throw new JsonException( $"{message} - {r.BytesConsumed} consumed bytes, current depth is {r.CurrentDepth}." );
        }

        /// <summary>
        /// Extends <see cref="Utf8JsonReader.Read()"/> to call <see cref="IUtf8JsonReaderContext.ReadMoreData(ref Utf8JsonReader)"/> as needed.
        /// </summary>
        /// <param name="r">This reader.</param>
        /// <param name="context">The reader context.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ReadWithMoreData( this ref Utf8JsonReader r, IUtf8JsonReaderContext context )
        {
            if( !r.Read() ) context.ReadMoreData( ref r );
        }

        /// <summary>
        /// Implements <see cref="Utf8JsonReader.Skip()"/> to call <see cref="IUtf8JsonReaderContext.SkipMoreData(ref Utf8JsonReader)"/> as needed.
        /// </summary>
        /// <param name="r">This reader.</param>
        /// <param name="context">The reader context.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SkipWithMoreData( this ref Utf8JsonReader r, IUtf8JsonReaderContext context )
        {
            if( !r.TrySkip() ) context.SkipMoreData( ref r );
        }

        /// <summary>
        /// Skips 0 or more <see cref="JsonTokenType.Comment"/>.
        /// </summary>
        /// <param name="r">This reader.</param>
        /// <param name="context">The reader context.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SkipComments( this ref Utf8JsonReader r, IUtf8JsonReaderContext context )
        {
            while( r.TokenType == JsonTokenType.Comment ) r.ReadWithMoreData( context );
        }

    }
}
