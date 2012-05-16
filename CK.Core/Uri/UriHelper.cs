using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace CK.Core
{
    /// <summary>
    /// Helper functions related to <see cref="Uri"/>.
    /// </summary>
    public static class UriHelper
    {
        /// <summary>
        /// When val is null, the parameter will be removed.
        /// </summary>
        static private string AddUrlParameter( string u, string parameter, string val, bool merge )
        {
            if( parameter == null || parameter.Length == 0 ) return u;
            int posMark = u.IndexOf( '?' );
            if( val == null ) merge = true;
            else
            {
                if( posMark < 0 ) return u + '?' + parameter + '=' + val;
                if( posMark == u.Length - 1 ) return u + parameter + '=' + val;
            }
            if( merge )
            {
                Regex r = new Regex( @"(&|\?)" + parameter + @"((?<1>=[^&]*))?(&|$)", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase );
                Match m = r.Match( u, posMark );
                if( m.Success )
                {
                    if( val != null )
                    {
                        Group g = m.Groups[1];
                        int iParam = m.Index + parameter.Length + 1;
                        return u.Substring( 0, iParam ) + '=' + val + u.Substring( iParam + g.Length );
                    }
                    return posMark == m.Index ? u.Remove( m.Index + 1, m.Length - 1 ) : (u.Length != m.Index + m.Length ? u.Remove( m.Index, m.Length - 1 ) : u.Remove( m.Index, m.Length ));
                }
            }
            return val != null ? u + '&' + parameter + '=' + val : u;
        }

        /// <summary>
        /// Ensures that the given parameter occurs in the query string with the given value.
        /// </summary>
        /// <param name="u">Url that may already contain the parameter: in such case, its value
        /// will be updated.</param>
        /// <param name="parameter">Name of the parameter.</param>
        /// <param name="val">Value of the parameter. It must be url safe since this method 
        /// will not escape it.</param>
        /// <returns>The url with the appended or updated parameter.</returns>
        static public string AssumeUrlParameter( string u, string parameter, string val )
        {
            if( val == null ) val = String.Empty;
            return AddUrlParameter( u, parameter, val, true );
        }

        /// <summary>
        /// Removes the given parameter from the url.
        /// </summary>
        /// <param name="u">Original url</param>
        /// <param name="parameter">Parameter name to remove.</param>
        /// <returns>An url without the parameter.</returns>
        static public string RemoveUrlParameter( string u, string parameter )
        {
            return AddUrlParameter( u, parameter, null, true );
        }

        /// <summary>
        /// Appends the given parameter and value to the url. If the parameter name already exists
        /// in the url (and you do not want duplicated parameters), use <see cref="M:AssumeUrlParameter"/>
        /// instead.
        /// </summary>
        /// <param name="u">Url</param>
        /// <param name="parameter">Name of the parameter.</param>
        /// <param name="val">Value of the parameter. It must be url safe since this method 
        /// will not escape it.</param>
        /// <returns>An url with the parameter and value added.</returns>
        static public string AppendUrlParameter( string u, string parameter, string val )
        {
            if( val == null ) val = String.Empty; 
            return AddUrlParameter( u, parameter, val, false );
        }

    }
}
