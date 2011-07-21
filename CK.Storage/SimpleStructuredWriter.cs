using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using CK.Core;
using System.Diagnostics;

namespace CK.Storage
{
    /// <summary>
    /// Factory for <see cref="IStructuredWriter"/> implementation.
    /// </summary>
    public static class SimpleStructuredWriter
    {
        /// <summary>
        /// Creates an opened standard <see cref="SimpleStructuredWriter"/>.
        /// The inner stream will be closed whenever the writer will be disposed.
        /// </summary>
        /// <param name="stream">Underlying stream.</param>
        /// <param name="baseServiceProvider">Optional <see cref="IServiceProvider"/>.</param>
        /// <returns>An opened, ready to use, <see cref="SimpleStructuredWriter"/> (that must be disposed once done).</returns>
        static public IStructuredWriter CreateWriter( Stream stream, IServiceProvider baseServiceProvider )
        {
            MissingDisposeCallSentinel.DebugCheckMissing( s => Debug.Fail( s ) );
            XmlWriter w = XmlWriter.Create( stream, new XmlWriterSettings() { CheckCharacters = true, Indent = true, CloseOutput = true } );
            return new WriterImpl( w, baseServiceProvider, true, true );
        }

    }
}
