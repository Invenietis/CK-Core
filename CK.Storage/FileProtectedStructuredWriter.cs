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

        public FileProtectedStructuredWriter( string path, IServiceProvider ctx, Func<Stream, IServiceProvider, IStructuredWriter> opener )
        {
            _pathNew = _path = path;
            if( File.Exists( _path ) ) _pathNew += ".new";
            StructuredWriter = opener( new FileStream( _pathNew, FileMode.Create ), ctx );
        }

        public IStructuredWriter StructuredWriter { get; private set; }

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
