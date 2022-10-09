using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CK.Core
{
    public partial class Throw
    {
        /// <summary>
        /// Throws a new <see cref="InvalidDataException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckData( [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckDataException( exp! );
            }
        }

        /// <summary>
        /// Throws a new <see cref="InvalidDataException"/> if <paramref name="valid"/> expression is false.
        /// </summary>
        /// <param name="message">An explicit message that replaces the default "Invalid data: ... should be true.".</param>
        /// <param name="valid">The expression to that must be true.</param>
        /// <param name="exp">Roslyn's automatic capture of the expression's value.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CheckData( string message, [DoesNotReturnIf( false )] bool valid, [CallerArgumentExpression( "valid" )] string? exp = null )
        {
            if( !valid )
            {
                CheckDataException( exp!, message );
            }
        }

        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        static void CheckDataException( string exp, string? message = null )
        {
            if( message == null )
            {
                InvalidDataException( $"Invalid data: '{exp}' should be true." );
            }
            else
            {
                InvalidDataException( $"{message} (Expression: '{exp}')" );
            }
        }


        /// <summary>
        /// Throws a new <see cref="System.IO.InvalidDataException"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void InvalidDataException( string? message = null, Exception? innerException = null )
        {
            InvalidDataException<object>( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.IO.InvalidDataException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T InvalidDataException<T>( string? message = null, Exception? innerException = null )
        {
            throw new InvalidDataException( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.IO.EndOfStreamException"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void EndOfStreamException( string? message = null, Exception? innerException = null )
        {
            EndOfStreamException<object>( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.IO.EndOfStreamException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T EndOfStreamException<T>( string? message = null, Exception? innerException = null )
        {
            throw new EndOfStreamException( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.Xml.XmlException"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void XmlException( string? message = null, Exception? innerException = null )
        {
            XmlException<object>( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.Xml.XmlException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T XmlException<T>( string? message = null, Exception? innerException = null )
        {
            throw new XmlException( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.FormatException"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void FormatException( string? message = null, Exception? innerException = null )
        {
            FormatException<object>( message, innerException );
        }

        /// <summary>
        /// Throws a new <see cref="System.FormatException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="innerException">Optional inner <see cref="Exception"/> to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T FormatException<T>( string? message = null, Exception? innerException = null )
        {
            throw new FormatException( message, innerException );
        }

    }
}
