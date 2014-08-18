#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\File\TemporaryFile.cs) is part of CiviKey. 
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
using System.IO;

namespace CK.Core
{
	/// <summary>
	/// Small helper to automatically delete a temporary file. 
    /// It is mainly a secure wrapper around <see cref="System.IO.Path.GetTempFileName">GetTempFileName</see> that 
    /// creates a uniquely named, zero-byte temporary file on disk and returns the full path of that file: the <see cref="P:Path"/>
    /// property exposes it.
	/// </summary>
	public sealed class TemporaryFile : IDisposable
	{
		string _path;

        /// <summary>
        /// Initializes a new short lived <see cref="TemporaryFile"/>.
        /// </summary>
		public TemporaryFile() 
			: this( true )
		{
		}

        /// <summary>
        /// Initializes a new short lived <see cref="TemporaryFile"/>.
        /// When <paramref name="shortLived"/> is true, the <see cref="FileAttributes.Temporary"/> is set on the file.
        /// </summary>
        /// <param name="shortLived">True to set the <see cref="FileAttributes.Temporary"/> on the file.</param>
        public TemporaryFile( bool shortLived )
            : this( shortLived, null )
        {
        }

        /// <summary>
        /// Initializes the TemporaryFile with an extension - the file will have a name looking like : xxxx.tmp.extension        
        /// </summary>
        /// <param name="extension">The extension of the file (example : '.png' and 'png' would both work) </param>
        public TemporaryFile( string extension )
            : this( true, extension )
        {
        }

        /// <summary>
        /// Initializes a new short lived <see cref="TemporaryFile"/> with an extension.
        /// When <paramref name="shortLived"/> is true, the <see cref="FileAttributes.Temporary"/> is set on the file.
        /// The file will have a name looking like : xxxx.tmp.extension
        /// </summary>
        /// <param name="shortLived">True to set the <see cref="FileAttributes.Temporary"/> on the file.</param>
        /// <param name="extension">Optional extension of the file (example : '.png' and 'png' would both work).</param>
        /// <remarks>
        /// When extension is ".", the final path will end with a ".".
        /// </remarks>
        public TemporaryFile( bool shortLived, string extension )
        {
            _path = System.IO.Path.GetTempFileName();
            if( !String.IsNullOrWhiteSpace( extension ) )
            {
                string origPath = _path;
                if( extension[0] == '.' ) _path += extension;
                else _path += '.' + extension;
                File.Move( origPath, _path );
            }
            if( shortLived ) File.SetAttributes( _path, FileAttributes.Temporary );
        }

        /// <summary>
        /// Finalizer attempts to delete the file.
        /// </summary>
		~TemporaryFile()
		{
			DeleteFile();
		}

        /// <summary>
        /// Gets the complete file path of the temporary file.
        /// It is <see cref="String.Empty"/> when the file has been <see cref="Detach"/>ed.
        /// The file is not opened but exists, initially empty.
        /// </summary>
		public string Path
		{
			get 
            {
                var p = _path;
                if( p == null ) throw new ObjectDisposedException( "TemporaryFile" );
                return p; 
            }
		}

        /// <summary>
        /// Gets whether the temporary file is detached (its <see cref="Path"/> is <see cref="String.Empty"/>).
        /// </summary>
        public bool IsDetached
        {
            get { return Path.Length == 0; }
        }

        /// <summary>
        /// Detaches the temporary file: it will no more be automatically destroyed.
        /// </summary>
		public void Detach()
		{
            var p = _path;
            if( p == null ) throw new ObjectDisposedException( "TemporaryFile" );
            _path = String.Empty;
		}

        /// <summary>
        /// Attempts to delete the temporary file.
        /// </summary>
		public void Dispose()
		{
			if( DeleteFile() ) GC.SuppressFinalize(this);
		}

		private bool DeleteFile()
		{
            var p = _path;
			if( p != null )
			{
                if( p.Length == 0 ) _path = null;
                else
                {
                    try { File.Delete( p ); _path = null; }
                    catch { return false; }
                }
                return true;
			}
            return false;
		}
	}
}
