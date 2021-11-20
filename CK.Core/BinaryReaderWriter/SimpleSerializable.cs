using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Simple <see cref="Serialize(ICKSimpleBinarySerializable)"/> and <see cref="Deserialize{T}(byte[])"/>.
    /// </summary>
    public static class SimpleSerializable
    {
        /// <summary>
        /// Serializes a <see cref="ICKSimpleBinarySerializable"/> as a byte array.
        /// </summary>
        /// <param name="o">This object.</param>
        /// <returns>The serialized object.</returns>
        public static byte[] Serialize( this ICKSimpleBinarySerializable o )
        {
            using( var m = new MemoryStream( 4096 ) )
            using( var w = new CKBinaryWriter( m ) )
            {
                o.Write( w );
                w.Flush();
                return m.ToArray();
            }
        }

        /// <summary>
        /// Desrializes a <see cref="ICKSimpleBinarySerializable"/> from serialized bytes.
        /// </summary>
        /// <typeparam name="T">The object's type.</typeparam>
        /// <param name="bytes">Serialized bytes.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>( byte[] bytes )
        {
            using( var m = new MemoryStream( bytes ) )
            using( var w = new CKBinaryReader( m ) )
            {
                return (T)Activator.CreateInstance( typeof( T ), w )!;
            }
        }
    }
}
