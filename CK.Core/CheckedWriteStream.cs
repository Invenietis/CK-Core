using System;
using System.IO;

namespace CK.Core
{
    /// <summary>
    /// A checker stream is a writable stream that compares the bytes written
    /// into it with an existing content. Once the new bytes are written, <see cref="GetResult"/>
    /// retrieves the conclusion.
    /// </summary>
    public abstract class CheckedWriteStream : Stream
    {
        /// <summary>
        /// Returned value of <see cref="CheckedWriteStream.GetResult()"/>.
        /// </summary>
        public enum Result
        {
            /// <summary>
            /// No differences.
            /// </summary>
            None,

            /// <summary>
            /// The byte at <see cref="Stream.Position"/> differs.
            /// </summary>
            HasByteDifference,

            /// <summary>
            /// The written bytes overflow the reference bytes.
            /// </summary>
            LongerThanRefBytes,

            /// <summary>
            /// The written bytes are shorter than the reference bytes.
            /// </summary>
            ShorterThanRefBytes
        }

        /// <summary>
        /// Gets or sets a flag that triggers a throw <see cref="ArgumentException"/> when a
        /// byte differs (<see cref="Result.HasByteDifference"/>) or there are more written bytes
        /// than the reference bytes (<see cref="Result.LongerThanRefBytes"/>).
        /// When there are less written bytes than the reference bytes (<see cref="Result.ShorterThanRefBytes"/>)
        /// the exception is thrown by <see cref="GetResult"/>.
        /// <para>
        /// This default to false.
        /// </para>
        /// </summary>
        public abstract bool ThrowArgumentException { get; set; }

        /// <summary>
        /// Gets the final result once all the bytes have been written into this checker.
        /// This throws a <see cref="ArgumentException"/> if <see cref="ThrowArgumentException"/> is true
        /// and there are less written bytes than the reference bytes (<see cref="Result.ShorterThanRefBytes"/>).
        /// </summary>
        /// <returns>The final result.</returns>
        public abstract Result GetResult();
    }


}
