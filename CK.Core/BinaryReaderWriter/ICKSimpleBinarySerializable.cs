using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Basic interface for simple binary serialization support.
    /// A deserialization constructor must be implemented (that accepts a <see cref="ICKBinaryReader"/>).
    /// <para>
    /// Simple serialization means that there is no support for object graph (no reference management),
    /// no support for polymorphism (the exact type must be known) and that versions must be manually managed.
    /// </para>
    /// </summary>
    public interface ICKSimpleBinarySerializable
    {
        /// <summary>
        /// Serializes this object into the writer.
        /// There should be a version written first (typically a byte): the deserialization
        /// constructor must read this version first.
        /// </summary>
        /// <param name="w"></param>
        void Write( ICKBinaryWriter w );
    }
}
