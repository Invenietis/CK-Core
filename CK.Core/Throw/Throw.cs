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
        /// Throws a new <see cref="Exception"/>.
        /// </summary>
        /// <param name="message">Optional message to include in the exception.</param>
        /// <param name="inner">Optional inner exception to include.</param>
        [DoesNotReturn]
        public static void Exception( string? message = null, Exception? inner = null )
        {
            throw new Exception( message );
        }


    }
}
