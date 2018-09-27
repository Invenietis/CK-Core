using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Specializes <see cref="BinaryWriter"/> to expose helpers.
    /// </summary>
    public class CKBinaryWriter : BinaryWriter, ICKBinaryWriter
    {
        /// <summary>
        /// Implements a simple object pool that works with its <see cref="CKBinaryReader.ObjectPool{T}"/>
        /// companion.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        public class ObjectPool<T>
        {
            readonly Dictionary<T, int> _pool;
            readonly ICKBinaryWriter _w;

            /// <summary>
            /// Initializes a new object pool for a writer.
            /// </summary>
            /// <param name="w">The binary writer. Must not be null.</param>
            /// <param name="comparer">The comparer to use.</param>
            public ObjectPool( ICKBinaryWriter w, IEqualityComparer<T> comparer = null )
            {
                if( w == null ) throw new ArgumentNullException( nameof( w ) );
                _pool = new Dictionary<T, int>( comparer );
                _w = w;
            }

            /// <summary>
            /// Registers the object if it has not been seen before and returns true: the
            /// actual object must be written.
            /// If the object has already been registered its index is written and false
            /// is returned.
            /// </summary>
            /// <param name="o">The object to write.</param>
            /// <param name="mustWriteMarker">
            /// Be default, '2' is written in the stream whenever the object is registered for the first
            /// time in this pool. Advanced scenarii can use this marker with any byte value greater or
            /// equal to 2.
            /// '1' followed by the object number is written whenever the object has already been handled.
            /// '0' is written for the default value of <typeparamref name="T"/>.
            /// </param>
            /// <returns>
            /// True if the object must be written, false if it has already been and
            /// there is nothing to do.
            /// </returns>
            public bool MustWrite( T o, byte mustWriteMarker = 2 )
            {
                if( EqualityComparer<T>.Default.Equals( o, default( T ) ) )
                {
                    _w.Write( (byte)0 );
                    return false;
                }
                if( _pool.TryGetValue( o, out var num ) )
                {
                    _w.Write( (byte)1 );
                    _w.WriteNonNegativeSmallInt32( num );
                    return false;
                }
                _pool.Add( o, _pool.Count );
                _w.Write( mustWriteMarker );
                return true;
            }

            /// <summary>
            /// Writes a reference to the value or the value itself if it has not been registered yet
            /// thanks to an actual writer function.
            /// </summary>
            /// <param name="o">The value to write.</param>
            /// <param name="actualWriter">
            /// Actual writer. Must not be null.
            /// Note that it will not be called if the value is the default of the <typeparamref name="T"/>:
            /// for reference types, the value will never be null.
            /// </param>
            public void Write( T o, Action<ICKBinaryWriter,T> actualWriter )
            {
                if( MustWrite( o ) ) actualWriter( _w, o );
            }
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryWriter"/> based on the specified
        /// stream and using UTF-8 encoding.
        /// The stream will be closed once this writer is disposed.
        /// </summary>
        /// <param name="output">The output stream.</param>
        public CKBinaryWriter( Stream output )
            : this( output, Encoding.UTF8, false )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryWriter"/> based on the specified
        /// stream and character encoding.
        /// The stream will be closed once this writer is disposed.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public CKBinaryWriter( Stream output, Encoding encoding )
            : this( output, encoding, false )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKBinaryWriter"/> based on the specified
        /// stream and character encoding.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen">true to leave the stream open after this writer object is disposed; otherwise, false.</param>
        public CKBinaryWriter( Stream output, Encoding encoding, bool leaveOpen )
            : base( output, encoding, leaveOpen )
        {
            StringPool = new ObjectPool<string>( this, StringComparer.Ordinal );
        }

        /// <summary>
        /// Gets a string pool, bound to <see cref="StringComparer.Ordinal"/> comparer.
        /// </summary>
        public ObjectPool<string> StringPool { get; }

        /// <summary>
        /// Writes a 32-bit 0 or positive integer in compressed format. See remarks.
        /// </summary>
        /// <param name="value">A 32-bit integer (should not be negative).</param>
        /// <remarks>
        /// Using this method to write a negative integer is the same as using it with a large
        /// positive number: the storage will actually require more than 4 bytes.
        /// It is perfectly valid, except that it is more "expansion" than "compression" :). 
        /// </remarks>
        public void WriteNonNegativeSmallInt32( int value ) => Write7BitEncodedInt( value );

        /// <summary>
        /// Writes a 32-bit integer in compressed format, accomodating rooms for some negative values.
        /// The <paramref name="minNegativeValue"/> simply offsets the written value.
        /// Use <see cref="CKBinaryReader.ReadSmallInt32(int)"/> with the 
        /// same <paramref name="minNegativeValue"/> to read it back.
        /// </summary>
        /// <param name="value">A 32-bit integer (greater or equal to <paramref name="minNegativeValue"/>).</param>
        /// <param name="minNegativeValue">Lowest possible negative value.</param>
        /// <remarks>
        /// <para>
        /// Writing a negative value lower than the <paramref name="minNegativeValue"/> is totally possible, however
        /// more than 4 bytes will be required for them.
        /// </para>
        /// <para>
        /// The default value of -1 is perfect to write small integers that are greater or equal to -1.
        /// </para>
        /// </remarks>
        public void WriteSmallInt32( int value, int minNegativeValue = -1 ) => Write7BitEncodedInt( value - minNegativeValue );

        /// <summary>
        /// Writes a potentially null string.
        /// You can use <see cref="WriteSharedString(string)"/> if the string
        /// has good chances to appear multiple times. 
        /// </summary>
        /// <param name="s">String to write.</param>
        public void WriteNullableString( string s )
        {
            if( s != null )
            {
                Write( true );
                Write( s );
            }
            else Write( false );
        }

        /// <summary>
        /// Writes a string, using the default <see cref="StringPool"/>.
        /// </summary>
        /// <param name="s">The string to write. Can be null.</param>
        public void WriteSharedString( string s )
        {
            if( StringPool.MustWrite( s ) )
            {
                Write( s );
            }
        }

        /// <summary>
        /// Writes a DateTime value.
        /// </summary>
        /// <param name="d">The value to write.</param>
        public void Write( DateTime d )
        {
            Write( d.ToBinary() );
        }

        /// <summary>
        /// Writes a TimeSpan value.
        /// </summary>
        /// <param name="t">The value to write.</param>
        public void Write( TimeSpan t )
        {
            Write( t.Ticks );
        }

        /// <summary>
        /// Writes a DateTimeOffset value.
        /// </summary>
        /// <param name="ds">The value to write.</param>
        public void Write( DateTimeOffset ds )
        {
            Write( ds.DateTime );
            Write( (short)ds.Offset.TotalMinutes );
        }

        /// <summary>
        /// Writes a DateTimeOffset value.
        /// </summary>
        /// <param name="g">The value to write.</param>
        public void Write( Guid g )
        {
            Write( g.ToByteArray() );
        }
    }

}
