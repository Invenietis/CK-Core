using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    public partial class Throw
    {
        /// <summary>
        /// Throws a new <see cref="InvalidOperationException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [StackTraceHidden]
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
        [StackTraceHidden]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckState( string message, [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckStateException( exp!, message );
            }
        }

        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
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
        [StackTraceHidden]
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void InvalidOperationException( string? message = null, Exception? innerException = null )
        {
            InvalidOperationException<object>( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.InvalidOperationException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [StackTraceHidden]
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T InvalidOperationException<T>( string? message = null, Exception? innerException = null )
        {
            throw new InvalidOperationException( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.ObjectDisposedException"/>.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [StackTraceHidden]
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ObjectDisposedException( string? message, Exception? innerException )
        {
            ObjectDisposedException<object>( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.ObjectDisposedException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [StackTraceHidden]
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T ObjectDisposedException<T>( string? message, Exception? innerException )
        {
            throw new ObjectDisposedException( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.ObjectDisposedException"/>.
        /// </summary>
        /// <param name="objectName">The name of the disposed object.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        [StackTraceHidden]
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ObjectDisposedException( string? objectName = null, string? message = null )
        {
            ObjectDisposedException<object>( objectName, message );
        }

        /// <summary>
        /// Throws a new <see cref="System.ObjectDisposedException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="objectName">The name of the disposed object.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        [StackTraceHidden]
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T ObjectDisposedException<T>( string? objectName = null, string? message = null )
        {
            if( message == null ) throw new ObjectDisposedException( objectName );
            throw new ObjectDisposedException( objectName, message );
        }

        /// <summary>
        /// Throws a new <see cref="System.TimeoutException"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [StackTraceHidden]
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void TimeoutException( string? message, Exception? innerException )
        {
            TimeoutException<object>( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.TimeoutException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [StackTraceHidden]
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T TimeoutException<T>( string? message, Exception? innerException )
        {
            throw new TimeoutException( message, innerException );
        }

    }
}
