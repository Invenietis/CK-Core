using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CK.Core
{
    /// <summary>
    /// Extends <see cref="ICKBinaryReader"/> and <see cref="ICKBinaryWriter"/> with
    /// more types.
    /// </summary>
    public static class CKBinaryReaderWriterExtension
    {
        /// <summary>
        /// Reads a <see cref="Range"/>.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <returns>The range.</returns>
        public static Range ReadRange( this ICKBinaryReader r )
        {
            return new Range( r.ReadIndex(), r.ReadIndex() );
        }

        /// <summary>
        /// Writes a <see cref="Range"/>.
        /// </summary>
        /// <param name="w">This writer.</param>
        /// <param name="range">The value to write.</param>
        public static void Write( this ICKBinaryWriter w, Range range )
        {
            w.Write( range.Start );
            w.Write( range.End );
        }

        /// <summary>
        /// Reads a nullable <see cref="Range"/>.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <returns>The (potentially null) range.</returns>
        public static Range? ReadNullableRange( this ICKBinaryReader r )
        {
            return r.ReadBoolean() ? r.ReadRange() : null;
        }

        /// <summary>
        /// Writes a nullable <see cref="Range"/>.
        /// </summary>
        /// <param name="w">This writer.</param>
        /// <param name="range">The value to write.</param>
        public static void WriteNullableRange( this ICKBinaryWriter w, Range? range )
        {
            if( range.HasValue )
            {
                w.Write( true );
                w.Write( range.Value.Start );
                w.Write( range.Value.End );
            }
            else
            {
                w.Write( false );
            }
        }

        /// <summary>
        /// Reads an <see cref="Index"/>.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <returns>The index.</returns>
        public static Index ReadIndex( this ICKBinaryReader r )
        {
            int v = r.ReadInt32();
            return v < 0 ? Index.FromEnd( ~v ) : Index.FromStart( v );
        }

        /// <summary>
        /// Writes an <see cref="Index"/>.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="index">The index.</param>
        public static void Write( this ICKBinaryWriter w, Index index )
        {
            w.Write( index.IsFromEnd ? ~index.Value : index.Value );
        }

        /// <summary>
        /// Reads a nullable <see cref="Index"/>.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <returns>The index.</returns>
        public static Index? ReadNullableIndex( this ICKBinaryReader r )
        {
            return r.ReadBoolean() ? r.ReadIndex() : null; 
        }

        /// <summary>
        /// Writes a nullable <see cref="Index"/>.
        /// </summary>
        /// <param name="w">The writer.</param>
        /// <param name="index">The index.</param>
        public static void WriteNullableIndex( this ICKBinaryWriter w, Index? index )
        {
            if( index.HasValue )
            {
                w.Write( true );
                w.Write( index.Value );
            }
            else
            {
                w.Write( false );
            }
        }

    }
}

