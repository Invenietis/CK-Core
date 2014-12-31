using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CK.Core
{
    public static class SystemStreamExtensions
    {
        /// <summary>
        /// Reads the bytes from the current stream and writes them to another stream.
        /// </summary>
        /// <param name="this">The current stream.</param>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <exception cref="System.ArgumentNullException">destination is null.</exception>
        /// <exception cref="System.ObjectDisposedException">
        /// Either the current stream or destination were closed before the System.IO.Stream.CopyTo(System.IO.Stream) method was called.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The current stream does not support reading, or the destination stream does not support writing.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurred.
        /// </exception>
        public static void CopyTo( this Stream @this, Stream destination )
        {
            if( destination == null )
            {
                throw new ArgumentNullException( "destination" );
            }
            if( !@this.CanRead && !@this.CanWrite )
            {
                throw new ObjectDisposedException( null, "Either the current stream or destination were closed before the System.IO.Stream.CopyTo(System.IO.Stream) method was called." );
            }
            if( !destination.CanRead && !destination.CanWrite )
            {
                throw new ObjectDisposedException( "destination", "Either the current stream or destination were closed before the System.IO.Stream.CopyTo(System.IO.Stream) method was called." );
            }
            if( !@this.CanRead )
            {
                throw new NotSupportedException( "The current stream does not support reading." );
            }
            if( !destination.CanWrite )
            {
                throw new NotSupportedException( "The destination stream does not support writing." );
            }
            @this.InternalCopyTo( destination, 4096 );
        }

        /// <summary>
        /// Reads the bytes from the current stream and writes them to another stream, using a specified buffer size.
        /// </summary>
        /// <param name="this">The current stream.</param>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">The size of the buffer. This value must be greater than zero. The default size is 4096.</param>
        /// <exception cref="System.ArgumentNullException">destination is null.</exception>
        /// <exception cref="System.ObjectDisposedException">
        /// Either the current stream or destination were closed before the System.IO.Stream.CopyTo(System.IO.Stream) method was called.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The current stream does not support reading, or the destination stream does not support writing.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An I/O error occurred.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// bufferSize is negative or zero.
        /// </exception>
        public static void CopyTo( this Stream @this, Stream destination, int bufferSize )
        {
            if( destination == null )
            {
                throw new ArgumentNullException( "destination" );
            }
            if( bufferSize <= 0 )
            {
                throw new ArgumentOutOfRangeException( "bufferSize", "bufferSize is negative or zero." );
            }
            if( !@this.CanRead && !@this.CanWrite )
            {
                throw new ObjectDisposedException( null, "Either the current stream or destination were closed before the System.IO.Stream.CopyTo(System.IO.Stream) method was called." );
            }
            if( !destination.CanRead && !destination.CanWrite )
            {
                throw new ObjectDisposedException( "destination", "Either the current stream or destination were closed before the System.IO.Stream.CopyTo(System.IO.Stream) method was called." );
            }
            if( !@this.CanRead )
            {
                throw new NotSupportedException( "The current stream does not support reading." );
            }
            if( !destination.CanWrite )
            {
                throw new NotSupportedException( "The destination stream does not support writing." );
            }
            @this.InternalCopyTo( destination, bufferSize );
        }

        private static void InternalCopyTo( this Stream @this, Stream destination, int bufferSize )
        {
            int num;
            byte[] buffer = new byte[bufferSize];
            while( (num = @this.Read( buffer, 0, buffer.Length )) != 0 )
            {
                destination.Write( buffer, 0, num );
            }
        }
    }
}
