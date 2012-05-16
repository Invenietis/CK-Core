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
    public static class UriExtension
    {
        /// <summary>
        /// Ensures that the given parameter occurs in this <see cref="Uri"/> with the given value.
        /// </summary>
        /// <param name="this">This uri.</param>
        /// <param name="parameter">Name of the parameter.</param>
        /// <param name="val">Value of the parameter. It must be url safe since this method 
        /// will not escape it.</param>
        /// <returns>A new <see cref="Uri"/> with the appended or updated parameter.</returns>
        static public Uri AssumeUrlParameter( this Uri @this, string parameter, string val )
        {
            return new Uri( UriHelper.AssumeUrlParameter( @this.AbsoluteUri, parameter, val ) );
        }

        /// <summary>
        /// Removes the given parameter from the <see cref="Uri"/>.
        /// </summary>
        /// <param name="this">This uri.</param>
        /// <param name="parameter">Parameter name to remove.</param>
        /// <returns>A new <see cref="Uri"/> without the parameter.</returns>
        static public Uri RemoveUrlParameter( this Uri @this, string parameter )
        {
            return new Uri( UriHelper.RemoveUrlParameter( @this.AbsoluteUri, parameter ) );
        }

        /// <summary>
        /// Appends the given parameter and value to this <see cref="Uri"/>. If the parameter name already exists
        /// in the uri (and you do not want duplicated parameters), use <see cref="AssumeUrlParameter"/>
        /// instead.
        /// </summary>
        /// <param name="this">This uri</param>
        /// <param name="parameter">Name of the parameter.</param>
        /// <param name="val">Value of the parameter. It must be url safe since this method 
        /// will not escape it.</param>
        /// <returns>A new <see cref="Uri"/> with the parameter and value added.</returns>
        static public Uri AppendUrlParameter( this Uri @this, string parameter, string val )
        {
            return new Uri( UriHelper.AppendUrlParameter( @this.AbsoluteUri, parameter, val ) );
        }
    }
}
