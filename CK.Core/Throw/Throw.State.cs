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

        /// <summary>
        /// Throws a new <see cref="System.ObjectDisposedException"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        public static void ObjectDisposedException( string? message, Exception? innerException )
        {
            throw new ObjectDisposedException( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.ObjectDisposedException"/>.
        /// </summary>
        /// <param name="objectName">Optional message to include in the exception.</param>
        /// <param name="message">Optional object name to include in the exception.</param>
        [DoesNotReturn]
        public static void ObjectDisposedException( string? objectName = null, string? message = null )
        {
            throw new ObjectDisposedException( objectName, message );
        }

        /// <summary>
        /// Throws a new <see cref="System.ObjectDisposedException"/> if <paramref name="isDisposed"/> expression is <see langword="true"/>.
        /// </summary>
        /// <param name="message">An explicit message that replaces the default "Invalid state: ... should be true.".</param>
        /// <param name="isDisposed">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckDisposed( [DoesNotReturnIf( true )] bool isDisposed, string? message = null, [CallerArgumentExpression( "isDisposed" )] string? exp = null )
        {
            if( isDisposed )
            {
                ObjectDisposedException( message: message ?? exp );
            }
        }
    }
}
