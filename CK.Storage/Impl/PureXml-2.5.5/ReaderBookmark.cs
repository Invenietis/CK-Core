#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\Impl\PureXml-2.5.5\ReaderBookmark.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using CK.Core;
using System.Collections.Generic;

namespace CK.Storage
{
    /// <summary>
    /// Implementation of <see cref="IStructuredReaderBookmark"/> for <see cref="SimpleStructuredReader"/>.
    /// </summary>
    internal sealed class ReaderBookmark : IStructuredReaderBookmark
    {
        XmlParserContext _xmlContext;
        Version _storageVersion;
        string _skippedFragment;

        internal ReaderBookmark( IStructuredReader r )
        {
            _storageVersion = r.StorageVersion;
            _xmlContext = new XmlParserContext( r.Xml.NameTable, null, r.Xml.XmlLang, r.Xml.XmlSpace );
            _skippedFragment = r.Xml.ReadOuterXml();
        }

        public IStructuredReader Restore( IServiceProvider baseServiceProvider )
        {
            XmlReader r = new XmlTextReader( _skippedFragment, XmlNodeType.Element, _xmlContext );
            ReaderImpl reader = new ReaderImpl( r, baseServiceProvider, _storageVersion, true );
            return reader;
        }

        public void WriteBack( IStructuredWriter w )
        {
            w.Xml.WriteRaw( _skippedFragment );
        }

    }
}
