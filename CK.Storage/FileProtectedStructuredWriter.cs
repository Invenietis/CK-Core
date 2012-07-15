using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CK.Storage
{
    /// <summary>
    /// Implementation of <see cref="IProtectedStructuredWriter"/> for files.
    /// </summary>
    public class FileProtectedStructuredWriter : IProtectedStructuredWriter
    {
        string _path;
        string _pathNew;

        /// <summary>
        /// Initializes a new <see cref="FileProtectedStructuredWriter"/>.
        /// Actual changes will be effective in <paramref name="path"/> only when <see cref="SaveChanges"/> is called.
        /// </summary>
        /// <param name="path">Path of the file to write to.</param>
        /// <param name="ctx">Services provider.</param>
        /// <param name="opener">Function that actually opens a stream as a <see cref="IStructuredWriter"/>.</param>
        public FileProtectedStructuredWriter( string path, IServiceProvider ctx, Func<Stream, IServiceProvider, IStructuredWriter> opener )
        {
            _pathNew = _path = path;
            if( File.Exists( _path ) ) _pathNew += ".new";
            StructuredWriter = opener( new FileStream( _pathNew, FileMode.Create ), ctx );
        }

        /// <summary>
        /// Gets the <see cref="IStructuredWriter"/>.
        /// </summary>
        public IStructuredWriter StructuredWriter { get; private set; }

        /// <summary>
        /// Atomically saves the changes and dispose the <see cref="StructuredWriter"/> (this method 
        /// must be called only once, any subsequent calls are ignored).
        /// </summary>
        public void SaveChanges()
        {
            if( StructuredWriter != null )
            {
                StructuredWriter.Dispose();
                StructuredWriter = null;
                if( _pathNew != _path ) File.Replace( _pathNew, _path, _path + ".bak" );
            }
        }

        void IDisposable.Dispose()
        {
            if( StructuredWriter != null ) StructuredWriter.Dispose();
        }
    }
}
