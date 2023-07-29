using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    public partial class Throw
    {
        /// <summary>
        /// <see cref="Debug.Assert(bool)"/> replacement that captures the expression, file path and
        /// line number and throws a <see cref="CKException"/>.
        /// </summary>
        /// <param name="valid">Must be true.</param>
        /// <param name="exp">Automatically captures <paramref name="valid"/> expression source text.</param>
        /// <param name="filePath">Automatically captures the source code file path.</param>
        /// <param name="lineNumber">Automatically captures the source code line number.</param>
        /// <exception cref="CKException">Whenever <paramref name="valid"/> is false.</exception>
        [Conditional( "DEBUG" )]
        public static void DebugAssert( [DoesNotReturnIf( false )] bool valid,
                                        [CallerArgumentExpression( "valid" )] string? exp = null,
                                        [CallerFilePath] string? filePath = null,
                                        [CallerLineNumber] int lineNumber = 0 )
        {
            if( !valid )
            {
                Throw.CKException( $"Debug Assertion failed '{exp}', {filePath}@{lineNumber}." );
            }
        }
    }

}
