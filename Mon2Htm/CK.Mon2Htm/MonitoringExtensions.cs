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
