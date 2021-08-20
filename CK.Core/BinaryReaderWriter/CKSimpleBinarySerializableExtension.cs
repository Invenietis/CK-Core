using System;
using System.IO;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Extends <see cref="ICKSimpleBinarySerializable"/> to offer deep cloning.
    /// </summary>
    public static class CKSimpleBinarySerializableExtension
    {
        /// <summary>
        /// Deep clones this object by using its binary serialization/deserialization.
        /// The <typeparamref name="T"/> must be the runtime type of <paramref name="this"/>
        /// otherwise an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <typeparam name="T">The type of the </typeparam>
        /// <param name="this">The object to clone.</param>
        /// <returns>A deep clone.</returns>
        public static T DeepClone<T>( this T @this ) where T : ICKSimpleBinarySerializable
        {
            if( @this == null ) throw new ArgumentNullException( nameof( @this ) );
            if( typeof( T ) != @this.GetType() ) throw new ArgumentException( $"Type parameter '{typeof( T )}' must be the same as the runtime type '{@this.GetType()}'." );
            using( var m = new MemoryStream() )
            using( var w = new CKBinaryWriter( m, Encoding.Default, true ) )
            using( var r = new CKBinaryReader( m, Encoding.Default, true ) )
            {
                @this.Write( w );
                w.Flush();
                m.Position = 0;
                return (T)Activator.CreateInstance( typeof( T ), new object[] { r } );
            }
        }

    }

}
