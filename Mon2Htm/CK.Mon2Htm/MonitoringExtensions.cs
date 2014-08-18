#region LGPL License
/*----------------------------------------------------------------------------
* This file (Mon2Htm\CK.Mon2Htm\MonitoringExtensions.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.Mon2Htm
{
    public static class MonitoringExtensions
    {
        /// <summary>
        /// Returns a byte array (size 9), including timestamp binary and uniquifier.
        /// </summary>
        /// <param name="t">DateTimeStamp to use.</param>
        /// <returns>byte[9] array.</returns>
        public static byte[] ToBytes( this DateTimeStamp t )
        {
            byte[] output = new byte[9];

            Debug.Assert( BitConverter.GetBytes( t.TimeUtc.ToBinary() ).Length == 8 );

            BitConverter.GetBytes( t.TimeUtc.ToBinary() ).CopyTo( output, 0 );
            output[8] = t.Uniquifier;

            return output;
        }

        /// <summary>
        /// Returns a Base64 representation of a DateTimeStamp.
        /// </summary>
        /// <param name="t">DateTimeStamp to use.</param>
        /// <returns>Base64 representation.</returns>
        public static string ToBase64String( this DateTimeStamp t )
        {
            return Convert.ToBase64String( t.ToBytes() );
        }

        public static DateTimeStamp CreateDateTimeStampFromBytes( byte[] bytes )
        {
            if( bytes.Length != 9 ) throw new ArgumentException( "Given byte[] does not have the correct size (9).", "bytes" );

            return new DateTimeStamp( DateTime.FromBinary( BitConverter.ToInt64( bytes, 0 ) ), bytes[8] );
        }

        public static DateTimeStamp CreateDateTimeStampFromBase64( string base64string )
        {
            return CreateDateTimeStampFromBytes( Convert.FromBase64String( base64string ) );
        }
    }
}
