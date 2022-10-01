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
        public static void CheckNotNullArgument( [NotNull] object? value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value == null )
            {
                ArgumentNullException( exp! );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentNullException"/> if the value is null.
        /// (This overload avoids any boxing of the value type.)
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckNotNullArgument<T>( [NotNull] T? value, [CallerArgumentExpression( "value" )] string? exp = null ) where T : struct
        {
            if( !value.HasValue )
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
        public static void CheckNotNullArgument( string message, [NotNull] object? value, [CallerArgumentExpression( "value" )] string? exp = null )
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
        /// Throws a new <see cref="System.ArgumentOutOfRangeException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckOutOfRangeArgument( [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckOutOfRangeArgumentException( exp! );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.IndexOutOfRangeException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckIndexOutOfRange( [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckOutOfRangeArgumentException( exp!, index: true );
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

        /// <summary>
        /// Throws a new <see cref="System.ArgumentOutOfRangeException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="message">Explicit message.</param>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckOutOfRangeArgument( string message, [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckOutOfRangeArgumentException( exp!, message );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.IndexOutOfRangeException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="message">Explicit message.</param>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckIndexOutOfRange( string message, [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckOutOfRangeArgumentException( exp!, message, true );
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

        [DoesNotReturn]
        static void CheckOutOfRangeArgumentException( string exp, string? message = null, bool index = false )
        {
            if( message == null )
            {
                if( index ) IndexOutOfRangeException( $"'{exp}' should be true." );
                ArgumentOutOfRangeException( null!, $"Invalid argument: '{exp}' should be true." );
            }
            else
            {
                if( index ) IndexOutOfRangeException( $"{message} ('{exp}' should be true)." );
                ArgumentOutOfRangeException( exp, message );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the string value is null or empty.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckNotNullOrEmptyArgument( [NotNull] string? value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( String.IsNullOrEmpty( value ) )
            {
                NullOrEmptyException( value, exp! );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the enumerable is null or empty.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckNotNullOrEmptyArgument<T>( [NotNull] IEnumerable<T>? value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value == null || !value.Any() )
            {
                NullOrEmptyException( value, exp! );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the non generic enumerable is null or empty.
        /// This is not aggressively inlined since using non generic enumerable is considered obsolete. This is
        /// here to ensure a full coverage of the IEnumerable stuff.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        public static void CheckNotNullOrEmptyArgument( [NotNull] System.Collections.IEnumerable? value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value == null )
            {
                ArgumentNullException( exp! );
            }
            else
            {
                // .NetFramework v1 non generic enumerator was not IDisposable, but now
                // it may be...
                System.Collections.IEnumerator e = value.GetEnumerator();
                try
                {
                    if( !e.MoveNext() )
                    {
                        ArgumentException( exp!, "Must not be null or empty." );
                    }
                }
                finally
                {
                    if( e is IDisposable d ) d.Dispose();
                }
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the collection is null or empty.
        /// <para>
        /// This is more efficient for <see cref="IReadOnlyCollection{T}"/> is supported than the IEnumerable overload:
        /// the compiler does the job.
        /// </para>
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckNotNullOrEmptyArgument<T>( [NotNull] IReadOnlyCollection<T>? value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value == null || value.Count == 0 )
            {
                NullOrEmptyException( value, exp! );
            }
        }

        [DoesNotReturn]
        static void NullOrEmptyException<T>( T? value, string exp )
        {
            if( value is null )
            {
                ArgumentNullException( exp );
            }
            else
            {
                ArgumentException( exp, "Must not be null or empty." );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the span is empty.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckNotNullOrEmptyArgument<T>( Span<T> value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value.IsEmpty )
            {
                ArgumentException( exp!, "Must not be empty." );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the memory is empty.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckNotNullOrEmptyArgument<T>( Memory<T> value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value.IsEmpty )
            {
                ArgumentException( exp!, "Must not be empty." );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the read only span is empty.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckNotNullOrEmptyArgument<T>( ReadOnlySpan<T> value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value.IsEmpty )
            {
                ArgumentException( exp!, "Must not be empty." );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the read only memory is empty.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckNotNullOrEmptyArgument<T>( ReadOnlyMemory<T> value, [CallerArgumentExpression( "value" )] string? exp = null )
        {
            if( value.IsEmpty )
            {
                ArgumentException( exp!, "Must not be empty." );
            }
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> if the string value is null, empty or whitespace.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckNotNullOrWhiteSpaceArgument( [NotNull] string? value, [CallerArgumentExpression( "value" )] string? exp = null )
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
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ArgumentOutOfRangeException( string name, string? message = null )
        {
            ArgumentOutOfRangeException<object>( name, message );
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentOutOfRangeException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">Optional message to include in the exception.</param>
        [DoesNotReturn]
        public static T ArgumentOutOfRangeException<T>( string name, string? message = null )
        {
            throw new ArgumentOutOfRangeException( name, message );
        }

        /// <summary>
        /// Throws a new <see cref="System.IndexOutOfRangeException"/>.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="message">Optional inner exception.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void IndexOutOfRangeException( string? message = null, Exception? inner = null )
        {
            IndexOutOfRangeException<object>( message );
        }

        /// <summary>
        /// Throws a new <see cref="System.IndexOutOfRangeException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="message">Optional inner exception.</param>
        [DoesNotReturn]
        public static T IndexOutOfRangeException<T>( string? message = null, Exception? inner = null )
        {
            throw new IndexOutOfRangeException( message, inner );
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/>.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">Optional message to include in the exception.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ArgumentException( string name, string? message = null )
        {
            ArgumentException<object>( name, message );
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">Optional message to include in the exception.</param>
        [DoesNotReturn]
        public static T ArgumentException<T>( string name, string? message = null )
        {
            throw new ArgumentException( message, name );
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentNullException"/>.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">Optional message to include in the exception.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ArgumentNullException( string name, string? message = null )
        {
            ArgumentNullException<object>( name, message );
        }

        /// <summary>
        /// Throws a new <see cref="System.ArgumentNullException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">Optional message to include in the exception.</param>
        [DoesNotReturn]
        public static T ArgumentNullException<T>( string name, string? message = null )
        {
            throw new ArgumentNullException( name, message );
        }


    }
}
