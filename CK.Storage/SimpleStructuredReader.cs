#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\SimpleStructuredReader.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace CK.Storage
{
    /// <summary>
    /// Factory for <see cref="IStructuredReader"/> implementation.
    /// </summary>
    public static class SimpleStructuredReader
    {

        /// <summary>
        /// Creates a simple (full xml based) <see cref="IStructuredReader"/> instance.
        /// The inner stream will be closed whenever the reader will be disposed.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        /// <returns>A reader bound to the <paramref name="stream"/>.</returns>
        static public IStructuredReader CreateReader( Stream stream, IServiceProvider serviceProvider )
        {
            return CreateReader( stream, serviceProvider, true );
        }

        /// <summary>
        /// Creates a simple (full xml based) <see cref="IStructuredReader"/> instance.
        /// The inner stream will be closed whenever the reader will be disposed.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        /// <param name="throwErrorOnMissingFile">True to throw an exception when the <paramref name="stream"/> parameter is null.</param>
        /// <returns>A reader bound to the <paramref name="stream"/> or null.</returns>
        static public IStructuredReader CreateReader( Stream stream, IServiceProvider serviceProvider, bool throwErrorOnMissingFile )
        {
            if( stream == null )
            {
                if( throwErrorOnMissingFile )
                    throw new CKException( R.FileNotFound );
                else
                    return null;
            }
            XmlReader r = null;
            try
            {
                r = XmlReader.Create( stream, new XmlReaderSettings()
                {
                    CloseInput = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true,
                    DtdProcessing = DtdProcessing.Prohibit,
                    ValidationType = ValidationType.None
                } );
            }
            catch( Exception ex )
            {
                if( r != null ) r.Close();
                throw new CKException( R.InvalidFileManifest, ex );
            }
            ReaderImpl rw = new ReaderImpl( r, serviceProvider, true );
            if( rw.StorageVersion == null )
            {
                rw.Dispose();
                throw new CKException( R.InvalidFileManifestVersion );
            }
            return rw;
        }


    }
}
