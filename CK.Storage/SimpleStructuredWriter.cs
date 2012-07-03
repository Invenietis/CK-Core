#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\SimpleStructuredWriter.cs) is part of CiviKey. 
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
            XmlWriter w = XmlWriter.Create( stream, new XmlWriterSettings() { CheckCharacters = true, Indent = true, CloseOutput = true } );
            return new WriterImpl( w, baseServiceProvider, true, true );
        }

    }
}
