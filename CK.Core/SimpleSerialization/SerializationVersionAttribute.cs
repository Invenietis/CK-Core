using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Associates a serialization version to a class or struct.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false )]
    public class SerializationVersionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new version attribute.
        /// </summary>
        /// <param name="version">The version. Must be positive or zero.</param>
        public SerializationVersionAttribute( int version )
        {
            if( version < 0 ) throw new ArgumentException( "Must be 0 or positive.", nameof( version ) );
            Version = version;
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Public helper that retrieves the version on a type or throws an <see cref="InvalidOperationException"/> if the 
        /// attribute is not defined.
        /// </summary>
        /// <param name="t">The type that must be decorated with the SerializationVersion attribute.</param>
        /// <returns>The version number.</returns>
        public static int GetRequiredVersion( Type t )
        {
            var a = (SerializationVersionAttribute?)GetCustomAttribute( t, typeof( SerializationVersionAttribute ) );
            if( a == null ) throw new InvalidOperationException( $"Type '{t}' must be decorated with a [SerializationVersion()] attribute." );
            return a.Version;
        }

        /// <summary>
        /// Public helper that retrieves the version on a type or returns -1 if the attribute is not defined.
        /// </summary>
        /// <param name="t">The type that may be decorated with the SerializationVersion attribute.</param>
        /// <returns>The version number or -1 if it is not defined.</returns>
        public static int TryGetRequiredVersion( Type t )
        {
            var a = (SerializationVersionAttribute?)GetCustomAttribute( t, typeof( SerializationVersionAttribute ) );
            return a == null ? -1 : a.Version;
        }
    }
}
