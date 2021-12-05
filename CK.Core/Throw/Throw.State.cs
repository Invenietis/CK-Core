using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    public partial class Throw
    {
        /// <summary>
        /// Throws a new <see cref="InvalidOperationException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="valid">The expression to that must be true.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckState( [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckStateException( exp! );
            }
        }

        [DoesNotReturn]
        static void CheckStateException( string exp )
        {
            InvalidOperationException( $"Invalid state: '{exp}' should be true." );
        }

        /// <summary>
        /// Throws a new <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        public static void InvalidOperationException( string? message = null, Exception? innerException = null )
        {
            throw new InvalidOperationException( message, innerException );
        }

    }
}
