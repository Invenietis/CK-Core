#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\FileProtectedStructuredWriter.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
    public sealed class FileProtectedStructuredWriter : IProtectedStructuredWriter
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

        /// <summary>
        /// Close the currently opened file if required.
        /// If <see cref="SaveChanges"/> has not been called, the original file is left unchanged.
        /// </summary>
        public void Dispose()
        {
            if( StructuredWriter != null ) StructuredWriter.Dispose();
        }
    }
}
