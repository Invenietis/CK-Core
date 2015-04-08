using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    public static class StringAndStringBuilderExtension
    {
        /// <summary>
        /// Concatenates multiple strings with an internal separator.
        /// </summary>
        /// <param name="this">Set of strings.</param>
        /// <param name="separator">The separator string.</param>
        /// <returns>The joined string.</returns>
        public static string Concatenate( this IEnumerable<string> @this, string separator = ", " )
        {
            return new StringBuilder().Append( @this, separator ).ToString();
        }

        /// <summary>
        /// Appends a set of strings with an internal separator.
        /// </summary>
        /// <param name="this">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="strings">Set of strings.</param>
        /// <param name="separator">The separator string.</param>
        /// <returns>The builder itself.</returns>
        public static StringBuilder Append( this StringBuilder @this, IEnumerable<string> strings, string separator = ", " )
        {
            using( var e = strings.GetEnumerator() )
            {
                if( e.MoveNext() )
                {
                    @this.Append( e.Current );
                    while( e.MoveNext() )
                    {
                        @this.Append( separator ).Append( e.Current );
                    }
                }
            }
            return @this;
        }

    }
}
