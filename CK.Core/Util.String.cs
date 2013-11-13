#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\Util.String.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Dynamic;
using System.ComponentModel;

namespace CK.Core
{
    /// <summary>
    /// Utility class.
    /// </summary>
    static public partial class Util
    {
        /// <summary>
        /// Offers extensibility methods for strings.
        /// </summary>
        static public class String
        {
            const string OpenTag = "{{";
            const string CloseTag = "}}";

            static Regex _namedFormatRegex = new Regex( string.Format( "{0}(?<token>.*?){1}", OpenTag, CloseTag ) );
            static Regex _validPlaceHolderName = new Regex( "^([a-zA-Z0-9])+$" );

            /// <summary>
            /// String.Format that works with named place holders like {{hello}} instead of {0}
            /// </summary>
            /// <param name="format">The format string</param>
            /// <param name="values">An object that will give named parameters values</param>
            /// <returns>Formatted string</returns>
            public static string NamedFormat( string format, object values )
            {
                if( string.IsNullOrEmpty( format ) )
                    throw new ArgumentNullException( "format" );
                if( values == null )
                    throw new ArgumentNullException( "values" );

                int lastIdx = 0;
                StringBuilder result = new StringBuilder();
                var match = _namedFormatRegex.Match( format );
                var valuesObjectProperties = TypeDescriptor.GetProperties( values );

                while( match.Success )
                {
                    Group token = match.Groups["token"];
                    if( !_validPlaceHolderName.IsMatch( token.Value ) )
                        throw new ArgumentException( string.Format( "The value '{0}' is not a valid named placeholder", token.Value ) );

                    int tokenIdx = token.Index - OpenTag.Length;
                    int tokenLength = token.Length + (OpenTag.Length + CloseTag.Length);

                    result.Append( format.Substring( lastIdx, tokenIdx - lastIdx ) );

                    var propDescriptor = valuesObjectProperties.Find( token.Value, false );
                    if( propDescriptor == null )
                        throw new ArgumentException( string.Format( "Unable to find the property '{0}' on the given value object", token.Value ) );

                    result.Append( propDescriptor.GetValue( values ) );
                    

                    lastIdx = tokenIdx + tokenLength;

                    match = match.NextMatch();
                }

                if( lastIdx < format.Length )
                    result.Append( format.Substring( lastIdx ) );

                return result.ToString();
            }
        }
    }
}
