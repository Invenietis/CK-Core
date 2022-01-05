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
        public static void Exception( string? message = null, Exception? inner = null )
        {
            throw new Exception( message, inner );
        }

        /// <summary>
        /// Throws a new <see cref="System.NotSupportedException"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="inner">Optional inner exception to include.</param>
        [DoesNotReturn]
        public static void NotSupportedException( string? message = null, Exception? inner = null )
        {
            throw new NotSupportedException( message, inner );
        }


    }
}
