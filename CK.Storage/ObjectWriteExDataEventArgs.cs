using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Storage
{
    /// <summary>
    /// Provides data for <see cref="IStructuredWriter.ObjectWriteExData"/> event.
    /// </summary>
    public class ObjectWriteExDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the object written.
        /// </summary>
        public readonly object Obj;

        /// <summary>
        /// Gets the structured writer.
        /// </summary>
        public readonly IStructuredWriter Writer;

        /// <summary>
        /// Initializes a new <see cref="ObjectWriteExDataEventArgs"/>.
        /// </summary>
        /// <param name="w">Structured writer.</param>
        /// <param name="o">Object written.</param>
        public ObjectWriteExDataEventArgs( IStructuredWriter w, object o )
        {
            Obj = o;
            Writer = w;
        }

    }
}
