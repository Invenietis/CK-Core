#nullable enable
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core.Extension
{
    public static class PipeReaderExtensions
    {
        /// <summary>
        /// Asynchronously reads a sequence of bytes which is size is at least <paramref name="minimumByteCount"/> from the current System.IO.Pipelines.PipeReader.
        /// Asking big <paramref name="minimumByteCount"/> will make the circular buffer grow, avoid doing that.
        /// </summary>
        /// <param name="reader">The instance to read from.</param>
        /// <returns></returns>
        /// <remarks>
        /// The buffer size may be smaller if:
        /// <list type="bullet">
        ///     <item>The operation was cancelled by the <paramref name="cancellationToken"/>, you can check it with the flag <see cref="ReadResult.IsCanceled"/>.</item>
        ///     <item>The pipe was completed before reading all asked bytes, you can check it with the flag <see cref="ReadResult.IsCompleted"/>.</item>
        /// </list>
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static async ValueTask<ReadResult> ReadAsync( this PipeReader reader, int minimumByteCount, CancellationToken cancellationToken = default )
        {
            ReadResult result = await reader.ReadAsync( cancellationToken );
            while( result.Buffer.Length < minimumByteCount )//Loop until we get the requested bytes.
            {
                if( result.IsCanceled || result.IsCompleted ) return result;//Completed/Canceled, we can't read any more bytes, so we return.
                //We need to signal that we examined the data, so the next read will fetch more data. If we don't the next read won't wait for more data.
                reader.AdvanceTo( result.Buffer.Start, result.Buffer.End );
                result = await reader.ReadAsync( cancellationToken );
            }
            return result;
        }

        /// <summary>
        /// Represent if the fill was succesfull, or if it's an error.
        /// </summary>
        public enum FillStatus
        {
            /// <summary>
            /// The fill was successfull and the buffer has been clompletly filled.
            /// </summary>
            Done,
            /// <summary>
            /// Operation has been canceled, the buffer is empty or partially filled.
            /// </summary>
            Canceled,
            /// <summary>
            /// Unexpected end of stream, the buffer is empty or partially filled.
            /// </summary>
            UnexpectedEndOfStream
        }

        /// <summary>
        /// Fill the given buffer with the data from the given <see cref="PipeReader"/>
        /// </summary>
        /// <param name="reader">The <see cref="PipeReader"/> to read data from.</param>
        /// <param name="buffer">The buffer to fill.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>An enum representing the status of the the operation.</returns>
        public static async ValueTask<FillStatus> FillBuffer( this PipeReader reader, Memory<byte> buffer, CancellationToken cancellationToken )
        {
            while( true )
            {
                ReadResult result = await reader.ReadAsync( cancellationToken );
                ReadOnlySequence<byte> readBuffer = result.Buffer;
                if( readBuffer.Length > buffer.Length ) readBuffer = readBuffer.Slice( 0, buffer.Length );//slice the pipe buffer if bigger than target.
                readBuffer.CopyTo( buffer.Span );//copy data to the input buffer.
                buffer = buffer.Slice( (int)readBuffer.Length );//then truncate the input buffer, so we don't overwrite our data.
                reader.AdvanceTo( readBuffer.End );//and don't forget to signal that we consumed the data.
                if( buffer.Length == 0 ) return FillStatus.Done;
                if( result.IsCanceled || cancellationToken.IsCancellationRequested ) return FillStatus.Canceled;
                if( result.IsCompleted ) return FillStatus.UnexpectedEndOfStream;
            }
        }
    }
}
