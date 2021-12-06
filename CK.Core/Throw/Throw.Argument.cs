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
        /// Throws a new <see cref="System.ArgumentNullException"/> if the value is null.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void OnNullArgument( [NotNull] object? value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value == null )
            {
                ArgumentNullException( exp! );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentNullException"/> if the value is null.
        /// </summary>
        /// <param name="message">Specific message.</param>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void OnNullArgument( string message, [NotNull] object? value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value == null )
            {
                ArgumentNullException( exp!, message );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckArgument( [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckArgumentException( exp! );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="message">Explicit message.</param>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckArgument( string message, [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckArgumentException( exp!, message );
            }
        }

        [DoesNotReturn]
        static void CheckArgumentException( string exp, string? message = null )
        {
            if( message == null )
            {
                ArgumentException( null!, $"Invalid argument: '{exp}' should be true." );
            }
            else
            {
                ArgumentException( exp, message );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the string value is null or empty.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void OnNullOrEmptyArgument( [NotNull] string? value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( String.IsNullOrEmpty( value ) )
            {
                NullOrEmptyException( value, exp! );
            }
        }

        [DoesNotReturn]
        static void NullOrEmptyException( string? value, string exp )
        {
            if( value != null )
            {
                ArgumentException( exp, "Must not be null or empty." );
            }
            else
            {
                ArgumentNullException( exp );
            }
        }


        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the string value is null, empty or whitespace.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void OnNullOrWhiteSpaceArgument( [NotNull] string? value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( String.IsNullOrWhiteSpace( value ) )
            {
                NullOrWhiteSpaceException( value, exp! );
            }
        }

        [DoesNotReturn]
        static void NullOrWhiteSpaceException( string? value, string exp )
        {
            if( value != null )
            {
                ArgumentException( exp, "Must not be null, empty or whitespace." );
            }
            else
            {
                ArgumentNullException( exp );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">Optional message to include in the exception.</param>
        [DoesNotReturn]
        public static void ArgumentOutOfRangeException( string name, string? message = null )
        {
            throw new ArgumentOutOfRangeException( name, message );
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/>.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">Optional message to include in the exception.</param>
        [DoesNotReturn]
        public static void ArgumentException( string name, string? message = null )
        {
            throw new ArgumentException( message, name );
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentNullException"/>.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">Optional message to include in the exception.</param>
        [DoesNotReturn]
        public static void ArgumentNullException( string name, string? message = null )
        {
            throw new ArgumentNullException( name, message );
        }


    }
}
