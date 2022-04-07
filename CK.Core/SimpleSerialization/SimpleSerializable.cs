using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Simple <see cref="SerializeSimple(ICKSimpleBinarySerializable)"/> and <see cref="DeserializeSimple{T}(byte[])"/>.
    /// </summary>
    public static class SimpleSerializable
    {
        /// <summary>
        /// Serializes a <see cref="ICKSimpleBinarySerializable"/> as a byte array.
        /// </summary>
        /// <param name="o">This object.</param>
        /// <returns>The serialized object.</returns>
        public static byte[] SerializeSimple( this ICKSimpleBinarySerializable o )
        {
            using( var s = new MemoryStream( 4096 ) )
            using( var w = new CKBinaryWriter( s, Encoding.UTF8, true ) )
            {
                o.Write( w );
                w.Flush();
                return s.ToArray();
            }
        }

        /// <summary>
        /// Serializes a <see cref="ICKVersionedBinarySerializable"/> as a byte array.
        /// </summary>
        /// <param name="o">This object.</param>
        /// <returns>The serialized object.</returns>
        public static byte[] SerializeVersioned( this ICKVersionedBinarySerializable o )
        {
            using( var s = new MemoryStream( 4096 ) )
            using( var w = new CKBinaryWriter( s, Encoding.UTF8, true ) )
            {
                w.WriteNonNegativeSmallInt32( SerializationVersionAttribute.GetRequiredVersion( o.GetType() ) );
                o.WriteData( w );
                w.Flush();
                return s.ToArray();
            }
        }

        /// <summary>
        /// Deserializes a <see cref="ICKSimpleBinarySerializable"/> from serialized bytes.
        /// </summary>
        /// <typeparam name="T">The object's type.</typeparam>
        /// <param name="bytes">Serialized bytes.</param>
        /// <returns>The deserialized object.</returns>
        public static T DeserializeSimple<T>( byte[] bytes ) where T : ICKSimpleBinarySerializable
        {
            using( var m = new MemoryStream( bytes ) )
            using( var r = new CKBinaryReader( m, Encoding.UTF8, true ) )
            {
                return (T)Activator.CreateInstance( typeof( T ), r )!;
            }
        }

        /// <summary>
        /// Deserializes a <see cref="ICKVersionedBinarySerializable"/> from serialized bytes.
        /// </summary>
        /// <typeparam name="T">The object's type.</typeparam>
        /// <param name="bytes">Serialized bytes.</param>
        /// <returns>The deserialized object.</returns>
        public static T DeserializeVersioned<T>( byte[] bytes ) where T : ICKVersionedBinarySerializable
        {
            using( var m = new MemoryStream( bytes ) )
            using( var r = new CKBinaryReader( m, Encoding.UTF8, true ) )
            {
                return (T)Activator.CreateInstance( typeof( T ), r, r.ReadNonNegativeSmallInt32() )!;
            }
        }

        /// <summary>
        /// Deep clones a <see cref="ICKSimpleBinarySerializable"/> by serializing/deserializing it.
        /// </summary>
        /// <typeparam name="T">The object's type.</typeparam>
        /// <param name="o">The object to clone.</param>
        /// <returns>The clone or null if the object to clone is null.</returns>
        [return: NotNullIfNotNull( "o" )]
        public static T? DeepCloneSimple<T>( T? o ) where T : ICKSimpleBinarySerializable
        {
            if( o is null ) return default;
            using( var s = new MemoryStream() )
            using( var w = new CKBinaryWriter( s, Encoding.UTF8, true ) )
            {
                o.Write( w );
                w.Flush();
                s.Position = 0;
                using( var r = new CKBinaryReader( s, Encoding.UTF8, true ) )
                {
                    return (T)Activator.CreateInstance( typeof( T ), r )!;
                }
            }
        }

        /// <summary>
        /// Deep clones a <see cref="ICKVersionedBinarySerializable"/> by serializing/deserializing it.
        /// </summary>
        /// <typeparam name="T">The object's type.</typeparam>
        /// <param name="o">The object to clone.</param>
        /// <returns>The clone or null if the object to clone is null.</returns>
        [return: NotNullIfNotNull( "o" )]
        public static T? DeepCloneVersioned<T>( T? o ) where T : ICKVersionedBinarySerializable
        {
            if( o is null ) return default;
            using( var s = new MemoryStream() )
            using( var w = new CKBinaryWriter( s, Encoding.UTF8, true ) )
            {
                o.WriteData( w );
                w.Flush();
                s.Position = 0;
                using( var r = new CKBinaryReader( s, Encoding.UTF8, true ) )
                {
                    return (T)Activator.CreateInstance( typeof( T ), r, SerializationVersionAttribute.GetRequiredVersion( o.GetType() ) )!;
                }
            }
        }


    }
}
