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
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckState( [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckStateException( exp! );
            }
        }

        /// <summary>
        /// Throws a new <see cref="InvalidOperationException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="message">An explicit message that replaces the default "Invalid state: ... should be true.".</param>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckState( string message, [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckStateException( exp!, message );
            }
        }

        [DoesNotReturn]
        static void CheckStateException( string exp, string? message = null )
        {
            if( message == null )
            {
                InvalidOperationException( $"Invalid state: '{exp}' should be true." );
            }
            else
            {
                InvalidOperationException( $"{message} (Expression: '{exp}')" );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.InvalidOperationException"/>.
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
