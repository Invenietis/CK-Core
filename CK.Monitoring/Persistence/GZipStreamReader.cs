#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\Persistence\GZipStreamReader.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.IO;
using System.IO.Compression;
using System.Runtime;
using System.Security.Permissions;

namespace CK.Monitoring
{
    internal class GZipStreamReader : Stream
    {
        readonly GZipStream _stream;
        long _position;

        public GZipStreamReader( Stream stream )
        {
            _stream = new GZipStream( stream, CompressionMode.Decompress );
        }

        public override IAsyncResult BeginRead( byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState )
        {
            return _stream.BeginRead( array, offset, count, asyncCallback, asyncState );
        }

        public override IAsyncResult BeginWrite( byte[] array, int offset, int count, AsyncCallback asyncCallback, object asyncState )
        {
            throw new NotSupportedException();
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing ) _stream.Close();
            base.Dispose( disposing );
        }
        
        public override int EndRead( IAsyncResult asyncResult )
        {
            int read = _stream.EndRead( asyncResult );
            _position += read;
            return read;
        }
        
        public override void EndWrite( IAsyncResult asyncResult )
        {
        }

        public override void Flush()
        {
            _stream.Flush();
        }
        
        public override int Read( byte[] array, int offset, int count )
        {
            int read = _stream.Read( array, offset, count );
            _position += read;
            return read;
        }
        
        public override long Seek( long offset, SeekOrigin origin )
        {
            return (_position = _stream.Seek( offset, origin ));
        }

        public override void SetLength( long value )
        {
            throw new NotSupportedException();
        }

        public override void Write( byte[] array, int offset, int count )
        {
            throw new NotSupportedException();
        }

        public Stream BaseStream { get { return _stream.BaseStream; } }

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return _stream.CanSeek; } }
      
        public override bool CanWrite { get { return false; } }
        
        public override long Length { get { throw new NotSupportedException(); } }

        public override long Position
        {
            get { return _position; }
            set { throw new NotSupportedException(); }
        }
    }
}

