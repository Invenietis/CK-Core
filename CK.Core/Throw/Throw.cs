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
    /// <summary>
    /// Contains basic throw helpers and simple guards that use C# 10 <see cref="System.Runtime.CompilerServices.CallerArgumentExpressionAttribute"/>. 
    /// </summary>
    public partial class Throw
    {
        /// <summary>
        /// Throws a new <see cref="System.Exception"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="inner">Optional inner exception to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void Exception( string? message = null, Exception? inner = null )
        {
            Exception<object>( message, inner );
        }

        /// <summary>
        /// Throws a new <see cref="System.Exception"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="inner">Optional inner exception to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T Exception<T>( string? message = null, Exception? inner = null )
        {
            throw new Exception( message, inner );
        }

        /// <summary>
        /// Throws a new <see cref="System.NotSupportedException"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="inner">Optional inner exception to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void NotSupportedException( string? message = null, Exception? inner = null )
        {
            NotSupportedException<object>( message, inner );
        }

        /// <summary>
        /// Throws a new <see cref="System.NotSupportedException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="inner">Optional inner exception to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T NotSupportedException<T>( string? message = null, Exception? inner = null )
        {
            throw new NotSupportedException( message, inner );
        }

        /// <summary>
        /// Throws a new <see cref="CKException"/>.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Optional inner exception to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void CKException( string message, Exception? inner = null )
        {
            CKException<object>( message, inner );
        }

        /// <summary>
        /// Throws a new <see cref="CKException"/> but formally returns a <typeparamref name="T"/> value.
        /// Can be used in switch expressions or as a returned value.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Optional inner exception to include.</param>
        [DoesNotReturn]
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T CKException<T>( string message, Exception? inner = null )
        {
            throw new CKException( message, inner );
        }
    }
}
