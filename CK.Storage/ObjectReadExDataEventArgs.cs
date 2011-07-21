using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Storage
{
    /// <summary>
    /// Provides data for <see cref="IStructuredReader.ObjectReadExData"/> event.
    /// </summary>
    public class ObjectReadExDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the object read.
        /// </summary>
        public readonly object Obj;

        /// <summary>
        /// Gets the structured reader. The reader is postionned on
        /// the element.
        /// </summary>
        public readonly IStructuredReader Reader;

        /// <summary>
        /// Gets or sets whether the extra element has been read.
        /// It must be set to true as soon as the <see cref="IStructuredReader.Xml"/> reader
        /// has been forwarded.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ObjectWriteExDataEventArgs"/>.
        /// </summary>
        /// <param name="r">Structured reader.</param>
        /// <param name="o">Object read.</param>
        public ObjectReadExDataEventArgs( IStructuredReader r, object o )
        {
            Obj = o;
            Reader = r;
        }

    }

}
