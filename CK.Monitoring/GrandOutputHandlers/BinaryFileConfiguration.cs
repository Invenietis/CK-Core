#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\GrandOutputHandlers\BinaryFileConfiguration.cs) is part of CiviKey. 
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
using System.Linq;
using System.Xml.Linq;
using CK.RouteConfig;
using CK.Core;
using System.IO;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Configuration object for <see cref="BinaryFile"/>.
    /// </summary>
    [HandlerType( typeof(BinaryFile) )]
    public class BinaryFileConfiguration : HandlerConfiguration
    {
        /// <summary>
        /// Initializes a new <see cref="BinaryFileConfiguration"/>.
        /// </summary>
        /// <param name="name">Name of this configuration.</param>
        public BinaryFileConfiguration( string name )
            : base( name )
        {
            MaxCountPerFile = 20000;
            FileBufferSize = 4096;
            UseGzipCompression = false;
        }

        /// <summary>
        /// Gets or sets the path of the file. When not rooted (see <see cref="System.IO.Path.IsPathRooted"/>),
        /// it is a sub path in <see cref="SystemActivityMonitor.RootLogPath"/>.
        /// It defaults to null: it must be specified.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the maximal count of entries per file.
        /// Defaults to 20000.
        /// </summary>
        public int MaxCountPerFile { get; set; }

        /// <summary>
        /// Gets or sets whether files will be opened with <see cref="FileOptions.WriteThrough"/>.
        /// Defaults to false.
        /// </summary>
        public bool FileWriteThrough { get; set; }

        /// <summary>
        /// Gets or sets the buffer size used to write files.
        /// Defaults to 4096.
        /// </summary>
        public int FileBufferSize { get; set; }

        /// <summary>
        /// Gets or sets whether to use Gzip compression after closing log files.
        /// Defaults to false.
        /// </summary>
        public bool UseGzipCompression { get; set; }

        /// <summary>
        /// Initializes (or reinitializes) this <see cref="BinaryFileConfiguration"/> from a <see cref="XElement"/>.
        /// </summary>
        /// <param name="monitor">Monitor to report errors or warnings.</param>
        /// <param name="xml">Source XML element.</param>
        protected override void Initialize( IActivityMonitor monitor, XElement xml )
        {
            Path = xml.AttributeRequired( "Path" ).Value;
            MaxCountPerFile = xml.GetAttributeInt( "MaxCountPerFile", MaxCountPerFile );
            UseGzipCompression = xml.GetAttributeBoolean( "UseGzipCompression", UseGzipCompression );
        }
    }
}
